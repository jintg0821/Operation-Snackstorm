using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel; // 전체 인벤토리
    public bool isOpen = false;

    public InventoryPanel storeInventory;
    public InventoryPanel vendingInventory;
    public InventoryPanel commonInventory;

    //void Start()
    //{
    //    inventoryPanel.SetActive(false);
    //}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isOpen);
        }
    }

    public void ShowStore()
    {
        storeInventory.gameObject.SetActive(true);
        vendingInventory.gameObject.SetActive(false);
        commonInventory.gameObject.SetActive(false);

        storeInventory.Populate(ItemDatabase.Instance.storeItems);
    }

    public void ShowVending()
    {
        storeInventory.gameObject.SetActive(false);
        vendingInventory.gameObject.SetActive(true);
        commonInventory.gameObject.SetActive(false);

        vendingInventory.Populate(ItemDatabase.Instance.vendingItems);
    }

    public void ShowCommon()
    {
        storeInventory.gameObject.SetActive(false);
        vendingInventory.gameObject.SetActive(false);
        commonInventory.gameObject.SetActive(true);

        commonInventory.Populate(ItemDatabase.Instance.commonItems);
    }
}
