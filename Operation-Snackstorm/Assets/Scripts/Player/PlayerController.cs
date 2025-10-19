using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public bool test = false;

    public Camera cam;
    public float raycastRange = 100f;

    public int coin = 100;

    public int roundPoint;
    public int bonusPoint;
    public int minusPoint;

    public int totalPoint;

    public bool isPanelOn = false;

    [SerializeField] private float wallTime;
    private bool isFireExtinguisherExplode;

    public bool isCatchable = true;

    public Inventory inventory;
    private VendingMachineUI VendingMachineUI;
    private VendingMachine vendingMachine;
    private Cafeteria cafeteria;
    public CharacterController characterController;
    private TestHotbar testHotbar;
    private WaterDispenser waterDispenser;

    public bool isInLibrary = false;
    public float runningSpeedThreshold = 3.0f;
    private bool hasBeenPunished = false;
    public bool isPunishmentImmune = false;

    [SerializeField]
    private TextMeshProUGUI penaltyText;

    private GameObject currentInteractableObject;

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
            inventory = GetComponent<Inventory>();
            cafeteria = FindObjectOfType<Cafeteria>();
            vendingMachine = FindObjectOfType<VendingMachine>();
            VendingMachineUI = FindObjectOfType<VendingMachineUI>();
            characterController = GetComponent<CharacterController>();
            testHotbar = FindObjectOfType<TestHotbar>();
            waterDispenser = FindObjectOfType<WaterDispenser>();
        }
        if (penaltyText != null)
        {
            penaltyText.gameObject.SetActive(false);
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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                VendingMachineUI.OnvendingMachinePanel(this);
            }

            PerformRaycast();
        }

        if (test)
        {
            Item[] items = Resources.LoadAll<Item>("Item");


            if (!isPanelOn)
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    GetBonusPoint(10);
                }
                if (Input.GetKeyDown(KeyCode.K))
                {
                    GetMinusPoint(10);
                }
                if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    int RandNum = Random.Range(0, items.Length);
                    inventory.AddItem(items[RandNum]);
                }
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    if (waterDispenser != null) 
                        waterDispenser.photonView.RPC("RPC_AssignRoleAndStart", RpcTarget.All, photonView.ViewID);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    testHotbar.ChangeItem(0);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    testHotbar.ChangeItem(1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    testHotbar.ChangeItem(2);
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    testHotbar.ChangeItem(3);
                }
            }
        }

        SetCursorState(isPanelOn);
    }

    public void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
        if (!isVisible)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void PerformRaycast()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        //Debug.DrawRay(ray.origin, ray.direction * raycastRange, Color.red, 1f);

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
                    ItemObj itemObj = hit.collider.gameObject.GetComponentInParent<ItemObj>();
                    if (itemObj != null)
                    {
                        inventory.AddItem(itemObj.item);

                        PhotonView itemPV = itemObj.GetComponent<PhotonView>();
                        if (itemPV != null)
                        {
                            itemPV.RPC("RPC_RequestDestroy", RpcTarget.MasterClient);
                        }
                    }
                }

                if (hit.collider.CompareTag("Door"))
                {
                    DoorController door = hit.collider.GetComponent<DoorController>();
                    door.ToggleDoor();
                }

                //if (hit.collider.CompareTag("AttendanceBook"))
                //{
                //    ItemObj itemObj = hit.collider.GetComponent<ItemObj>();
                //    if (itemObj != null)
                //    {
                //        itemObj.Interact();
                //    }
                //}

                //if (hit.collider.CompareTag("NewsletterBox"))
                //{
                //    ItemObj itemObj = hit.collider.GetComponent<ItemObj>();
                //    if (itemObj != null)
                //    {
                //        itemObj.Interact();
                //    }
                //}

                //if (hit.collider.CompareTag("Trash"))
                //{
                //    TrashObject trash = hit.collider.GetComponent<TrashObject>();
                //    if (trash != null)
                //    {
                //        trash.Interact();
                //    }
                //}
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
    }

    private IEnumerator WallCoolTime()
    {
        yield return new WaitForSeconds(wallTime);
        isFireExtinguisherExplode = false;
    }

    [PunRPC]
    private void RPC_SetCatchable(bool value)
    {
        isCatchable = value;
    }

    [PunRPC]
    private void RPC_RemoveRandomItemFromInventory()
    {
        if (inventory != null && inventory.items.Count > 0)
        {
            int randNum = Random.Range(0, inventory.items.Count);
            Item item = inventory.items[randNum];
            if (item != null)
            {
                inventory.RemoveItem(item);
            }
            else
            {
                Debug.LogWarning($"Random item not found.");
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isCatchable);
        }
        else
        {
            isCatchable = (bool)stream.ReceiveNext();
        }
    }

    public void GetRoundPoint()
    {
        if (inventory == null || inventory.items == null)
            return;

        roundPoint = 0;
        List<Item> itemsCopy = new List<Item>(inventory.items);
        foreach (Item item in itemsCopy)
        {
            roundPoint += item.point;
            inventory.RemoveItem(item);
        }
        itemsCopy.Clear();
        inventory.items.Clear();

        var hash = new ExitGames.Client.Photon.Hashtable();
        hash["RoundPoint"] = roundPoint;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        totalPoint += roundPoint;
    }

    public void GetBonusPoint(int point)
    {
        bonusPoint += point;

        var hash = new ExitGames.Client.Photon.Hashtable();
        hash["BonusPoint"] = bonusPoint;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void GetMinusPoint(int point)
    {
        minusPoint += point;

        var hash = new ExitGames.Client.Photon.Hashtable();
        hash["MinusPoint"] = minusPoint;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void UpdateTotalPoint()
    {
        int roundPoint = this.roundPoint;

        int accumulatedRoundPoint = 0;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("AccumulatedRoundPoint"))
        {
            accumulatedRoundPoint = (int)PhotonNetwork.LocalPlayer.CustomProperties["AccumulatedRoundPoint"];
        }

        accumulatedRoundPoint += roundPoint;

        int bonusPoint = this.bonusPoint;
        int minusPoint = this.minusPoint;

        int totalPoint = accumulatedRoundPoint + bonusPoint - minusPoint;

        var hash = new ExitGames.Client.Photon.Hashtable();
        hash["AccumulatedRoundPoint"] = accumulatedRoundPoint;
        hash["BonusPoint"] = bonusPoint;
        hash["MinusPoint"] = minusPoint;
        hash["TotalPoint"] = totalPoint;

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void EnterLibraryZone(bool isEntering)
    {
        isInLibrary = isEntering;
        if (!isEntering)
        {
            hasBeenPunished = false;
        }
    }

    //µµ¼­°ü¿¡¼­¶Û¶§
    public void RequestPunishment()
    {
        photonView.RPC("RPC_RequestPunishment", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_RequestPunishment()
    {
        GameManager.Instance.StartPunishment(photonView.ViewID);
    }

    public void ApplyPenaltyPoints(int points)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();

        int currentPoints = 0;
        if (photonView.Owner.CustomProperties.ContainsKey("MinusPoint"))
        {
            currentPoints = (int)photonView.Owner.CustomProperties["MinusPoint"];
        }

        currentPoints += points;
        hash.Add("MinusPoint", currentPoints);

        photonView.Owner.SetCustomProperties(hash);
    }

    public void ShowPenaltyText(string message, float duration)
    {
        photonView.RPC("RPC_ShowPenaltyText", RpcTarget.All, message, duration);
    }

    [PunRPC]
    private void RPC_ShowPenaltyText(string message, float duration)
    {
        StartCoroutine(PenaltyTextCoroutine(message, duration));
    }

    private IEnumerator PenaltyTextCoroutine(string message, float duration)
    {
        penaltyText.text = message;
        penaltyText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        penaltyText.gameObject.SetActive(false);
    }

    public void AddNextRoundCoin(int amount)
    {
        UpdateCustomProperty("NextRoundBonusCoin", amount);
    }

    public void AddCoin(int amount)
    {
        UpdateCustomProperty("Coin", amount);
    }

    private void UpdateCustomProperty(string key, int amount)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();

        int currentValue = 0;
        if (photonView.Owner.CustomProperties.ContainsKey(key))
        {
            currentValue = (int)photonView.Owner.CustomProperties[key];
        }

        currentValue += amount;
        hash.Add(key, currentValue);

        photonView.Owner.SetCustomProperties(hash);
    }
    public void GrantPunishmentImmunity(float duration)
    {
        StartCoroutine(PunishmentImmunityCoroutine(duration));
    }

    private IEnumerator PunishmentImmunityCoroutine(float duration)
    {
        isPunishmentImmune = true;
        yield return new WaitForSeconds(duration);
        isPunishmentImmune = false;
    }
}