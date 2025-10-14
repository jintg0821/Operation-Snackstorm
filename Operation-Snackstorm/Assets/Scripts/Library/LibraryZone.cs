using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine)
        {
            player.isInLibrary = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine)
        {
            player.isInLibrary = false;
        }
    }
}