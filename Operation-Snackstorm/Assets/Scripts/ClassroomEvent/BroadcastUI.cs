using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BroadcastUI : MonoBehaviourPun
{
    [SerializeField] private GameObject broadcastPanel;
    public bool isOpen;
    public PlayerController PlayerController;


    void Start()
    {
        broadcastPanel = GameObject.Find("BroadcastPanel");

        broadcastPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen)
            {
                broadcastPanel.SetActive(false);
                isOpen = false;
                PlayerController.isPanelOn = isOpen;
            }
        }
    }

    public void OnBroadcastPanel(PlayerController player)
    {
        isOpen = !broadcastPanel.activeSelf;
        broadcastPanel.SetActive(isOpen);

        PlayerController = player;
        PlayerController.isPanelOn = isOpen;
    }

    public void OnBroadcastButtonClick(int type)
    {
        if (BroadcastManager.Instance.canBroadcast)
        {
            BroadcastManager.Instance.IssueCommand((CommandType)type);
        }
    }
}
