using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cafeteria : MonoBehaviourPun
{
    private Item[] allItems;
    public List<Item> cafeteriaItems;
    
    public GameObject cafeteriaPanel;
    public GameObject cafeteriaContent;
    public Transform itemSpawnPoint;


    private Dictionary<string, int> originalPrices = new Dictionary<string, int>();
    bool discountApplied = false;

    [Header("Item")]
    public GameObject itemUIPrefab;

    public bool isCafeteriaPanelOpen;

    public PlayerController PlayerController;

    void Start()
    {
        allItems = Resources.LoadAll<Item>("Item");
        foreach (Item item in allItems)
        {
            if (item.category != "ÀÚÆÇ±â")
            {
                cafeteriaItems.Add(item);
            }
        }

        foreach (Item item in cafeteriaItems)
        {
            originalPrices[item.id] = item.price;
        }

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

        if (PlayerController.artVIPCard && !discountApplied)
        {
            ApplyDiscount(2);
        }
        else if (!PlayerController.artVIPCard && discountApplied)
        {
            RestoreOriginalPrices();
        }

        RefreshUI();
    }

    void ApplyDiscount(int amount)
    {
        foreach (Item item in cafeteriaItems)
        {
            item.price = Mathf.Max(0, item.price - amount);
        }
        discountApplied = true;
    }

    void RestoreOriginalPrices()
    {
        foreach (Item item in cafeteriaItems)
        {
            if (originalPrices.ContainsKey(item.id))
            {
                item.price = originalPrices[item.id];
            }
        }
        discountApplied = false;
    }

    void RefreshUI()
    {
        foreach (Transform child in cafeteriaContent.transform)
        {
            Destroy(child.gameObject);
        }
        GenerateItem();
    }

    public void GenerateItem()
    {
        for (int i = 0; i < cafeteriaItems.Count; i++)
        {
            GameObject itemUIObj = Instantiate(itemUIPrefab, cafeteriaContent.transform);

            Image itemImage = itemUIObj.transform.GetChild(0).GetComponentInChildren<Image>();
            TextMeshProUGUI itemName = itemUIObj.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI itemPrice = itemUIObj.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            Button buyBtn = itemUIObj.transform.GetChild(3).GetComponentInChildren<Button>();

            int index = i;

            buyBtn.onClick.AddListener(() => OnBuyButtonClick(cafeteriaItems[index]));

            if (itemUIObj != null && cafeteriaItems[index] != null)
            {
                itemImage.sprite = cafeteriaItems[index].icon;
                itemName.text = cafeteriaItems[index].name;
                itemPrice.text = cafeteriaItems[index].price.ToString();
            }
        }
    }

    [PunRPC]
    void RPC_Buy(string id, int actorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            Item item = Resources.Load<Item>($"Item/{id}");
            if (item != null)
            {
                GameObject itemObj = PhotonNetwork.Instantiate($"Prefabs/Items/{item.prefab.name}", itemSpawnPoint.position, Quaternion.identity);

                itemObj.transform.localScale = Vector3.one;

                PhotonView itemPV = itemObj.GetComponent<PhotonView>();
                if (itemPV != null)
                {
                    itemPV.TransferOwnership(PhotonNetwork.MasterClient);
                }
            }
        }
    }

    public void OnBuyButtonClick(Item item)
    {
        if (PlayerController.coin >= item.price)
        {
            PlayerController.SubtractCoin(item.price);

            if (PlayerController.photonView.IsMine) 
            {
                photonView.RPC("RPC_Buy", RpcTarget.All, item.id, PhotonNetwork.LocalPlayer.ActorNumber);
            }

            if (PlayerController.artVIPCard && discountApplied)
            {
                RestoreOriginalPrices();
                RefreshUI();
                PlayerController.artVIPCard = false;
            }
        }
    }
}
