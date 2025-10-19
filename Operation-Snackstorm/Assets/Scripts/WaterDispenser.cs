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
    public Image fillGauge;
    public TextMeshProUGUI stateText;

    private int currentRound = 1;
    private float currentFill = 0f;
    private bool missionActive = false;
    private bool isOverflowing = false;
    private bool isMyRoleA = false;

    private static List<int> interactingPlayers = new List<int>();
    private static bool isInUse = false;

    [PunRPC]
    public void RPC_AssignRoleAndStart(int playerViewID)
    {
        if (isInUse && interactingPlayers.Count >= 2)
        {
            PhotonView pv = PhotonView.Find(playerViewID);
            if (pv != null && pv.IsMine)
            {
                Debug.Log("이미 다른 플레이어들이 사용 중입니다");
            }
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
    }

    void Update()
    {
        if (!missionActive || isOverflowing) return;

        // 플레이어 A (정수기 담당)
        if (isMyRoleA && Input.GetKey(KeyCode.X))
        {
            photonView.RPC("RPC_FillCup", RpcTarget.All, Time.deltaTime);
        }

        // 플레이어 B (컵 교체 담당)
        if (!isMyRoleA && Input.GetKeyDown(KeyCode.C))
        {
            photonView.RPC("RPC_TrySwap", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_FillCup(float delta)
    {
        currentFill += fillSpeed * delta;
        currentFill = Mathf.Clamp01(currentFill);
        //fillGauge.fillAmount = currentFill;
        UpdateWaterVisual();

        if (currentFill >= dangerZoneStart)
            stateText.text = $"Round {currentRound}/3 - Danger!";

        if (currentFill >= maxFill)
            RPC_Overflow();
    }

    [PunRPC]
    void RPC_TrySwap()
    {
        if (currentFill >= dangerZoneStart && currentFill < maxFill)
        {
            RPC_SuccessRound();
        }
        else
        {
            RPC_Overflow();
        }
    }

    [PunRPC]
    void RPC_SuccessRound()
    {
        stateText.text = $" Round {currentRound} Success!";
        currentRound++;

        if (currentRound > totalRounds)
        {
            RPC_MissionSuccess();
        }
        else
        {
            StartCoroutine(NextRound());
        }
    }

    [PunRPC]
    void RPC_Overflow()
    {
        if (isOverflowing) return;
        isOverflowing = true;

        stateText.text = " Overflow! Mission Failed!";

        photonView.RPC("RPC_FailEffects", RpcTarget.All);
        //StartCoroutine(ApplySlipPenalty(0.6f, 10f));
    }

    [PunRPC]
    void RPC_MissionSuccess()
    {
        missionActive = false;
        stateText.text = " All Success! +10% Speed (15s)";
        //StartCoroutine(ApplyTeamBuff(1.1f, 15f));
    }

    [PunRPC]
    void RPC_FailEffects()
    {
        // 컵 뒤집히는 개그 연출 (로컬 오브젝트로 가능)
        // 예: 머리 위에 컵+물고기 아이콘 띄우기
    }

    IEnumerator NextRound()
    {
        yield return new WaitForSeconds(1f);
        currentFill = 0f;
        UpdateWaterVisual();
        isOverflowing = false;
        stateText.text = $"Round {currentRound}/3 - Ready!";
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


    //IEnumerator ApplyTeamBuff(float speedMultiplier, float duration)
    //{
    //    TeamStats.Instance.ApplySpeedMultiplier(speedMultiplier);
    //    yield return new WaitForSeconds(duration);
    //    TeamStats.Instance.ResetSpeed();
    //}

    //IEnumerator ApplySlipPenalty(float slowMultiplier, float duration)
    //{
    //    TeamStats.Instance.ApplySpeedMultiplier(slowMultiplier);
    //    yield return new WaitForSeconds(duration);
    //    TeamStats.Instance.ResetSpeed();
    //}
}
