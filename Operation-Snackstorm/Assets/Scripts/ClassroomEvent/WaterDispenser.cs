using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class WaterDispenser : MonoBehaviourPun
{
    [Header("Settings")]
    public int totalRounds = 3;
    public float fillSpeed = 0.25f;
    public float dangerZoneStart = 0.8f;
    public float maxFill = 1f;

    [SerializeField] private Vector3 boxSize = new Vector3(3f, 2f, 3f);
    [SerializeField] private Vector3 boxOffset = Vector3.zero;
    [SerializeField] private List<PlayerController> nearbyPlayers = new List<PlayerController>();

    public TextMeshProUGUI waterDispenserText;
    private Coroutine textRoutine;

    [Header("References")]
    public Transform waterMesh;
    public TextMeshProUGUI stateText;

    [Header("Water")]
    [SerializeField] private float minY = 0.1f;
    [SerializeField] private float maxY = 1.0f;

    [SerializeField] private float startScaleXZ = 26f;
    [SerializeField] private float endScaleXZ = 32f;

    private int currentRound = 1;
    private float currentFill = 0f;
    private bool missionActive = false;
    private bool isOverflowing = false;
    private bool isMyRoleA = false;

    [SerializeField] private Transform RoleA_Point;
    [SerializeField] private Transform RoleB_Point;
    [SerializeField] private Collider WaterArea;

    private static List<int> interactingPlayers = new List<int>();
    private static bool isInUse = false;

    [PunRPC]
    public void RPC_AssignRoleAndStart(int playerViewID)
    {
        if (isInUse && interactingPlayers.Count >= 2)
        {
            PhotonView pv = PhotonView.Find(playerViewID);
            if (pv != null && pv.IsMine)
                Debug.Log("이미 다른 플레이어들이 사용 중입니다");
            return;
        }

        if (interactingPlayers.Contains(playerViewID)) return;

        interactingPlayers.Add(playerViewID);

        if (interactingPlayers.Count == 1)
        {
            PhotonView pv = PhotonView.Find(playerViewID);
            if (pv != null && pv.IsMine)
                isMyRoleA = true;

            stateText.text = "플레이어 대기 중...";
        }
        else if (interactingPlayers.Count == 2)
        {
            PhotonView pv = PhotonView.Find(playerViewID);
            if (pv != null && pv.IsMine)
                isMyRoleA = false;

            isInUse = true;
            photonView.RPC("StartMission", RpcTarget.All);
        }
    }

    [PunRPC]
    void StartMission()
    {
        currentRound = 1;
        missionActive = true;
        stateText.text = $"Round {currentRound}/3 - Ready!";

        PhotonView playerPV = GetLocalPlayerView();
        if (playerPV == null && !interactingPlayers.Contains(playerPV.ViewID)) return;

        SetPlayerPanelState(true);

        if (isMyRoleA)
            playerPV.transform.position = RoleA_Point.position;
        else
            playerPV.transform.position = RoleB_Point.position;
    }

    void Update()
    {
        SearchPlayer();

        if (!missionActive || isOverflowing) return;

        PhotonView playerPV = GetLocalPlayerView();
        if (playerPV == null) return;
        if (!interactingPlayers.Contains(playerPV.ViewID)) return;

        // 플레이어 A (정수기 담당)
        if (isMyRoleA && Input.GetKey(KeyCode.X))
            photonView.RPC("RPC_FillCup", RpcTarget.All, Time.deltaTime);

        // 플레이어 B (컵 교체 담당)
        if (!isMyRoleA && Input.GetKeyDown(KeyCode.C))
            photonView.RPC("RPC_TrySwap", RpcTarget.All);
    }

    void SearchPlayer()
    {
        Vector3 center = transform.position + transform.TransformDirection(boxOffset);

        Collider[] hits = Physics.OverlapBox(center, boxSize / 2f, transform.rotation);
        List<PlayerController> currentPlayers = new List<PlayerController>();

        foreach (var hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                currentPlayers.Add(player);
                player.isWaterDispenser = true;

                if (!nearbyPlayers.Contains(player) && player.photonView.IsMine)
                {
                    if (textRoutine == null)
                        textRoutine = StartCoroutine(ShowInteractionText(3f));
                }
            }
        }

        foreach (var player in nearbyPlayers)
        {
            if (!currentPlayers.Contains(player))
            {
                player.isWaterDispenser = false;
                if (player.photonView.IsMine && waterDispenserText != null)
                {
                    if (textRoutine != null)
                    {
                        StopCoroutine(textRoutine);
                        textRoutine = null;
                    }
                    waterDispenserText.text = "";
                }
            }
        }

        nearbyPlayers = currentPlayers;
    }

    void ResetGame()
    {
        isInUse = false;
        interactingPlayers.Clear();
        currentRound = 1;
        currentFill = 0f;
        missionActive = false;
        isOverflowing = false;
}

    [PunRPC]
    void RPC_FillCup(float delta)
    {
        currentFill += fillSpeed * delta;
        currentFill = Mathf.Clamp01(currentFill);
        UpdateWaterVisual();

        if (currentFill >= dangerZoneStart)
            stateText.text = $"Round {currentRound}/3 - 위험!";

        if (currentFill >= maxFill)
            photonView.RPC("RPC_Overflow", RpcTarget.All);
    }

    [PunRPC]
    void RPC_TrySwap()
    {
        if (currentFill >= dangerZoneStart && currentFill < maxFill)
        {
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC("RPC_SuccessRound", RpcTarget.All);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC("RPC_Overflow", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_SuccessRound()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentRound++;
            photonView.RPC("RPC_UpdateRound", RpcTarget.All, currentRound);
        }
    }

    [PunRPC]
    void RPC_UpdateRound(int round)
    {
        currentRound = round;

        if (currentRound > totalRounds)
            photonView.RPC("RPC_MissionSuccess", RpcTarget.All);
        else
            StartCoroutine(NextRound());
    }

    [PunRPC]
    void RPC_Overflow()
    {
        if (isOverflowing) return;
        isOverflowing = true;

        stateText.text = "미션 실패! 물이 넘쳤습니다.";
        SetPlayerPanelState(false);

        StartCoroutine(ApplySlipPenalty(10));
    }

    [PunRPC]
    void RPC_MissionSuccess()
    {
        missionActive = false;
        stateText.text = "성공! 15초 동안 이동속도 10% 증가";
        SetPlayerPanelState(false);
        ResetGame();

        photonView.RPC("RPC_ApplyTeamBuff", RpcTarget.All, 1.1f, 15f);
    }

    [PunRPC]
    void RPC_ApplyTeamBuff(float multiplier, float duration)
    {
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in players)
        {
            if (player != null && player.photonView.IsMine)
                player.ApplySpeedModifier(multiplier, duration);
        }
    }

    IEnumerator NextRound()
    {
        yield return new WaitForSeconds(1f);
        currentFill = 0f;
        UpdateWaterVisual();
        isOverflowing = false;
        stateText.text = $"Round {currentRound}/3";
    }

    void UpdateWaterVisual()
    {
        float yPos = Mathf.Lerp(minY, maxY, currentFill / maxFill);

        float scaleXZ = Mathf.Lerp(startScaleXZ, endScaleXZ, currentFill / maxFill);

        Vector3 scale = waterMesh.localScale;
        scale.x = scaleXZ;
        scale.z = scaleXZ;
        waterMesh.localScale = scale;

        Vector3 pos = waterMesh.localPosition;
        pos.y = yPos;
        waterMesh.localPosition = pos;
    }

    IEnumerator ApplySlipPenalty(int t)
    {
        WaterArea.enabled = true;
        yield return new WaitForSeconds(t);
        WaterArea.enabled = false;
        stateText.text = "";
        ResetGame();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!WaterArea.enabled) return;

        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null && player.photonView.IsMine)
            {
                player.ApplySpeedModifier(0.6f, 10f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            Debug.Log("1");
            if (player != null && player.photonView.IsMine)
            {
                player.ApplySpeedModifier(1f, 0f);
                Debug.Log("2");
            }
        }
    }

    PhotonView GetLocalPlayerView()
    {
        foreach (var player in FindObjectsOfType<PhotonView>())
        {
            if (player.IsMine && player.CompareTag("Player"))
                return player;
        }
        return null;
    }

    void SetPlayerPanelState(bool state)
    {
        foreach (int id in interactingPlayers)
        {
            PhotonView pv = PhotonView.Find(id);
            if (pv != null)
            {
                PlayerController pc = pv.GetComponent<PlayerController>();
                if (pc != null)
                    pc.miniGameStart = state;
            }
        }
    }

    IEnumerator ShowInteractionText(float duration)
    {
        if (waterDispenserText == null) yield break;

        waterDispenserText.text = "'Z'를 눌러 게임 시작";

        yield return new WaitForSeconds(duration);

        waterDispenserText.text = "";
        textRoutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + transform.TransformDirection(boxOffset), transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}