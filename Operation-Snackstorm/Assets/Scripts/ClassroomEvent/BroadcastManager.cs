using Photon.Pun;
using UnityEngine;

public enum CommandType { None, Walk, Run, Idle }

public class BroadcastManager : MonoBehaviourPun
{
    public static BroadcastManager Instance;

    public CommandType currentCommand;
    public bool isCommanding = false;
    public float commandDuration = 5f;
    private float timer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void RPC_IssueCommand(int senderID, int commandType)
    {
        currentCommand = (CommandType)commandType;
        timer = commandDuration;
        Debug.Log($"방송 명령 : {(CommandType)commandType}");

        foreach (var player in FindObjectsOfType<PlayerCommandHandler>())
        {
            if (player.photonView.IsMine && player.photonView.OwnerActorNr != senderID)
            {
                player.ReceiveCommand(currentCommand);
                Debug.Log("안시현");
                break;
            }
        }
    }

    private void Update()
    {
        if (currentCommand != CommandType.None)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                currentCommand = CommandType.None;
                isCommanding = false;
                Debug.Log("명령 종료");
            }
            else
                isCommanding = true;
        }
    }

    public void IssueCommand(CommandType cmd)
    {
        if (!isCommanding)
        {
            photonView.RPC("RPC_IssueCommand", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, (int)cmd);
        }
    }
}
