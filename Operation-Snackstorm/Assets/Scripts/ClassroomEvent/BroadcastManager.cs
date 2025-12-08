using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public enum CommandType { None, Walk, Run, Idle }

public class BroadcastManager : MonoBehaviourPun
{
    public static BroadcastManager Instance;

    public CommandType currentCommand;
    public bool isCommanding = false;
    public float commandDuration = 5f;
    private float timer;

    public TextMeshProUGUI broadcastText;

    public bool canBroadcast;

    private BroadcastUI BroadcastUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BroadcastUI = GetComponent<BroadcastUI>();
    }

    [PunRPC]
    public void RPC_IssueCommand(int senderID, int commandType) 
    {
        currentCommand = (CommandType)commandType;
        timer = commandDuration;
        
        switch (commandType)
        {
            case 1:
                CommandText("걸어주세여", 4f);
                break;

            case 2:
                CommandText("뛰어주세여", 4f);
                break;

            case 3:
                CommandText("멈춰주세여", 4f);
                break;
        }

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

        StartCoroutine(Broadcast());
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

    public void CommandText(string text, float n)
    {
        broadcastText.text = text;
        StartCoroutine(ResetText(n));
    }

    IEnumerator ResetText(float n)
    {
        yield return new WaitForSeconds(n);
        broadcastText.text = "";
    }

    IEnumerator Broadcast()
    {
        canBroadcast = false;

        yield return new WaitForSeconds(40f);

        canBroadcast = true;
    }    
}
