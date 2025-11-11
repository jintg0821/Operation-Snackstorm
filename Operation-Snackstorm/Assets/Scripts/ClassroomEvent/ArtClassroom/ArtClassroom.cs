using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ArtQuestionSet
{
    public string questionName;
    public GameObject correctArt;
    public List<GameObject> wrongArts;
}
public class ArtClassroom : MonoBehaviourPunCallbacks
{
    public Transform artPoint1;
    public Transform artPoint2;

    public List<ArtQuestionSet> questionSets;

    [SerializeField] private GameObject spawnedArt1;
    [SerializeField] private GameObject spawnedArt2;
    [SerializeField] private PhotonView spawnedArtPV1;
    [SerializeField] private PhotonView spawnedArtPV2;
    [SerializeField] private GameObject correctArtObj;

    [SerializeField] private Transform entryDirection;

    [SerializeField] private ArtQuestionSet currentQuestion;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI artStateText;

    public static bool isPlaying = false;
    private bool isMyTurn = false; 
    private bool canStart = true;

    [PunRPC]
    void RPC_GenerateQuestion()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentQuestion = questionSets[Random.Range(0, questionSets.Count)];
        GameObject randomWrong = currentQuestion.wrongArts[Random.Range(0, currentQuestion.wrongArts.Count)];

        List<GameObject> artList = new List<GameObject> { currentQuestion.correctArt, randomWrong };
        ShuffleList(artList);

        List<Transform> spawnPoints = new List<Transform> { artPoint1, artPoint2 };
        ShuffleList(spawnPoints);

        spawnedArt1 = PhotonNetwork.Instantiate($"Picture/Prefab/{artList[0].name}", spawnPoints[0].position, Quaternion.Euler(-90f, 0, 180f));

        spawnedArt2 = PhotonNetwork.Instantiate($"Picture/Prefab/{artList[1].name}", spawnPoints[1].position, Quaternion.Euler(-90f, 0, 180f));

        correctArtObj = (artList[0] == currentQuestion.correctArt) ? spawnedArt1 : spawnedArt2;
    }

    public void TryStartGame()
    {
        if (!canStart)
        {
            artStateText.text = "아직 시작할 수 없습니다.";
            StartCoroutine(ResetText(2f));
            return;
        }

        if (isPlaying)
        {
            artStateText.text = "이미 다른 플레이어가 플레이 중입니다.";
            StartCoroutine(ResetText(2f));
            return;
        }

        photonView.RPC("RPC_RequestStartGame", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC] 
    void RPC_RequestStartGame(int requesterActorNumber) 
    { 
        if (!PhotonNetwork.IsMasterClient) return;
        if (!canStart || isPlaying) return;

        canStart = false;

        photonView.RPC("RPC_StartGame", RpcTarget.All, requesterActorNumber); 
    }

    [PunRPC]
    void RPC_StartGame(int playerActorNumber)
    {
        isPlaying = true;

        if (PhotonNetwork.IsMasterClient)
        {
            if (spawnedArt1 != null) PhotonNetwork.Destroy(spawnedArt1);
            if (spawnedArt2 != null) PhotonNetwork.Destroy(spawnedArt2);
        }

        isMyTurn = (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_GenerateQuestion", RpcTarget.All);
        }
    }

    public void TryAnswer(GameObject clickedObj)
    {
        if (!isMyTurn)
        {
            artStateText.text = "다른 플레이어가 플레이 중입니다.";
            StartCoroutine(ResetText(3f));
            return;
        }

        bool isCorrect = (clickedObj == correctArtObj);
        photonView.RPC("RPC_EndGame", RpcTarget.All, isCorrect, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_EndGame(bool isCorrect, int actorNumber)
    {
        if (isCorrect)
        {
            artStateText.text = "정답입니다!";
            PlayerController player = FindObjectOfType<PlayerController>();
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber && player)
                player.artVIPCard = true;
        }
        else
        {
            artStateText.text = "틀렸습니다!";
        }

        StartCoroutine(ResetText(2f));

        isPlaying = false;
        isMyTurn = false;

        StartCoroutine(AllowRestart());
    }

    IEnumerator AllowRestart()
    {
        yield return new WaitForSeconds(3f);
        canStart = true;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 playerForward = other.transform.forward;

        Vector3 entryForward = entryDirection.forward;

        float dot = Vector3.Dot(playerForward, entryForward);

        if (dot < 0f)
        {
            return;
        }

        TryStartGame();
    }

    IEnumerator ResetText(float n)
    {
        yield return new WaitForSeconds(n);
        artStateText.text = "";
    }
}
