using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
}
