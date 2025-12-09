using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class VendingMachineUI : MonoBehaviour
{
    [SerializeField] private Transform slotParent; // ½½·Ô ºÎ¸ð (GridLayoutGroup)
    [SerializeField] private GameObject slotPrefab; // ½½·Ô Prefab
    [SerializeField] private VendingMachine vendingMachine;

    public GameObject vendingMachineUI;

    public PlayerController PlayerController;

    public bool vendingMachineOpen;

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }

    public void Start()
    {
        GenerateItem();
    }

    public void GenerateItem()
    {
        for (int i = 0; i < vendingMachine.availableItems.Count; i++)
        {
            GameObject itemUIObj = Instantiate(slotPrefab, slotParent.transform);

            Image itemImage = itemUIObj.transform.GetChild(0).GetComponentInChildren<Image>();
            TextMeshProUGUI itemName = itemUIObj.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI itemPrice = itemUIObj.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            Button buyBtn = itemUIObj.transform.GetChild(3).GetComponentInChildren<Button>();
            if (buyBtn == null)
                Debug.Log("dddd");

            int index = i;

            buyBtn.onClick.AddListener(() => vendingMachine.OnBuyButtonClick(vendingMachine.availableItems[index]));

            if (itemUIObj != null && vendingMachine.availableItems[index] != null)
            {
                itemImage.sprite = vendingMachine.availableItems[index].icon;
                itemName.text = vendingMachine.availableItems[index].name;
                itemPrice.text = vendingMachine.availableItems[index].price.ToString();
            }
        }
    }

    public void OnvendingMachinePanel(PlayerController playerController)
    {
        vendingMachineOpen = !vendingMachineUI.activeSelf;
        vendingMachineUI.SetActive(vendingMachineOpen);

        PlayerController = playerController;
        PlayerController.isPanelOn = vendingMachineOpen;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (vendingMachineOpen)
            {
                vendingMachineUI.SetActive(false);
                vendingMachineOpen = false;
                PlayerController.isPanelOn = vendingMachineOpen;
            }
        }
    }
}