using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObj : MonoBehaviourPun
{
    public Item item;

    [PunRPC]
    public void RPC_RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"Destroying {gameObject.name}, ViewID: {photonView.ViewID}");
            PhotonNetwork.Destroy(gameObject);
        }
        gameObject.SetActive(false);
    }
}