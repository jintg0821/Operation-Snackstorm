using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class VendingMachine : MonoBehaviourPun
{
    public  Item[] availableItems; // 판매 아이템 목록
    public  GameObject vendingMachineUI; // UI Canva

    public bool vendingMachineOpen;

    public Transform itemSpawnPoint;

    public void Start()
    {
        availableItems = Resources.LoadAll<Item>("Item");
    }

    [PunRPC]
    void RPC_VenBuy(string id, int actorNumber)
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

    public void OnBuyButtonClick(Item item, PlayerController PlayerController)
    {
        if (PlayerController.coin >= item.price)
        {
            PlayerController.coin -= item.price;

            if (PlayerController.photonView.IsMine)
            {
                photonView.RPC("RPC_VenBuy", RpcTarget.All, item.id, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }
}