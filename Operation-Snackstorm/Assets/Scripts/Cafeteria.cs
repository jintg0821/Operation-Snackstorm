using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cafeteria : MonoBehaviourPun
{
    public Item[] items;
    
    public GameObject cafeteriaPanel;
    public GameObject cafeteriaContent;
    public Transform itemSpawnPoint;

    [Header("Item")]
    public GameObject itemUIPrefab;

    public bool isCafeteriaPanelOpen;

    public PlayerController PlayerController;

    void Start()
    {
        items = Resources.LoadAll<Item>("Item");
        GenerateItem();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isCafeteriaPanelOpen)
            {
                cafeteriaPanel.SetActive(false);
                isCafeteriaPanelOpen = false;
                PlayerController.isPanelOn = isCafeteriaPanelOpen;
            }
        }
    }

    public void OnCafeteriaPanel(PlayerController playerController)
    {
        isCafeteriaPanelOpen = !cafeteriaPanel.activeSelf;
        cafeteriaPanel.SetActive(isCafeteriaPanelOpen);

        PlayerController = playerController;
        PlayerController.isPanelOn = isCafeteriaPanelOpen;
    }

    public void GenerateItem()
    {
        for (int i = 0; i < items.Length; i++)
        {
            GameObject itemUIObj = Instantiate(itemUIPrefab, cafeteriaContent.transform);

            Image itemImage = itemUIObj.transform.GetChild(0).GetComponentInChildren<Image>();
            TextMeshProUGUI itemName = itemUIObj.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI itemPrice = itemUIObj.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            Button buyBtn = itemUIObj.transform.GetChild(3).GetComponentInChildren<Button>();

            int index = i;

            buyBtn.onClick.AddListener(() => OnBuyButtonClick(items[index]));

            if (itemUIObj != null && items[index] != null)
            {
                itemImage.sprite = items[index].icon;
                itemName.text = items[index].name;
                itemPrice.text = items[index].price.ToString();
            }
        }
    }

    [PunRPC]
    void RPC_Buy(string id)
    {
        Item item = Resources.Load<Item>($"Item/{id}");
        if (item != null)
        {
            GameObject itemObj = Instantiate(item.prefab, itemSpawnPoint.position, Quaternion.identity);
            itemObj.transform.localScale = Vector3.one;
        }
    }

    public void OnBuyButtonClick(Item item)
    {
        if (PlayerController.coin >= item.price)
        {
            PlayerController.coin -= item.price;

            photonView.RPC("RPC_Buy", RpcTarget.AllBuffered, item.id);
        }
    }
}
