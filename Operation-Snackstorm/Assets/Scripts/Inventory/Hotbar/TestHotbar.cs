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

        for (int i = 0; i < slots.Length; i++)
        {
            var img = slots[i].transform.Find("Icon").GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == currentIndex) ? Color.white : Color.gray;
            }
        }
    }

    public void UpdateHeldItemUI(Sprite itemIcon)
    {
        if (heldImage != null)
        {
            if (itemIcon != null)
            {
                heldImage.sprite = itemIcon;
                heldImage.color = Color.white;
            }
            else
            {
                heldImage.color = new Color(0, 0, 0, 0);
            }
        }
    }
}
