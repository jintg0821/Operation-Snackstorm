using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObj : MonoBehaviourPun
{
    public Item item;
    private Rigidbody rb;
    private Collider col;

    [SerializeField] private bool isHeld = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetHeld(bool held)
    {
        if (isHeld == held) return;
        isHeld = held;

        photonView.RPC("RPC_SetHeld", RpcTarget.AllBuffered, held);
    }

    [PunRPC]
    void RPC_SetHeld(bool held)
    {
        isHeld = held;
        rb.isKinematic = held;
        rb.useGravity = !held;
        col.enabled = !held;

        if (held)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    [PunRPC]
    void RPC_Throw(Vector3 velocity)
    {
        SetHeld(false);

        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;

        rb.velocity = velocity;
    }

    void ApplyPhysicsState()
    {
        if (rb == null || col == null) return;

        if (isHeld)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            col.enabled = false;
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            col.enabled = true;
        }
    }

    [PunRPC]
    public void RPC_RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}