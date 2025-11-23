using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;

    private Item _item;
    private Inventory inventory;

    public Item item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null)
            {
                image.sprite = item.icon;
                image.color = new Color(1, 1, 1, 1);
            }
            else
            {
                image.sprite = null;
            }
        }
    }

    public void InitSlot(Inventory inv)
    {
        inventory = inv;
        button.onClick.AddListener(OnClickSlot);
    }

    void OnClickSlot()
    {
        if (_item != null)
        {
            inventory.SelectedItem(_item);
        }
    }
}
