using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public Transform spawnPoint;
    public Collider PointArea;

    [SerializeField] private List<PhotonView> players = new List<PhotonView>();
    [SerializeField] private GameObject[] startWalls;
    public List<AIController> aiList = new List<AIController>();
    [SerializeField] private List<PhotonView> inPointAreaPlayers = new List<PhotonView>();

    [SerializeField] private bool roundStart = false;
    public bool gameStart = false;

    private AudioSource bellSource;

    #region UI

    [Header("Timer")]
    public bool onTimer = false;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timerTime;
    [SerializeField] private float currentTimerTime;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private double timerStartTime;
    private float fillAmount = 1;
    [SerializeField] private Image timeImage;

    [Header("Round")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private int currentRound;
    [SerializeField] private int maxRound;

    [Header("RoundPoint")]
    [SerializeField] private GameObject pointPanel;
    [SerializeField] private GameObject pointSlotPrefab;
    [SerializeField] private GameObject pointSlotContent;

    [Header("TotalPoint")]
    [SerializeField] private GameObject totalPointPanel;
    [SerializeField] private GameObject totalPointSlotPrefab;
    [SerializeField] private Transform totalPointSlotContent;

    [SerializeField] private GameObject resultOptionPanel;
    #endregion

    private int playersUpdated = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            bellSource = GetComponent<AudioSource>();
            if (bellSource != null)
                bellSource.playOnAwake = false;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        timerTime = 90f;
        SetPlayerPosition();

        if (timerText != null && roundText != null)
        {
            roundText.text = "게임 대기 중...";
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

            if (currentTimerTime > 0f)
            {
                fillAmount = currentTimerTime / timerTime;
                timeImage.fillAmount = Mathf.Clamp01(fillAmount);
            }

            if (currentTimerTime <= 0f)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    RoundOver();
                    photonView.RPC("RPC_StopTimer", RpcTarget.All);
                }
            }

            foreach (var wall in startWalls)
            {
                wall.SetActive(false);
            }

            if (aiList.Count > 0)
            {
                foreach (var ai in aiList)
                {
                    ai.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            foreach (var wall in startWalls)
            {
                wall.SetActive(true);
            }

            if (aiList.Count > 0)
            {
                foreach (var ai in aiList)
                {
                    ai.gameObject.SetActive(false);
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
        PlayBell();

        timerStartTime = startTime;
        onTimer = true;
        gameStart = true;
        currentTimerTime = timerTime;
        fillAmount = 1f;
        timerText.gameObject.SetActive(true);
        roundText.text = $"Round {currentRound}";

        //FindObjectOfType<LibraryItemSpawner>()?.SpawnItems();
    }

    private void PlayBell()
    {
        if (bellSource != null && bellSource.clip != null)
        {
            bellSource.PlayOneShot(bellSource.clip);
        }
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
            CheckDirty();
            ClearAllDirty();

            playersUpdated = 0;
            photonView.RPC("RPC_RoundOver", RpcTarget.All);
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
                if (pv.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController))
                {
                    playerController.GetRoundPoint(inPointAreaPlayers.Contains(pv));
                    playerController.UpdateTotalPoint();

                    var playerCC = playerController.GetComponent<CharacterController>();
                    if (playerCC != null)
                    {
                        StartCoroutine(Stop_CC(playerCC));
                    }
                }
            }
        }

        inPointAreaPlayers.Clear();
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (!gameStart) return;

        if (PhotonNetwork.IsMasterClient && changedProps.ContainsKey("RoundPoint"))
        {
            playersUpdated++;
            if (playersUpdated >= PhotonNetwork.PlayerList.Length)
            {
                photonView.RPC("RPC_OnPointPanel", RpcTarget.All);
                playersUpdated = 0;
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
        if (currentRound >= maxRound)
        {
            totalPointPanel.SetActive(true);
            SetTotalPointPanel();

            yield return new WaitForSeconds(5f);

            PhotonView[] photonViews = FindObjectsOfType<PhotonView>();

            foreach (PhotonView pv in photonViews)
            {
                if (players.Contains(pv) && pv.IsMine)
                {
                    if (pv.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController))
                    {
                        playerController.isPanelOn = true;
                    }
                }
            }
                totalPointPanel.SetActive(false);

            resultOptionPanel.SetActive(true);
        }
        else
        {
            pointPanel.SetActive(true);
            SetPointPanel();

            yield return new WaitForSeconds(5f);
            pointPanel.SetActive(false);

            PlayBell();

            if (PhotonNetwork.IsMasterClient && gameStart)
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

        var sortedPlayers = PhotonNetwork.PlayerList.OrderByDescending(p => (int)(p.CustomProperties["RoundPoint"] ?? 0));

        foreach (Photon.Realtime.Player p in sortedPlayers)
        {
            GameObject pointSlot = Instantiate(pointSlotPrefab, pointSlotContent.transform);
            TextMeshProUGUI name = pointSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI point = pointSlot.transform.Find("Point").GetComponent<TextMeshProUGUI>();
            name.text = p.NickName;

            if (p.CustomProperties.ContainsKey("RoundPoint"))
            {
                point.text = p.CustomProperties["RoundPoint"].ToString();
            }
            else
            {
                point.text = "0";
            }
        }
    }

    public void SetTotalPointPanel()
    {
        foreach (Transform child in totalPointSlotContent.transform)
        {
            Destroy(child.gameObject);
        }

        var sortedPlayers = PhotonNetwork.PlayerList.OrderByDescending(p => (int)(p.CustomProperties["TotalPoint"] ?? 0));

        foreach (Photon.Realtime.Player p in sortedPlayers)
        {
            GameObject totalPointSlot = Instantiate(totalPointSlotPrefab, totalPointSlotContent.transform);
            TextMeshProUGUI name = totalPointSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI roundPoint = totalPointSlot.transform.Find("RoundPoint").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI bonusPoint = totalPointSlot.transform.Find("BonusPoint").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI minusPoint = totalPointSlot.transform.Find("MinusPoint").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI totalPoint = totalPointSlot.transform.Find("TotalPoint").GetComponent<TextMeshProUGUI>();

            name.text = p.NickName;

            if (p.CustomProperties.ContainsKey("AccumulatedRoundPoint"))
                roundPoint.text = p.CustomProperties["AccumulatedRoundPoint"].ToString();
            else
                roundPoint.text = "0";

            if (p.CustomProperties.ContainsKey("BonusPoint"))
                bonusPoint.text = p.CustomProperties["BonusPoint"].ToString();
            else
                bonusPoint.text = "0";

            if (p.CustomProperties.ContainsKey("MinusPoint"))
                minusPoint.text = p.CustomProperties["MinusPoint"].ToString();
            else
                minusPoint.text = "0";

            if (p.CustomProperties.ContainsKey("TotalPoint"))
                totalPoint.text = p.CustomProperties["TotalPoint"].ToString();
            else
                totalPoint.text = "0";
        }
    }

    public void StartPunishment(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_ExecutePunishment", RpcTarget.All, viewID);
        }
    }

    [PunRPC]
    void RPC_ExecutePunishment(int viewID)
    {
        PhotonView targetPV = PhotonView.Find(viewID);
        if (targetPV != null && targetPV.IsMine)
        {
            TrashCleanupMission.Instance.StartMission(targetPV.GetComponent<PlayerController>());
        }
    }

    public void OnClick_PlayAgain()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_RestartGame", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_RestartGame()
    {
        StopAllCoroutines();

        currentRound = 1;
        roundText.text = "게임 대기 중...";
        timerText.gameObject.SetActive(false);
        timerText.text = "";
        timeImage.fillAmount = 1;
        currentTimerTime = timerTime;
        onTimer = false;
        gameStart = false;
        roundStart = false;
        inPointAreaPlayers.Clear();
        playersUpdated = 0;

        foreach (var playerObj in FindObjectsOfType<PlayerController>())
        {
            var pv = playerObj.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                CharacterController cc = playerObj.GetComponent<CharacterController>();

                if (cc != null)
                {
                    cc.enabled = false;
                    playerObj.transform.position = spawnPoint.position;
                    cc.enabled = true;
                }
                playerObj.isPanelOn = false;

                playerObj.ResetTotalPoint();
            }
        }

        foreach (var ai in aiList)
        {
            if (ai != null)
            {
                ai.gameObject.SetActive(false);
            }
        }

        pointPanel.SetActive(false);
        totalPointPanel.SetActive(false);
        resultOptionPanel.SetActive(false);

        if (timerText != null && roundText != null)
        {
            roundText.text = "게임 대기 중...";
        }
    }

    public void OnClick_ReturnToLobby()
    {
        StartCoroutine(ReturnToLobbyCoroutine());
    }

    IEnumerator ReturnToLobbyCoroutine()
    {
        PhotonNetwork.LeaveRoom();

        while (PhotonNetwork.InRoom)
            yield return null;

        SceneManager.LoadScene("LobbyScene");
    }

    public void OnClick_QuitGame()
    {
        Application.Quit();
    }

    IEnumerator Stop_CC(CharacterController cc)
    {
        if (cc != null)
        {
            cc.enabled = false;
            cc.transform.position = spawnPoint.position;
            yield return new WaitForSeconds(0.5f);
            cc.enabled = true;
        }
    }

    private void CheckDirty()
    {
        GameObject[] dirts = GameObject.FindGameObjectsWithTag("Dirty");

        if (dirts.Length > 0)
        {
            foreach (var player in players)
            {
                photonView.RPC("RPC_GetMinusPoint", player.Owner, 5);
            }
        }
    }

    void ClearAllDirty()
    {
        GameObject[] dirts = GameObject.FindGameObjectsWithTag("Dirty");

        foreach (GameObject dirt in dirts)
        {
            PhotonView pv = dirt.GetComponent<PhotonView>();
            if (pv && pv.IsMine)
            {
                PhotonNetwork.Destroy(dirt);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView player = other.GetComponent<PhotonView>();
            if (!inPointAreaPlayers.Contains(player))
            {
                inPointAreaPlayers.Add(player);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView player = other.GetComponent<PhotonView>();
            if (inPointAreaPlayers.Contains(player))
            {
                inPointAreaPlayers.Remove(player);
            }
        }
    }
}