using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FireExtinguisher : MonoBehaviourPun
{
    public Collider powderRange;
    public Image testPowderImage;
    public float explodeTime;

    public void FireExtinguisherExplode()
    {
        photonView.RPC("RpcExtinguisherExplode", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RpcExtinguisherExplode()
    {
        StartCoroutine(ExtinguisherExplode());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
                testPowderImage.gameObject.SetActive(true);
        }

        if (other.CompareTag("NPC"))
        {
            AIController ai = other.GetComponent<AIController>();
            ai.isSightRestricted = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
            testPowderImage.gameObject.SetActive(false);
    }

    IEnumerator ExtinguisherExplode()
    {
        powderRange.enabled = true;
        yield return new WaitForSeconds(explodeTime);
        powderRange.enabled = false;
        testPowderImage.gameObject.SetActive(false);
    }
}