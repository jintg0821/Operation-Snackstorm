using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class SuggestionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;               
    public Transform buttonParent;         
    public Button candidateButtonPrefab;   
    public Button submitButton;
    public Button cancelButton;

    [Header("건의 대상들")]
    public List<ReportTarget> candidates;  

    [Header("교장실 소환 위치")]
    public Transform principalRoomSpawn;   

    ReportTarget selectedTarget;
    Transform playerTransform;

    PlayerController playerController;

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
    public void Open(PlayerController player)
    {
        playerController = player;
        player.isPanelOn = true;
        playerTransform = player.transform;
        selectedTarget = null;
        panel.SetActive(true);
    }

    public void Close()
    {
        playerController.isPanelOn = false;
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
            Debug.Log("선택 안 함");
            return;
        }

        selectedTarget.Report();   // ← 그냥 이거 하나만!
        Close();
    }
}
