using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum HotItem
{
    Skateboard,
    Mop,
    Coin,
    ArtVIP
}


public class HotbarItem
{
    public Sprite icon;
}

public class TestHotbarSlot : MonoBehaviour
{
    public Sprite itemRef;
    public int amount;
    public TextMeshProUGUI amountText;

    private HotbarItem _item;
    private HotItem hotItem;

    public HotbarItem item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null)
            {
                itemRef = _item.icon;
            }
            else
            {
                itemRef = null;
            }
        }
    }

    public void SetAmount(int newAmount)
    {
        amount = Mathf.Max(0, newAmount);
        if (amountText != null)
            amountText.text = amount > 0 ? amount.ToString() : "";
    }

    public void AddAmount(int addValue)
    {
        SetAmount(amount + addValue);
    }
}
