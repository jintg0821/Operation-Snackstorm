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

    [PunRPC]
    public void RPC_OnPianoSuccess(PhotonMessageInfo info)
    {
        Debug.Log($"피아노 성공! Sender: {info.Sender?.NickName ?? "NULL"} | IsMaster: {PhotonNetwork.IsMasterClient}");

        CloseMinigame();

        if (PhotonNetwork.IsMasterClient)
        {
            PlayerController player = info.Sender?.TagObject as PlayerController;
            if (player != null)
            {
                Debug.Log($"보상 지급 대상: {player.name}");
                SpawnRewardCoinsForPlayer(player.transform);
            }
            else
            {
                Debug.LogError("PlayerController를 찾을 수 없음! TagObject이 null이거나 PlayerController 없음");
            }
        }
    }

    // 마스터클라이언트 전용: 보상 코인 생성 함수
    private void SpawnRewardCoinsForPlayer(Transform playerTransform)
    {
        Vector3 center = playerTransform.position + Vector3.up * 1.5f; // 머리 위

        for (int i = 0; i < 2; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1.2f, 1.2f),
                Random.Range(0.3f, 0.8f),
                Random.Range(-1.2f, 1.2f)
            );

            GameObject coinObj = PhotonNetwork.Instantiate("Prefabs/Coin", center + offset, Quaternion.identity);

        }
    }
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
            photonView.RPC(nameof(RPC_OnPianoSuccess), RpcTarget.All);
        }
        else
        {
            questionText.text = "틀렸습니다! 다시 시도하세요.";
        }
    }
}
