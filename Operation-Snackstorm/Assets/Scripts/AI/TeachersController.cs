using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachersController : AIController
{
    public override void OnCatchTarget(PlayerController player)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView playerPV = player.photonView;
        if (playerPV != null)
        {
            var inventory = player.inventory;

            if (inventory.items.Count > 0)
            {
                int randNum = Random.Range(0, inventory.items.Count);
                inventory.RemoveItem(inventory.items[randNum]);
            }

            playerPV.RPC("RPC_SetCatchable", playerPV.Owner, false);

            StartCoroutine(ResetCatchableAfterDelay(playerPV));
        }
    }

    private IEnumerator ResetCatchableAfterDelay(PhotonView playerPV)
    {
        yield return new WaitForSeconds(3f);
        playerPV.RPC("RPC_SetCatchable", playerPV.Owner, true);
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
