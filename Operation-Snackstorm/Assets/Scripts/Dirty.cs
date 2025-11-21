using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirty : MonoBehaviourPun
{
    [PunRPC]
    public void RPC_RequestDirtyDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
