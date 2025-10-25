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

    [Header("References")]
    public Transform waterMesh;
    public TextMeshProUGUI stateText;

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
        // 현재 높이를 0~1 비율로 반영
        Vector3 scale = waterMesh.localScale;
        scale.y = Mathf.Lerp(0f, 1f, currentFill);
        waterMesh.localScale = scale;

        // 물이 위로만 차오르도록 위치 보정
        Vector3 pos = waterMesh.localPosition;
        pos.y = scale.y * 0.5f; // 절반만큼 위로 올려서 바닥 기준으로 성장
        waterMesh.localPosition = pos;
    }

    IEnumerator ApplySlipPenalty(int t)
    {
        WaterArea.enabled = true;
        yield return new WaitForSeconds(t);
        WaterArea.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!WaterArea.enabled) return;

        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null && player.photonView.IsMine)
                player.ApplySpeedModifier(0.6f, Mathf.Infinity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null && player.photonView.IsMine)
                player.ApplySpeedModifier(1f, 0f);
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
}