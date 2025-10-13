using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amountText;
    public GameObject selectedHighlight;

    public void Bind(HotbarSlot slot, bool selected)
    {
        if (slot == null || slot.IsEmpty)
        {
            if (icon) icon.enabled = false;
            if (amountText) amountText.text = "";
            if (selectedHighlight) selectedHighlight.SetActive(selected);
            return;
        }

        var hItem = (slot.itemRef as IHotbarItem);
        if (icon)
        {
            icon.enabled = true;
            icon.sprite = hItem != null ? hItem.GetIcon() : null;
        }
        if (amountText)
            amountText.text = (hItem != null && hItem.IsStackable() && slot.amount > 1) ? slot.amount.ToString() : "";
        if (selectedHighlight) selectedHighlight.SetActive(selected);
    }
}
