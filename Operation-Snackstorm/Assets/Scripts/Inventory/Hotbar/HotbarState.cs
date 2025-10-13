using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarState : MonoBehaviour
{
    [Header("보관 4칸")]
    public HotbarSlot[] slots = new HotbarSlot[4];

    [Header("들고있는 1칸")]
    public HotbarSlot held = new HotbarSlot();

    void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null) slots[i] = new HotbarSlot();
        if (held == null) held = new HotbarSlot();
    }

    public bool TryAdd(object itemRef, int amount)
    {
        if (itemRef == null || amount <= 0) return false;
        var hItem = itemRef as IHotbarItem;

        // 1) 스택 합치기
        if (hItem != null && hItem.IsStackable())
        {
            for (int i = 0; i < slots.Length && amount > 0; i++)
            {
                if (!slots[i].IsEmpty && slots[i].itemRef == itemRef)
                {
                    int can = Mathf.Max(0, hItem.MaxStack() - slots[i].amount);
                    int add = Mathf.Min(can, amount);
                    slots[i].amount += add;
                    amount -= add;
                }
            }
        }

        // 2) 빈 칸 채우기
        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].itemRef = (UnityEngine.Object)itemRef;
                slots[i].amount = hItem != null && hItem.IsStackable()
                                  ? Mathf.Min(hItem.MaxStack(), amount)
                                  : 1;
                amount -= slots[i].amount;
            }
        }

        // 3) held 비었으면 자동 장착(선택)
        if ((held == null || held.IsEmpty) && !slots[0].IsEmpty)
            EquipFromSlot(0);

        return amount <= 0;
    }

    public void EquipFromSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        if (slots[index].IsEmpty) return;

        // swap
        var tempItem = held.itemRef; var tempAmt = held.amount;
        held.itemRef = slots[index].itemRef; held.amount = slots[index].amount;
        slots[index].itemRef = tempItem; slots[index].amount = tempAmt;
    }

    public void ReturnHeld()
    {
        if (held == null || held.IsEmpty) return;
        var hItem = held.AsHotbarItem();
        int amount = held.amount;

        // 기존 스택 합치기
        if (hItem != null && hItem.IsStackable())
        {
            for (int i = 0; i < slots.Length && amount > 0; i++)
            {
                if (!slots[i].IsEmpty && slots[i].itemRef == held.itemRef)
                {
                    int can = Mathf.Max(0, hItem.MaxStack() - slots[i].amount);
                    int add = Mathf.Min(can, amount);
                    slots[i].amount += add;
                    amount -= add;
                }
            }
        }

        // 빈 칸
        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].itemRef = held.itemRef;
                slots[i].amount = hItem != null && hItem.IsStackable() ? Mathf.Min(hItem.MaxStack(), amount) : 1;
                amount -= slots[i].amount;
            }
        }

        if (amount <= 0) { held.itemRef = null; held.amount = 0; }
        else { held.amount = amount; }
    }
}
