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
    [Header("Skate")]
    public Image skateImage;
    public bool showCooldown = false;

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

    private void Update()
    {
        if (skateImage != null && skateImage.gameObject.activeInHierarchy)
        {
            skateImage.fillAmount = showCooldown ? 1f : 0f;
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

    public void StartCooldown(float duration)
    {
        if (skateImage != null)
        {
            skateImage.gameObject.SetActive(true);
            skateImage.fillAmount = 1f;

            StartCoroutine(SkateCoolDown(duration));
        }
    }

    private IEnumerator SkateCoolDown(float duration)
    {
        showCooldown = true;
        skateImage.fillAmount = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            skateImage.fillAmount = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        skateImage.fillAmount = 0f;
        skateImage.gameObject.SetActive(false);
        showCooldown = false;
    }
}
