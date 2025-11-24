using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoInteract : MonoBehaviour
{
    private bool isPlayerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.P))
        {
            PianoMinigameManager.Instance.StartMinigame();
        }
    }
}
