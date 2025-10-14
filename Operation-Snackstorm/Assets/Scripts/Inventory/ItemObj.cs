using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObj : MonoBehaviourPun
{
    public enum InteractionType { Item, Store, VendingMachine, AttendanceBook, NewsletterBox }
    public InteractionType type;

    //아이템
    public Item item;

    // UI를 띄우는 상호작용일 경우에만 사용 (상점, 자판기, 출석부)
    public GameObject targetUI;

    // 외부(PlayerController)에서 호출할 상호작용 함수
    public void Interact()
    {
        switch (type)
        {
            case InteractionType.Store:
                FindObjectOfType<InventoryUI>().ShowStore();
                break;
                
            case InteractionType.VendingMachine:
                FindObjectOfType<InventoryUI>().ShowVending();
                break;
                
            case InteractionType.AttendanceBook:
                if (targetUI != null)
                {
                    targetUI.SetActive(true);
                }
                break;
            case InteractionType.NewsletterBox:
                if (targetUI != null)
                {
                    targetUI.SetActive(true);
                }
                break;
        }
    }
    [PunRPC]
    public void RPC_RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}