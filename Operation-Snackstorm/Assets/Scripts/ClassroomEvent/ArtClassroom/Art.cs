using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Art : MonoBehaviourPun
{
    [PunRPC]
    public void RPC_RequestArtDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
