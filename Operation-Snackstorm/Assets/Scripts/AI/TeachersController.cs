using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachersController : AIController
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
