using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    [SerializeField] Image image;

    public int count;

    [SerializeField] TextMeshProUGUI countText;

    private Item _item;

    public Item item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null)
            {
                image.sprite = item.icon;
                countText.text = count.ToString();
                image.color = new Color(1, 1, 1, 1);
            }
            else
            {
                countText.text = "";
                image.color = new Color(0, 0, 0, 0);

            }
        }
    }
}
