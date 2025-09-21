using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    private int agentsInRange = 0;

    private NavMeshObstacle obstacle;

    void Start()
    {
        //obstacle = GetComponent<NavMeshObstacle>();
        //if (obstacle != null)
        //{
        //    obstacle.carving = !isOpen;
        //}
    }

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

    public void AgentEntered()
    {
        if (photonView.IsMine)
        {
            agentsInRange++;
            if (agentsInRange == 1 && !isOpen)
            {
                ToggleDoor();
            }
        }
    }

    public void AgentExited()
    {
        if (photonView.IsMine)
        {
            agentsInRange--;
            if (agentsInRange <= 0)
            {
                agentsInRange = 0;
                if (isOpen)
                {
                    StartCoroutine(CloseDoorCoroutine(3f));
                }
            }
        }
    }

    public void ToggleDoor()
    {
        photonView.RPC("RPC_ToggleDoor", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ToggleDoor()
    {
        isOpen = !isOpen;
        //if (obstacle != null)
        //{
        //    obstacle.carving = !isOpen;
        //}
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
            stream.SendNext(agentsInRange);
        }
        else
        {
            isOpen = (bool)stream.ReceiveNext();
            agentsInRange = (int)stream.ReceiveNext();
        }
    }
}