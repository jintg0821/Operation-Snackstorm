using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScienceClassroom : MonoBehaviourPun
{
    public Animator animator;
    public bool onAnim = false;

    public GameObject[] gameObjects;
    public GameObject scienceObj;

    [PunRPC]
    public void ScienceClassroomOnAnim(bool v)
    {
        animator.SetBool("OnAnim", v);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!onAnim)
            {
                photonView.RPC("ScienceClassroomOnAnim", RpcTarget.All, true);
            }
        }
    }

    public void AnimEnd()
    {
        foreach (var obj in gameObjects)
        { 
            Destroy(obj.gameObject);        
        }
        scienceObj.SetActive(true);
    }
}
