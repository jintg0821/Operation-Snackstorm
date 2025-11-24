using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class SuggestionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;               // SuggestionPanel
    public Transform buttonParent;         // ScrollView Content
    public Button candidateButtonPrefab;   // 후보 버튼 프리팹
    public Button submitButton;
    public Button cancelButton;

    [Header("건의 대상들")]
    public List<ReportTarget> candidates;  // 선생님/선배 18명

    [Header("교장실 소환 위치")]
    public Transform principalRoomSpawn;   // 같은 씬 안 교장실 방에 빈 오브젝트 하나 두고 연결

    ReportTarget selectedTarget;
    Transform playerTransform;             // 건의한 플레이어 Transform

    void Start()
    {
        panel.SetActive(false);

        submitButton.onClick.AddListener(OnSubmit);
        cancelButton.onClick.AddListener(Close);

        CreateButtons();
    }

    void CreateButtons()
    {
        foreach (Transform child in buttonParent)
            Destroy(child.gameObject);

        foreach (var target in candidates)
        {
            Button b = Instantiate(candidateButtonPrefab, buttonParent);

            TMP_Text label = b.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = target.displayName;
            }
            else
            {
                Debug.LogError("버튼 프리팹에 TMP_Text가 없습니다!", b);
            }


            ReportTarget captured = target;
            b.onClick.AddListener(() => OnSelect(captured));
        }
    }

    /// <summary>
    /// 건의창 열기 (누가 열었는지 플레이어 Transform 넘겨줌)
    /// </summary>
    public void Open(Transform player)
    {
        playerTransform = player;
        selectedTarget = null;
        panel.SetActive(true);

        // 필요하면 여기서 Time.timeScale = 0f; / 마우스 커서 활성화 등도 가능
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    void OnSelect(ReportTarget target)
    {
        selectedTarget = target;
        Debug.Log("선택된 건의 대상: " + target.displayName);
        // 선택된 버튼 하이라이트 같은 건 UI 쪽에서 추가로 구현 가능
    }

    void OnSubmit()
    {
        if (selectedTarget == null)
        {
            Debug.Log("아직 대상 선택 안 함");
            return;
        }

        // 1) 선택된 NPC 행동 멈추기 (Photon RPC 안에서 처리됨)
        selectedTarget.Report();

        // 2) 건의한 플레이어를 교장실 방으로 텔레포트
        if (playerTransform != null && principalRoomSpawn != null)
        {
            playerTransform.position = principalRoomSpawn.position;
            playerTransform.rotation = principalRoomSpawn.rotation;
        }

        // 3) 창 닫기
        Close();
    }
}
