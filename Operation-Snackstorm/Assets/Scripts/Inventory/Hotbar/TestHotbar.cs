using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestHotbar : MonoBehaviour
{
    [Header("보관 4칸")]
    public TestHotbarSlot[] slots = new TestHotbarSlot[4];

    [Header("현재 들고 있는 아이템 이미지")]
    public Image heldImage;

    public GameObject TestHotbarPanel;

    private int currentIndex = -1;

    public void ChangeItem(int n)
    {
        if (n < 0 || n >= slots.Length) return;

        currentIndex = n;
        UpdateHeldItemUI();
    }

    void UpdateHeldItemUI()
    {
        if (currentIndex >= 0)
        {
            var selected = slots[currentIndex];

            if (selected.itemRef != null)
            {
                heldImage.sprite = selected.itemRef;
                heldImage.color = Color.white;
            }
            else
            {
                heldImage.color = new Color(0, 0, 0, 0);
            }
            if (selected.amount == 0)
            {
                selected.amountText.text = "";
            }
            else
            {
                selected.amountText.text = selected.amount.ToString();
            }
        }
    }
}
