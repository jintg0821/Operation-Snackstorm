using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommandHandler : MonoBehaviourPun
{
    private CommandType currentCommand;
    private bool isBeingChased;

    private PlayerController playerController;
    private PlayerMovement playerMovement;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void ReceiveCommand(CommandType command)
    {
        if (!photonView.IsMine) return;

        currentCommand = command;
        isBeingChased = false;
        Debug.Log($"[{photonView.Owner.NickName}] 명령 수신 : {command}");
        StartCoroutine(CheckCommandCompliance());
    }

    private IEnumerator CheckCommandCompliance()
    {
        float totalDuration = BroadcastManager.Instance.commandDuration;
        float requiredDuration = 3f;
        float successTimer = 0f;

        while (totalDuration > 0)
        {
            if (IsFollowingCommand())
            {
                successTimer += Time.deltaTime;

                if (successTimer >= requiredDuration)
                {
                    OnCommandSuccess();
                    yield break;
                }
            }
            else
            {
                successTimer = 0f;
            }

            totalDuration -= Time.deltaTime;
            yield return null;
        }

        isBeingChased = true;
        BroadcastManager.Instance.CommandText("실패했습니다", 4f);
        photonView.RPC("RPC_OnCommandFailed", RpcTarget.All);
    }

    private bool IsFollowingCommand()
    {
        if (playerMovement == null) return false;

        switch (currentCommand)
        {
            case CommandType.Walk:
                return playerMovement.currentState == PlayerState.Walk;

            case CommandType.Run:
                return playerMovement.currentState == PlayerState.Run;

            case CommandType.Idle:
                return playerMovement.currentState == PlayerState.Idle;
        }

        return false;
    }

    [PunRPC]
    public void RPC_OnCommandFailed()
    {
        OnCommandFailed();
    }

    private void OnCommandFailed()
    {
        AIController[] aIControllers = GameManager.Instance.aiList.ToArray();

        foreach (var ai in  aIControllers)
        {
            ai.isBroadcasting = true;
            ai.target = this.gameObject.transform;
            ai.currentState = AIState.Chase;
        }
    }

    private void OnCommandSuccess()
    {
        playerController.GetBonusPoint(10);

        BroadcastManager.Instance.CommandText("성공했습니다", 4f);
    }
}
