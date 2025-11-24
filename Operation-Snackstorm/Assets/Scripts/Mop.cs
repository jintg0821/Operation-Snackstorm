using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mop : MonoBehaviourPun
{
    [SerializeField] private PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    [PunRPC]
    void RPC_MopHit(int hitViewID)
    {
        PhotonView hitPV = PhotonView.Find(hitViewID);
        PlayerController hitPlayer = hitPV?.GetComponent<PlayerController>();
        if (hitPlayer != null && !hitPlayer.isHit)
            hitPlayer.Hit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.gameObject.CompareTag("Player") && player.isAttacking)
        {
            PhotonView hitPV = other.GetComponent<PhotonView>();
            if (hitPV != null && hitPV.ViewID != photonView.ViewID)
            {
                photonView.RPC("RPC_MopHit", RpcTarget.All, hitPV.ViewID);
            }
        }

        if (other.gameObject.CompareTag("Dirty") && player.isMopping)
        {
            PhotonView dirtyPV = other.GetComponent<PhotonView>();
            if (dirtyPV != null)
            {
                dirtyPV.RPC("RPC_RequestDirtyDestroy", RpcTarget.MasterClient);
            }
        }
    }
}