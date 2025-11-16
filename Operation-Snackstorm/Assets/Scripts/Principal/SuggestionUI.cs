using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class SuggestionUI : MonoBehaviour
{
    public static SuggestionUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform npcButtonParent;
    [SerializeField] private GameObject npcButtonPrefab;
    [SerializeField] private TMP_InputField contentField;

    private SuggestionBox currentBox;
    private NPC selectedNPC;
    private Action<NPC, string> onSubmit;

    private void Awake() => Instance = this;

    public void Open(SuggestionBox box, List<NPC> npcs)
    {
        currentBox = box;
        panel.SetActive(true);
        PopulateNPCButtons(npcs);
    }

    private void PopulateNPCButtons(List<NPC> npcs)
    {
        foreach (Transform child in npcButtonParent) Destroy(child.gameObject);

        foreach (var npc in npcs)
        {
            var btn = Instantiate(npcButtonPrefab, npcButtonParent).GetComponent<Button>();
            btn.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = npc.displayName;
            btn.onClick.AddListener(() => SelectNPC(npc));
        }
    }

    private void SelectNPC(NPC npc)
    {
        selectedNPC = npc;
        npc.PauseAI(true); // 즉시 행동 정지
    }

    public void OnSubmitButton()
    {
        if (selectedNPC == null) return;

        // 1. NPC 정지
        selectedNPC.PauseAI(true);

        // 2. 로그 저장
        SuggestionManager.Instance.Submit(selectedNPC, contentField.text);

        // 3. 플레이어를 교장실로 이동
        //PlayerController.Instance.TeleportToPrincipalOffice();

        // UI 닫기
        selectedNPC = null;
        panel.SetActive(false);
    }
}
