using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviourPun
{
    public GameObject leftObj;
    public GameObject rightObj;

    public Vector3 closedLeftRot;
    public Vector3 openLeftRot;
    public Vector3 closedRightRot;
    public Vector3 openRightRot;

    public float rotateSpeed = 2f;

    public bool isOpen = false;

    void Update()
    {
        if (leftObj != null)
        {
            Quaternion targetRot = Quaternion.Euler(isOpen ? openLeftRot : closedLeftRot);
            leftObj.transform.localRotation = Quaternion.Slerp(leftObj.transform.localRotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (rightObj != null)
        {
            Quaternion targetRot = Quaternion.Euler(isOpen ? openRightRot : closedRightRot);
            rightObj.transform.localRotation = Quaternion.Slerp(rightObj.transform.localRotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    public void ToggleDoor()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_ToggleDoor", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_ToggleDoor()
    {
        isOpen = !isOpen;
    }

    public void CloseDoorAfterDelay(float delay)
    {
        if (photonView.IsMine)
        {
            StartCoroutine(CloseDoorCoroutine(delay));
        }
    }

    private IEnumerator CloseDoorCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isOpen && photonView.IsMine)
        {
            photonView.RPC("RPC_ToggleDoor", RpcTarget.AllBuffered);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isOpen);
        }
        else
        {
            isOpen = (bool)stream.ReceiveNext();
        }
    }
}