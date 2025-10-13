using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    public HotbarState state;          // 플레이어에 붙인 컴포넌트
    public HotbarSlotUI[] slotUIs;     // 4칸
    public HotbarSlotUI heldUI;        // 들고있는 1칸
    int selectedIndex = 0;

    void Start() => Refresh();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { state.EquipFromSlot(0); selectedIndex = 0; Refresh(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { state.EquipFromSlot(1); selectedIndex = 1; Refresh(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { state.EquipFromSlot(2); selectedIndex = 2; Refresh(); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { state.EquipFromSlot(3); selectedIndex = 3; Refresh(); }

        if (Input.GetKeyDown(KeyCode.R)) { state.ReturnHeld(); Refresh(); }

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(wheel) > 0.01f)
        {
            selectedIndex = (selectedIndex + (wheel > 0 ? -1 : 1) + 4) % 4;
            Refresh();
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].Bind(state.slots[i], i == selectedIndex);
        heldUI.Bind(state.held, false);
    }
}
