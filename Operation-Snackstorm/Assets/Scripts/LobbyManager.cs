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
        PhotonNetwork.ConnectUsingSettings(); // 서버 연결 시작
    }

    public void OnClickJoinGame()
    {
        if (string.IsNullOrEmpty(nicknameInput.text))
        {
            Debug.Log("닉네임을 입력해주세요!");
            return;
        }

        PhotonNetwork.NickName = nicknameInput.text;
        Debug.Log("닉네임 설정됨: " + PhotonNetwork.NickName);

        // 마스터 서버에 연결된 상태라면 바로 방 참가 시도
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 방 참가 실패, 방 새로 생성합니다.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 참가 성공, 게임 씬으로 이동");
        PhotonNetwork.LoadLevel(gameSceneName);
    }
}
