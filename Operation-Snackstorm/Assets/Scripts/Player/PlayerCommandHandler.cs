using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommandHandler : MonoBehaviourPun
{
    private CommandType currentCommand;
    private bool isBeingChased;

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
        OnCommandFailed();
    }

    private bool IsFollowingCommand()
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null) return false;

        switch (currentCommand)
        {
            case CommandType.Walk:
                return playerMovement.currentState == PlayerState.Walk;

            case CommandType.Run:
                return playerMovement.currentState == PlayerState.Run;

            case CommandType.Crouch:
                return playerMovement.currentState == PlayerState.Crouch;
        }

        return false;
    }

    private void OnCommandFailed()
    {
        Debug.Log($"명령 위반! NPC들이 쫓습니다.");
    }

    private void OnCommandSuccess()
    {
        Debug.Log($"명령 수행 성공!");
    }
}
