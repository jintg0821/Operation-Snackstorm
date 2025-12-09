using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class VendingMachine : MonoBehaviourPun
{
    private Item[] allItems;
    public List<Item> availableItems; // 판매 아이템 목록

    public bool vendingMachineOpen;

    public Transform itemSpawnPoint; 
    public GameObject vendingMachineUI;

    [Header("UI")]
    [SerializeField] private Transform slotParent; // 슬롯 부모 (GridLayoutGroup)
    [SerializeField] private GameObject slotPrefab; // 슬롯 Prefab

    [SerializeField] private PlayerController PlayerController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    public void Start()
    {
        allItems = Resources.LoadAll<Item>("Item");

        foreach (Item item in allItems)
        {
            if (item.category != "매점")
            {
                availableItems.Add(item);
            }
        }
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

    public void GenerateItem()
    {
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < availableItems.Count; i++)
        {
            GameObject itemUIObj = Instantiate(slotPrefab, slotParent.transform);

            Image itemImage = itemUIObj.transform.GetChild(0).GetComponentInChildren<Image>();
            TextMeshProUGUI itemName = itemUIObj.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI itemPrice = itemUIObj.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            Button buyBtn = itemUIObj.transform.GetChild(3).GetComponentInChildren<Button>();

            int index = i;

            buyBtn.onClick.AddListener(() => OnBuyButtonClick(availableItems[index]));

            if (itemUIObj != null && availableItems[index] != null)
            {
                itemImage.sprite = availableItems[index].icon;
                itemName.text = availableItems[index].name;
                itemPrice.text = availableItems[index].price.ToString();
            }
        }
    }

    public void OnvendingMachinePanel(PlayerController playerController)
    {
        vendingMachineOpen = !vendingMachineUI.activeSelf;
        vendingMachineUI.SetActive(vendingMachineOpen);
        GenerateItem();

        PlayerController = playerController;
        PlayerController.isPanelOn = vendingMachineOpen;
    }

    [PunRPC]
    void RPC_VenBuy(string id, int actorNumber, int vendingViewID)
    {
        PhotonView targetView = PhotonView.Find(vendingViewID);
        if (targetView == null) return;

        VendingMachine targetVM = targetView.GetComponent<VendingMachine>();
        if (targetVM == null) return;

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            Item item = Resources.Load<Item>($"Item/{id}");
            if (item != null)
            {
                GameObject itemObj = PhotonNetwork.Instantiate($"Prefabs/Items/{item.prefab.name}",targetVM.itemSpawnPoint.position,Quaternion.identity);

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
                photonView.RPC("RPC_VenBuy", RpcTarget.All, item.id, PhotonNetwork.LocalPlayer.ActorNumber, photonView.ViewID);
                audioSource.PlayOneShot(audioClip);
            }
        }
    }
}