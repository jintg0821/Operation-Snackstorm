using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PianoMinigameManager : MonoBehaviourPun
{
    public static PianoMinigameManager Instance;

    [Header("UI")]
    public GameObject pianoUI;
    public TextMeshProUGUI questionText;

    private NoteName currentAnswer;
    private bool isPlaying = false;

    PlayerController playerController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (pianoUI != null)
            pianoUI.SetActive(false);
    }

    public void StartMinigame(PlayerController player)
    {
        if (isPlaying) return;

        playerController = player;
        playerController.isPanelOn = true;

        isPlaying = true;
        pianoUI.SetActive(true);
        GenerateQuestion();
    }

    public void CloseMinigame()
    {
        playerController.isPanelOn = false;

        isPlaying = false;
        if (pianoUI != null)
            pianoUI.SetActive(false);
    }

    void GenerateQuestion()
    {
        int index = Random.Range(0, System.Enum.GetValues(typeof(NoteName)).Length);
        currentAnswer = (NoteName)index;

        string[] colors = { "빨강", "주황", "노랑", "초록", "파랑", "남색", "보라" };
        string color = colors[index % colors.Length];

        questionText.text = $"{currentAnswer} ({color}) 키를 누르세요!";
    }

    public void OnKeyPressed(NoteName pressed)
    {
        if (!isPlaying) return;

        if (pressed == currentAnswer)
        {
            photonView.RPC("RPC_OnPianoSuccess", RpcTarget.All);
        }
        else
        {
            questionText.text = "틀렸습니다! 다시 시도하세요.";
        }
    }
}
