using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // ���� ���� ����
    }

    public void OnClickJoinGame()
    {
        if (string.IsNullOrEmpty(nicknameInput.text))
        {
            Debug.Log("�г����� �Է����ּ���!");
            return;
        }

        PhotonNetwork.NickName = nicknameInput.text;
        Debug.Log("�г��� ������: " + PhotonNetwork.NickName);

        // ������ ������ ����� ���¶�� �ٷ� �� ���� �õ�
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("���� �� ���� ����, �� ���� �����մϴ�.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("�� ���� ����, ���� ������ �̵�");
        PhotonNetwork.LoadLevel(gameSceneName);
    }
}
