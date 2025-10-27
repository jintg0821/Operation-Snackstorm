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

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.gameObject.CompareTag("Player") && player.isAttacking)
        {
            PlayerController hitPlayer = other.gameObject.GetComponent<PlayerController>();
            if (hitPlayer != null && hitPlayer.photonView.Owner != player.photonView.Owner)
            {
                if (!hitPlayer.isHit)
                {
                    hitPlayer.Hit();
                }
            }
        }
    }
}
