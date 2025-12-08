using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mop : MonoBehaviourPun
{
    [SerializeField] private PlayerController player;
    [SerializeField] private bool isDirtyMop;
    [SerializeField] private GameObject dirtyMop;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void DirtyMop(bool dirty)
    {
        photonView.RPC("RPC_DirtyMop", RpcTarget.AllBuffered, dirty);
    }

    [PunRPC]
    void RPC_DirtyMop(bool dirty)
    {
        dirtyMop.SetActive(dirty);
        isDirtyMop = dirty;
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
            if (hitPV != null && hitPV.ViewID != player.photonView.ViewID)
            {
                photonView.RPC("RPC_MopHit", RpcTarget.All, hitPV.ViewID);
            }
        }

        if (other.gameObject.CompareTag("Dirty") && player.isMopping)
        {
            PhotonView dirtyPV = other.GetComponent<PhotonView>();
            if (dirtyPV != null)
            {
                if (isDirtyMop)
                {
                    dirtyPV.RPC("RPC_InstantiateDirty", RpcTarget.MasterClient);
                }
                else
                {
                    dirtyPV.RPC("RPC_RequestDirtyDestroy", RpcTarget.MasterClient);
                    DirtyMop(true);
                }
            }
        }
    }
}