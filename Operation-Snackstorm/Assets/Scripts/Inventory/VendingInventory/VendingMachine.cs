using UnityEngine;
using System.Collections.Generic;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] private List<Item> availableItems; // 판매 아이템 목록
    [SerializeField] private GameObject vendingMachineUI; // UI Canvas
    [SerializeField] private Inventory inventory; // Inventory로 변경

    void Start()
    {
        // 동적 참조 (옵션)
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
            if (inventory == null)
            {
                Debug.LogError("Inventory 컴포넌트를 찾을 수 없습니다!");
            }
        }
    }

    public void ActivateUI()
    {
        if (vendingMachineUI != null)
        {
            vendingMachineUI.SetActive(true);
            VendingMachineUI uiScript = vendingMachineUI.GetComponent<VendingMachineUI>();
            if (uiScript != null)
            {
                uiScript.SetupItems(availableItems, inventory, this);
            }
        }
    }

    public void AddItemToInventory(Item item)
    {
        if (inventory != null)
        {
            inventory.AddItem(item); // Inventory의 AddItem 호출
            Debug.Log($"아이템 추가: {item.name}");
        }
        else
        {
            Debug.LogError("Inventory가 설정되지 않았습니다!");
        }
    }
}