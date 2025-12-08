using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachersController : AIController
{

    public override void CheckSight()
    {
        if (isSightRestricted)
        {
            target = null;
            if (currentState == AIState.Chase) currentState = AIState.Patrol;
            return;
        }

        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        bool seeTarget = false;
        bool runPlayer = false;

        foreach (Collider targetCol in targets)
        {
            PlayerController player = targetCol.GetComponent<PlayerController>();
            PlayerMovement playerState = targetCol.GetComponent<PlayerMovement>();
            if (player != null && !player.isCatchable) continue;

            Transform targetTransform = targetCol.transform;
            Vector3 dirToTarget = (targetTransform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
            {
                float distance = Vector3.Distance(transform.position, targetTransform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distance, obstacleMask))
                {
                    seeTarget = true;
                    if (playerState.currentState == PlayerState.Run)
                    {
                        target = target = targetCol.transform;
                        runPlayer = true;
                    }
                    break;
                }
            }
        }

        if (runPlayer && seeTarget && chaseAI)  // 시야에 플레이어가 있다면
        {
            currentState = AIState.Chase;   // 추적 상태
        }
        else
        {
            if (currentState != AIState.Chase)  //시야에 플레이어가 없으며 추적 상태가 아니라면
            {
                currentState = AIState.Patrol;  // 순찰 상태
            }
        }
    }

    public override void OnCatchTarget(PlayerController player)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (player == null)
        {
            Debug.Log("Player null");
            return;
        }

        PhotonView playerPV = player.photonView;
        if (playerPV != null)
        {
            playerPV.RPC("RPC_RemoveRandomItemFromInventory", playerPV.Owner);

            playerPV.RPC("RPC_SetCatchable", RpcTarget.All, false);
            StartCoroutine(ResetCatchableAfterDelay(playerPV));
            playerPV.RPC("RPC_GetMinusPoint", playerPV.Owner, 1);
            AIController[] aIControllers = GameManager.Instance.aiList.ToArray();
            foreach (var ai in aIControllers)
            {
                if (ai.isBroadcasting)
                {
                    isBroadcasting = false;
                    target = null;
                    ai.currentState = AIState.Patrol;
                }
            }
        }
    }

    

    private IEnumerator ResetCatchableAfterDelay(PhotonView playerPV)
    {
        yield return new WaitForSeconds(3f);
        playerPV.RPC("RPC_SetCatchable", RpcTarget.All, true);
    }

    [PunRPC]
    private void RPC_HandleCatch(int playerViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null)
        {
            PlayerController player = playerPV.GetComponent<PlayerController>();
            if (player != null && player.isCatchable)
            {
                OnCatchTarget(player);
            }
        }
    }

}
