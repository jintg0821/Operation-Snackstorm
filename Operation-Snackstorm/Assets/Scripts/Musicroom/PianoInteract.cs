using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PianoInteract : MonoBehaviour
{
    [Header("Guide UI")]
    public TextMeshProUGUI guideText;

    public bool isPlayerInRange = false;
    PlayerController player;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            isPlayerInRange = true;

            if (guideText != null)
            {
                guideText.gameObject.SetActive(true);
                guideText.text = "O 키를 눌러 미니 게임을 시작하세요!";
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            isPlayerInRange = false;

            if (guideText != null)
                guideText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.O))
        {
            PianoMinigameManager.Instance.StartMinigame(player);

            if (guideText != null)
                guideText.gameObject.SetActive(false);
        }
    }
}
