using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CounselDialog : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject dialoguePanel; // 대화창 패널 (DialoguePanel)
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("NPC 설정")]
    public string npcName = "상담 선생님";
    [TextArea(3, 5)]
    public string[] sentences; // 대사 목록 (여러 줄 가능)

    [Header("플레이어 설정 (중요!)")]
    private MonoBehaviour playerMoveScript;

    private bool isPlayerNear = false; // 플레이어 접근 여부
    private int index = 0;             // 현재 대사 순서

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F) && !dialoguePanel.activeSelf)
        {
            StartDialogue();
        }
        else if (dialoguePanel.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)))
        {
            NextSentence();
        }
    }

    void StartDialogue()
    {
        if (playerMoveScript != null) playerMoveScript.enabled = false;

        dialoguePanel.SetActive(true); // UI 켜기
        nameText.text = npcName;       // 이름 설정
        index = 0;                     // 대사 순서 초기화
        dialogueText.text = sentences[index]; // 첫 대사 출력
    }

    void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;
            dialogueText.text = sentences[index];
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (playerMoveScript != null) playerMoveScript.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            playerMoveScript = other.GetComponent<PlayerMovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            EndDialogue();
            playerMoveScript = null;
        }
    }
}