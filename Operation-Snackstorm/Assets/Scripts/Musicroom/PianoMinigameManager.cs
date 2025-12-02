using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Linq;

public class PianoMinigameManager : MonoBehaviourPun
{
    public static PianoMinigameManager Instance;

    [Header("UI")]
    public GameObject pianoUI;
    public TextMeshProUGUI questionText;

    [Header("Audio")]
    public AudioClip[] noteSounds = new AudioClip[7];

    private List<NoteName> currentAnswers = new List<NoteName>();
    private List<NoteName> pressedSequence = new List<NoteName>();
    private static int challengeCount = 0;
    private bool isPlaying = false;
    private PlayerController playerController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        if (pianoUI != null) pianoUI.SetActive(false);
    }

    public void StartMinigame(PlayerController player)
    {
        if (isPlaying) return;
        playerController = player;
        playerController.isPanelOn = true;
        isPlaying = true;
        pianoUI.SetActive(true);

        challengeCount++;
        pressedSequence.Clear();
        GenerateQuestion();
        AssignSoundsToKeys();
    }

    public void CloseMinigame()
    {
        playerController.isPanelOn = false;
        isPlaying = false;
        if (pianoUI != null) pianoUI.SetActive(false);
        pressedSequence.Clear();
        currentAnswers.Clear();
    }

    void GenerateQuestion()
    {
        int numNotes = Mathf.Min(challengeCount, 7);

        // 모든 음계를 리스트로 만들고 랜덤 섞기
        List<NoteName> allNotes = new List<NoteName>
        {
            NoteName.Do, NoteName.Re, NoteName.Mi, NoteName.Fa,
            NoteName.Sol, NoteName.La, NoteName.Ti
        };

        for (int i = allNotes.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            var temp = allNotes[i];
            allNotes[i] = allNotes[r];
            allNotes[r] = temp;
        }

        currentAnswers.Clear();
        for (int i = 0; i < numNotes; i++)
            currentAnswers.Add(allNotes[i]);

        // 텍스트 만들기
        string[] noteNames = { "도", "레", "미", "파", "솔", "라", "시" };
        string[] colors = { "빨강", "주황", "노랑", "초록", "파랑", "남색", "보라" };

        string notesText = "";
        string colorsText = "";
        for (int i = 0; i < currentAnswers.Count; i++)
        {
            notesText += noteNames[(int)currentAnswers[i]];
            colorsText += colors[(int)currentAnswers[i]];
            if (i < currentAnswers.Count - 1)
            {
                notesText += ", ";
                colorsText += ", ";
            }
        }

        questionText.text = $"{notesText} ({colorsText}) 키를 순서대로 누르세요!";
    }

    public void OnKeyPressed(NoteName pressed)
    {
        if (!isPlaying) return;

        pressedSequence.Add(pressed);

        if (pressedSequence.Count == currentAnswers.Count)
        {
            bool correct = true;
            for (int i = 0; i < currentAnswers.Count; i++)
            {
                if (pressedSequence[i] != currentAnswers[i])
                {
                    correct = false;
                    break;
                }
            }

            if (correct)
                StartCoroutine(DelayedSuccess());
            else
            {
                questionText.text = "틀렸습니다! 다시 시도하세요.";
                pressedSequence.Clear();
            }
        }
        else
        {
            // 아직 다 안 눌렀으면 현재까지 누른 거 표시 (선택)
            string[] noteNames = { "도", "레", "미", "파", "솔", "라", "시" };
            string pressedText = string.Join(", ", pressedSequence.Select(n => noteNames[(int)n]));
            questionText.text = $"{string.Join(", ", currentAnswers.Select(n => noteNames[(int)n]))} 키를 순서대로 누르세요!\n(현재: {pressedText})";
        }
    }

    private IEnumerator DelayedSuccess()
    {
        yield return new WaitForSeconds(0.25f);
        photonView.RPC("RPC_OnPianoSuccess", RpcTarget.All);
    }

    [PunRPC]
    void RPC_OnPianoSuccess(PhotonMessageInfo info)
    {
        CloseMinigame();
        if (PhotonNetwork.IsMasterClient)
        {
            var player = info.Sender?.TagObject as PlayerController;
            if (player != null)
                SpawnRewardCoinsForPlayer(player.transform);
        }
    }

    private void SpawnRewardCoinsForPlayer(Transform playerTransform)
    {
        Vector3 center = playerTransform.position + Vector3.up * 1.5f;

        int coinCount = challengeCount + 1;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 offset = new Vector3(
            Random.Range(-1.5f, 1.5f),
            Random.Range(0.5f, 1.2f),
            Random.Range(-1.5f, 1.5f)
            );
            GameObject coinObj = PhotonNetwork.Instantiate("Prefabs/Coin", center + offset, Quaternion.identity);

            Rigidbody rb = coinObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;                         
                rb.velocity = Vector3.up * Random.Range(2f, 4f);  
            }
        }
    }

    void AssignSoundsToKeys()
    {
        foreach (var key in FindObjectsOfType<PianoKeyButton>())
        {
            int idx = (int)key.note;
            if (idx >= 0 && idx < noteSounds.Length && noteSounds[idx] != null)
                key.SetSound(noteSounds[idx]);
        }
    }
}
