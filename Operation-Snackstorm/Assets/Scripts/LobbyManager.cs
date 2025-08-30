using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private GameObject roomSlotPrefab;

    [SerializeField] private string gameSceneName = "GameScene";

    private Dictionary<string, GameObject> roomSlots = new Dictionary<string, GameObject>();


    void Start()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
        PhotonNetwork.ConnectUsingSettings(); // ���� ���� ����
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void SetNickName()
    {
        if (string.IsNullOrEmpty(nicknameInput.text))
        {
            Debug.Log("�г����� �Է����ּ���!");
            return;
        }

        PhotonNetwork.NickName = nicknameInput.text;
        Debug.Log("�г��� ������: " + PhotonNetwork.NickName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var slot in roomSlots.Values)
        {
            Destroy(slot);
        }
        roomSlots.Clear();

        int index = 1;
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList) continue;

            GameObject slot = Instantiate(roomSlotPrefab, roomListContent);
            slot.transform.Find("RoomNum").GetComponent<TextMeshProUGUI>().text = index.ToString("00");
            slot.transform.Find("RoomName").GetComponent<TextMeshProUGUI>().text = info.Name;
            slot.transform.Find("PlayerCount").GetComponent<TextMeshProUGUI>().text = info.PlayerCount + " / " + info.MaxPlayers;

            var joinBtn = slot.GetComponent<UnityEngine.UI.Button>();
            joinBtn.onClick.AddListener(() =>
            {
                if (PhotonNetwork.InLobby)
                {
                    PhotonNetwork.JoinRoom(info.Name);
                }
                else
                {
                    Debug.LogWarning("���� �κ� �������� �ʾҽ��ϴ�. ��� �� �ٽ� �õ��ϼ���.");
                }
            });

            roomSlots.Add(info.Name, slot);
            index++;
        }
    }

    public void CreateRoom()
    {
        string roomName = string.IsNullOrEmpty(roomNameInput.text)
            ? PhotonNetwork.NickName + "�� ��"
            : roomNameInput.text;

        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("�� ���� ����, ���� ������ �̵�");
        PhotonNetwork.LoadLevel(gameSceneName);
    }
    

}
