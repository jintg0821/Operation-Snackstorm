using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;

    public Transform spawnPoint;
    public GameObject startDoor;

    #region UI
    [Header("Timer")]
    public bool onTimer = false;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timerTime;
    [SerializeField] private float currentTimerTime;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private double timerStartTime;

    [Header("Round")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private int currentRound;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetPlayerPosition();

        if (timerText != null && roundText != null)
        {
            roundText.text = $"Round {currentRound.ToString()}";
        }
        currentTimerTime = timerTime;
    }

    void Update()
    {
        if (onTimer)
        {
            double elapsed = PhotonNetwork.Time - timerStartTime;
            currentTimerTime = Mathf.Clamp((float)(timerTime - elapsed), 0f, timerTime);
            timerText.text = currentTimerTime.ToString("F1");

            if (currentTimerTime <= 0f)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    NextRound();
                    photonView.RPC("RPC_StopTimer", RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    void RPC_StopTimer()
    {
        onTimer = false;
    }

    [PunRPC]
    void RPC_NextRound(int round)
    {
        currentRound = round;
        roundText.text = $"Round {currentRound}";
    }

    public void NextRound()
    {
        currentRound++;
        photonView.RPC("RPC_NextRound", RpcTarget.All, currentRound);

        //GameStart();
    }

    void SetPlayerPosition()
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in photonViews)
        {
            if (pv.IsMine)
            {
                pv.transform.position = spawnPoint.position;
                break;
            }    
        }
    }

    [PunRPC]
    void RPC_GameStart(double startTime)
    {
        timerStartTime = startTime;
        onTimer = true;
        currentTimerTime = timerTime;
        startDoor.SetActive(false);
    }

    public void GameStart()
    {
        double startTime = PhotonNetwork.Time;
        photonView.RPC("RPC_GameStart", RpcTarget.All, startTime);
    }
}
