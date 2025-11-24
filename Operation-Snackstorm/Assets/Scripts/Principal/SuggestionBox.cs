using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuggestionBox : MonoBehaviour
{
    public SuggestionUI suggestionUI;
    public string playerTag = "Player";

    Transform playerInRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = other.transform;
            Debug.Log("건의함 범위 안으로 들어옴");
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (playerInRange == other.transform)
                playerInRange = null;

            Debug.Log("건의함 범위 밖으로 나감");
        }
    }

    void Update()
    {
        if (playerInRange != null && Input.GetKeyDown(KeyCode.C))
        {
            PlayerController player = playerInRange.GetComponent<PlayerController>();
            suggestionUI.Open(player);

            Debug.Log("아무거나");
        }
    }
}
