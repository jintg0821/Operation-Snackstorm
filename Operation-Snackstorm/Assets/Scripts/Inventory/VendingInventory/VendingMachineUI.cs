using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VendingMachineUI : MonoBehaviour
{
    [SerializeField] private Transform slotParent; // ½½·Ô ºÎ¸ð (GridLayoutGroup)
    [SerializeField] private GameObject slotPrefab; // ½½·Ô Prefab
    private VendingMachine vendingMachine;

    public void SetupItems(List<Item> items, Inventory inventory, VendingMachine vending) // Inventory·Î º¯°æ
    {
        vendingMachine = vending;

        // ±âÁ¸ ½½·Ô Á¦°Å
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // ½½·Ô »ý¼º
        foreach (Item item in items)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            slot.GetComponentInChildren<Text>().text = item.name;
            slot.GetComponentInChildren<Image>().sprite = item.icon;
            Button selectButton = slot.GetComponentInChildren<Button>();
            selectButton.onClick.AddListener(() => vendingMachine.AddItemToInventory(item));
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}