using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuggestionBox : MonoBehaviour
{
    public SuggestionUI suggestionUI;

    private Coroutine resultCoroutine; 

    [Header("건의함 안내 텍스트")]
    public TMP_Text interactText;

    Transform playerInRange;

    private void Awake()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = other.transform;

            if (interactText != null && !suggestionUI.panel.activeSelf)
            {
                interactText.gameObject.SetActive(true);
                interactText.text = "C 키를 이용하여 건의함 사용"; 
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInRange == other.transform)
        {
            playerInRange = null;
            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (suggestionUI.panel.activeSelf)
        {
            if (interactText != null)
                interactText.gameObject.SetActive(false);
            return;
        }

        if (playerInRange != null && Input.GetKeyDown(KeyCode.C))
        {
            PlayerController player = playerInRange.GetComponent<PlayerController>();
            if (player != null)
            {
                suggestionUI.Open(player);
                if (interactText != null)
                    interactText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowResultMessage(string message)
    {
        if (interactText == null) return;

        if (resultCoroutine != null)
            StopCoroutine(resultCoroutine);

        interactText.text = message;
        interactText.color = new Color(1f, 0.95f, 0.6f);
        interactText.fontSize = 38;
        interactText.gameObject.SetActive(true);

        resultCoroutine = StartCoroutine(HideResultAfterDelay());
    }

    private IEnumerator HideResultAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (interactText != null)
        {
            interactText.gameObject.SetActive(false);
            interactText.fontSize = 28;
            interactText.color = Color.white;
        }
        resultCoroutine = null;
    }
}
