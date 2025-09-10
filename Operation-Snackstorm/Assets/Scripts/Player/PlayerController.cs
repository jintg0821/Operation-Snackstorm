using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Camera cam;
    public float raycastRange = 100f;
    public int coin = 100;
    public int point;
    public bool isPanelOn = false;

    [SerializeField] private float wallTime;
    private bool isFireExtinguisherExplode;

    private Inventory inventory;
    private Cafeteria cafeteria;
    private CharacterController characterController;

    private void Awake()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "Player" + photonView.ViewID;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        cam = GetComponentInChildren<Camera>();

        if (PhotonNetwork.IsConnected && photonView != null)
        {
            GameManager.Instance.RegisterPlayer(photonView);
        }
        if (photonView.IsMine)
        {
            inventory = FindObjectOfType<Inventory>();
            cafeteria = FindObjectOfType<Cafeteria>();
            characterController = GetComponent<CharacterController>();
        }
        
    }

    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        if (!isPanelOn)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    GameManager.Instance.GameStart();
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                BroadcastManager.Instance.IssueCommand(CommandType.Walk);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                inventory.OnInventoryPanel(this);
            }

            PerformRaycast();
        }

        SetCursorState(isPanelOn);
    }

    public void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
        if (!isVisible)
        {
            Cursor.lockState = CursorLockMode.Locked; // ?????? ???? ?????? ????
        }
        else
        {
            Cursor.lockState = CursorLockMode.None; // ?????? ?????? ?????? ???? ???????? ??????
        }
    }

    void PerformRaycast()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastRange, Color.red, 1f);

        if (Physics.Raycast(ray, out hit, raycastRange))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (hit.collider.CompareTag("CafeteriaNPC"))
                {
                    cafeteria = hit.collider.gameObject.GetComponent<Cafeteria>();
                    if (cafeteria != null)
                    {
                        cafeteria.OnCafeteriaPanel(this);
                    }
                }

                if (hit.collider.CompareTag("Item"))
                {
                    ItemObj item = hit.collider.gameObject.GetComponent<ItemObj>();
                    inventory.AddItem(item.item);

                    PhotonView itemPV = item.GetComponent<PhotonView>();
                    itemPV.RPC("RPC_RequestDestroyItem", RpcTarget.MasterClient);
                }

                if (hit.collider.CompareTag("Door"))
                {
                    DoorController door = hit.collider.GetComponent<DoorController>();
                    door.ToggleDoor();
                }
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Wall"))
        {
            if (!isFireExtinguisherExplode)
            {
                isFireExtinguisherExplode = true;
                hit.gameObject.GetComponent<WallTrigger>().WallFireExtinguisherExplode();
                StartCoroutine(WallCoolTime());
            }
        }

        if (hit.gameObject.CompareTag("NPC"))
        {
            characterController.enabled = false;
            gameObject.transform.position = GameManager.Instance.spawnPoint.position;
            characterController.enabled = true;
        }
    }

    private IEnumerator WallCoolTime()
    {
        yield return new WaitForSeconds(wallTime);
        isFireExtinguisherExplode = false;
    }

    public void GetPoint()
    {
        if (inventory == null || inventory.items == null)
            return;

        List<Item> itemsCopy = new List<Item>(inventory.items);
        foreach (Item item in itemsCopy)
        {
            point += item.point;
            inventory.RemoveItem(item);
        }
        itemsCopy.Clear();
        inventory.items.Clear();

        var hash = new ExitGames.Client.Photon.Hashtable();
        hash["Point"] = point;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
}