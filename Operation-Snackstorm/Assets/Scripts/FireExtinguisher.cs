using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FireExtinguisher : MonoBehaviour
{
    public Collider powderRange;
    public Image testPowderImage;
    public float explodeTime;

    public void FireExtinguisherExplode()
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