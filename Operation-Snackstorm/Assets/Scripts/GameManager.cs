using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public Transform spawnPoint;

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
    [SerializeField] private int maxRound;

    [Header("Point")]
    [SerializeField] private GameObject pointPanel;
    [SerializeField] private GameObject pointSlotPrefab;
    [SerializeField] private GameObject pointSlotContent;
    #endregion

    private int playersUpdated = 0;

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

        GameStart();
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
    }

    public void GameStart()
    {
        double startTime = PhotonNetwork.Time;
        photonView.RPC("RPC_GameStart", RpcTarget.All, startTime);
    }

    public void RoundOver()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            playersUpdated = 0; //
            photonView.RPC("RPC_RoundOver", RpcTarget.All);
            //photonView.RPC("RPC_OnPointPanel", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_RoundOver()
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in photonViews)
        {
            if (players.Contains(pv) && pv.IsMine)
            {
                PlayerController playerController;
                if (pv.gameObject.TryGetComponent<PlayerController>(out playerController))
                {
                    playerController.GetPoint();
                    var playerCC = playerController.GetComponent<CharacterController>();
                    if (playerCC != null)
                    {
                        playerCC.enabled = false;
                        pv.transform.position = spawnPoint.position;
                        playerCC.enabled = true;
                    }
                }
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (PhotonNetwork.IsMasterClient && changedProps.ContainsKey("Point"))
        {
            playersUpdated++;
            if (playersUpdated >= PhotonNetwork.PlayerList.Length)
            {
                photonView.RPC("RPC_OnPointPanel", RpcTarget.All);
                playersUpdated = 0;  // Reset for next round
            }
        }
    }

    [PunRPC]
    public void RPC_OnPointPanel()
    {
        StartCoroutine(PointPanel());
    }

    public void OnPointPanel()
    {
        photonView.RPC("RPC_OnPointPanel", RpcTarget.All);
    }

    IEnumerator PointPanel()
    {
        pointPanel.SetActive(true);
        SetPointPanel();
        yield return new WaitForSeconds(5f);
        pointPanel.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            if (currentRound >= maxRound)
            {
                PhotonNetwork.LoadLevel("LobbyScene");
            }
            else
            {
                NextRound();
            }
        }
    }

    public void SetPointPanel()
    {
        foreach (Transform child in pointSlotContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            GameObject pointSlot = Instantiate(pointSlotPrefab, pointSlotContent.transform);

            TextMeshProUGUI name = pointSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI point = pointSlot.transform.Find("Point").GetComponent<TextMeshProUGUI>();

            name.text = p.NickName;

            if (p.CustomProperties.ContainsKey("Point"))
            {
                point.text = p.CustomProperties["Point"].ToString();
            }
            else
            {
                point.text = "0";
            }
        }
    }
}
