using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviourPunCallbacks
{
    public List<Item> items;

    [SerializeField]
    private Transform slotParent;
    [SerializeField]
    private Slot[] slots;

    public GameObject inventoryPanel;
    public bool isOpen;

    public PlayerController PlayerController;

#if UNITY_EDITOR
    private void OnValidate()
    {
        slots = slotParent.GetComponentsInChildren<Slot>();
    }
#endif

    void Awake()
    {
        FreshSlot();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen)
            {
                inventoryPanel.SetActive(false);
                isOpen = false;
                PlayerController.isPanelOn = isOpen;
            }
        }
    }

    public void OnInventoryPanel(PlayerController playerController)
    {
        isOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isOpen);

        PlayerController = playerController;
        PlayerController.isPanelOn = isOpen;
    }

    public void FreshSlot()
    {
        int i = 0;
        for (; i < items.Count && i < slots.Length; i++)
        {
            slots[i].item = items[i];
        }
        for (; i < slots.Length; i++)
        {
            slots[i].item = null;
        }
    }

    public void AddItem(Item _item)
    {
        if (items.Count < slots.Length)
        {
            items.Add(_item);
            FreshSlot();
        }
        else
        {
            print("½½·ÔÀÌ °¡µæ Â÷ ÀÖ½À´Ï´Ù.");
        }
    }

    public void RemoveItem(Item _item)
    {
        if (_item != null)
        {
            items.Remove(_item);
            FreshSlot();
        }
    }
}
