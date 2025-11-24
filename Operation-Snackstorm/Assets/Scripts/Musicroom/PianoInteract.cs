using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoInteract : MonoBehaviour
{
    private bool isPlayerInRange = false;
    PlayerController player;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            isPlayerInRange = false;
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.O))
        {
            
            PianoMinigameManager.Instance.StartMinigame(player);
        }
    }
}
