using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StudentChatController : MonoBehaviourPun
{
    [SerializeField] private string[] chats;
    [SerializeField] private TextMeshPro[] texts;
    [SerializeField] private Transform textPos;
    public GameObject targetPlayer;

    [SerializeField] private float chatInterval = 15f;
    private double timerStartTime;

    void Start()
    {
        timerStartTime = PhotonNetwork.Time;
    }

    void Update()
    {
        double elapsed = PhotonNetwork.Time - timerStartTime;

        if (elapsed >= chatInterval)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                OnChat();
            }
            timerStartTime = PhotonNetwork.Time;
        }
    }

    void LateUpdate()
    {
        if (targetPlayer == null) return;

        Vector3 lookDir = (targetPlayer.transform.position - textPos.position);
        lookDir.y = 0;

        textPos.rotation = Quaternion.LookRotation(lookDir);
    }

    [PunRPC]
    public void RPC_SetTarget(int targetViewID)
    {
        PhotonView targetPV = PhotonView.Find(targetViewID);
        if (targetPV != null)
        {
            targetPlayer = targetPV.gameObject;
        }
    }

    void OnChat()
    {
        photonView.RPC("RPC_OnChat", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_OnChat()
    {
        int randomNum = Random.Range(0, chats.Length);

        StartCoroutine(Chat(chats[randomNum]));
    }

    private IEnumerator Chat(string chat)
    {
        foreach (var text in texts)
        {
            text.text = chat;
        }
        textPos.gameObject.SetActive(true);

        yield return new WaitForSeconds(5f);

        foreach (var text in texts)
        {
            text.text = "";
        }
        textPos.gameObject.SetActive(false);
    }
}
