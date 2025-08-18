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

    [SerializeField] private List<PhotonView> players = new List<PhotonView>();

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

    [Header("Point")]
    [SerializeField] private GameObject pointPanel;
    [SerializeField] private GameObject pointSlotPrefab;
    [SerializeField] private GameObject pointSlotContent;
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
                    RoundOver();
                    //NextRound();
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

    [PunRPC]
    void RPC_SetPlayerPosition()
    {
        
    }

    void SetPlayerPosition()
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in photonViews)
        {
            if (pv.IsMine)
            {
                if (pv.gameObject.GetComponent<PlayerController>() != null)
                {
                    pv.transform.position = spawnPoint.position;
                    break;
                }
            }
        }
    }

    public void RegisterPlayer(PhotonView pv)
    {
        if (!players.Contains(pv))
        {
            players.Add(pv);
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

    public void RoundOver()
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in photonViews)
        {
            if (pv.IsMine && players.Contains(pv))
            {
                PlayerController playerController;
                if (pv.gameObject.TryGetComponent<PlayerController>(out playerController))
                {
                    playerController.GetPoint();
                }
            }
        }
        OnPointPanel();
    }

    public void OnPointPanel()
    {
        StartCoroutine(PointPanel());

    }

    IEnumerator PointPanel()
    {
        pointPanel.SetActive(true);
        SetPointPanel();
        yield return new WaitForSeconds(5f);
        pointPanel.SetActive(false);
    }

    public void SetPointPanel()
    {
        if (players.Count > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                GameObject pointSlot = Instantiate(pointSlotPrefab, pointSlotContent.transform);
                TextMeshProUGUI name = pointSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI point = pointSlot.transform.Find("Point").GetComponent<TextMeshProUGUI>();

                int index = i;

                if (pointSlot != null && players[index] != null)
                {
                    name.text = players[index].Owner.NickName;
                    point.text = players[index].GetComponent<PlayerController>().point.ToString();

                }
            }
        }
    }
}
