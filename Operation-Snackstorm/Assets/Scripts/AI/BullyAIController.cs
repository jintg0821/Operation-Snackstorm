using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullyAIController : AIController
{
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
            player.SubtractCoin(15);
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

        StudentChatController chatController = GetComponent<StudentChatController>();
        if (chatController != null)
        {
            chatController.OnCatchChat();
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
