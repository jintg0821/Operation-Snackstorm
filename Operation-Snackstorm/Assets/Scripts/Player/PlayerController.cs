using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public bool test = false;

    public Camera fpsCam;
    public Camera tpsCam;
    public float raycastRange = 100f;

    public int coin = 100;

    public int roundPoint;
    public int bonusPoint;
    public int minusPoint;

    public int totalPoint;

    public bool isPanelOn = false;
    public bool miniGameStart = false;
    public bool rideSkate = false;

    [Header("Mop")]
    [SerializeField] private GameObject MopObj;
    [SerializeField] private Transform mop;
    [SerializeField] private Vector3 defaultMopPos;
    [SerializeField] private Vector3 defaultMopRot;
    [SerializeField] private Vector3 attackMopPos;
    [SerializeField] private Vector3 attackMopRot;
    [SerializeField] private Vector3 mopPos;
    [SerializeField] private Vector3 mopRot;

    public bool isHoldingMop = false;
    public bool isMopping = false;
    public bool isAttacking = false;
    public bool isHit = false;
    public bool isFallDown = false;

    [SerializeField] private float wallTime;
    private bool isFireExtinguisherExplode;

    [SerializeField] private Transform itemPos;
    [SerializeField] private GameObject handItem;
    [SerializeField] private float throwForce;
    [SerializeField] private float throwUpwardForce;
    [SerializeField] private bool throwing;

    public bool isCatchable = true;

    public Inventory inventory;
    private Cafeteria cafeteria;
    public CharacterController characterController;
    public PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimController playerAnimController;
    private TestHotbar testHotbar;
    private WaterDispenser waterDispenser;
    private ArtClassroom art;

    public bool isInLibrary = false;
    public float runningSpeedThreshold = 3.0f;
    private bool hasBeenPunished = false;
    public bool isPunishmentImmune = false;
    public bool artVIPCard = false;
    public bool isWaterDispenser = false;

    [SerializeField]
    private TextMeshProUGUI penaltyText;

    private GameObject currentInteractableObject;

    private void Awake()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "Player" + photonView.ViewID;
        }

        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.TagObject = this;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (PhotonNetwork.IsConnected && photonView != null)
        {
            GameManager.Instance.RegisterPlayer(photonView);
        }

        playerMovement = GetComponent<PlayerMovement>();
        playerAnimController = GetComponent<PlayerAnimController>();
        characterController = GetComponent<CharacterController>();
        if (photonView.IsMine)
        {
            inventory = GetComponent<Inventory>();
            cafeteria = FindObjectOfType<Cafeteria>();
            testHotbar = FindObjectOfType<TestHotbar>();
            waterDispenser = FindObjectOfType<WaterDispenser>();
            art = FindObjectOfType<ArtClassroom>();
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
                if (!isHoldingMop && !rideSkate && !throwing && handItem != null)
                {
                    Throwing();
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                inventory.OnInventoryPanel(this);
            }

            PerformRaycast();
        }

        if (test)
        {
            Item[] items = Resources.LoadAll<Item>("Item");


            if (!isPanelOn)
            {
                if (isHoldingMop)
                {
                    if (!isAttacking && !isMopping)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Attack();
                        }
                        if (Input.GetMouseButtonDown(1))
                        {
                            Mopping();
                        }
                    }
                }

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
                    if (waterDispenser != null && isWaterDispenser)
                        waterDispenser.photonView.RPC("RPC_AssignRoleAndStart", RpcTarget.All, photonView.ViewID);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (!isHoldingMop)
                    {
                        testHotbar.ChangeItem(0);
                        playerMovement.StartSkate();
                    }
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (!rideSkate)
                    {
                        testHotbar.ChangeItem(1);
                        photonView.RPC("RPC_ToggleMop", RpcTarget.AllBuffered);
                    }
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
        if (fpsCam == null) return;

        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
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

                if (hit.collider.CompareTag("Coin"))
                {
                    Coin coinObj = hit.collider.gameObject.GetComponent<Coin>();
                    if (coinObj != null)
                    {
                        AddCoin(coinObj.value);
                    }

                    PhotonView coinPV = coinObj.GetComponent<PhotonView>();
                    if (coinPV != null)
                    {
                        coinPV.RPC("RPC_RequestCoinDestroy", RpcTarget.MasterClient);
                        Debug.Log("343");
                    }
                }

                if (hit.collider.CompareTag("VendingMachine"))
                {
                    VendingMachine vendingMachine = hit.collider.GetComponent<VendingMachine>();
                    if (vendingMachine != null)
                        vendingMachine.OnvendingMachinePanel(this);
                }

                if (hit.collider.CompareTag("Door"))
                {
                    DoorController door = hit.collider.GetComponent<DoorController>();
                    door.ToggleDoor();
                }

                if (hit.collider.CompareTag("Art"))
                {
                    art.TryAnswer(hit.collider.gameObject);
                }

                if (hit.collider.CompareTag("Broadcast"))
                {
                    BroadcastUI broadcast = hit.collider.GetComponent<BroadcastUI>();
                    if (broadcast != null)
                        broadcast.OnBroadcastPanel(this);
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

    [PunRPC]
    public void RPC_ToggleMop()
    {
        MopObj.SetActive(!isHoldingMop);
        isHoldingMop = !isHoldingMop;
    }

    void Mopping()
    {
        isMopping = true;

        if (playerAnimController != null)
        {
            MopObj.transform.localRotation = Quaternion.Euler(mopRot);
            MopObj.transform.localPosition = mopPos;
            playerAnimController.SetMop(isMopping);
        }  
    }

    public void MoppingEnd()
    {
        if (!photonView.IsMine) return;
        isMopping = false;
        if (playerAnimController != null)
            playerAnimController.SetMop(isMopping);

        MopObj.transform.localPosition = defaultMopPos;
        MopObj.transform.localRotation = Quaternion.Euler(defaultMopRot);
    }

    void Attack()
    {
        isAttacking = true;
        if (playerAnimController != null)
        {
            MopObj.transform.localRotation = Quaternion.Euler(attackMopRot);
            MopObj.transform.localPosition = attackMopPos;
            playerAnimController.SetAttack(isAttacking);
        }  
    }

    public void AttackEnd()
    {
        if (!photonView.IsMine) return;
        isAttacking = false;
        if (playerAnimController != null)
            playerAnimController.SetAttack(isAttacking);

        MopObj.transform.localPosition = defaultMopPos;
        MopObj.transform.localRotation = Quaternion.Euler(defaultMopRot);
    }

    public void Hit()
    {
        if (isHit) return;

        isHit = true;
        if (photonView.IsMine && playerMovement != null)
            playerMovement.OnFallDown();
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
        if (hit.gameObject.layer == 8 || hit.gameObject.layer == 6)
        {
            if (playerMovement != null && rideSkate)
                photonView.RPC("RPC_SelfHit", RpcTarget.All);
        }
    }

    public void PickItem(string itemName)
    {
        if (!photonView.IsMine) return;

        if (handItem != null)
            PhotonNetwork.Destroy(handItem);

        handItem = PhotonNetwork.Instantiate($"Prefabs/Items/{itemName}", itemPos.position, itemPos.rotation);

        handItem.transform.SetParent(itemPos);

        ItemObj itemObj = handItem.GetComponent<ItemObj>();
        if (itemObj != null)
        {
            itemObj.SetHeld(true);
            testHotbar.UpdateHeldItemUI(itemObj.item.icon);
        } 
    }

    public void Throwing()
    {
        throwing = true;
        playerAnimController.SetThrow(true);
    }

    public void ThrowItem()
    {
        if (!photonView.IsMine) return;
        if (handItem == null) return;

        ItemObj itemObj = handItem.GetComponent<ItemObj>();
        PhotonView itemPV = handItem.GetComponent<PhotonView>();

        if (itemObj == null || itemPV == null) return;

        handItem.transform.SetParent(null);

        inventory.RemoveItem(itemObj.item);
        testHotbar.UpdateHeldItemUI(null);

        Vector3 throwDirection = fpsCam.transform.forward;
        Vector3 throwVelocity = throwDirection * throwForce + Vector3.up * throwUpwardForce;

        object[] data = new object[] { throwVelocity };
        itemPV.RPC("RPC_Throw", RpcTarget.All, data);

        handItem = null;
        itemPV.TransferOwnership(PhotonNetwork.MasterClient);

        playerAnimController.SetThrow(false);
        throwing = false;
    }


    [PunRPC]
    void RPC_SelfHit()
    {
        Hit();
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

    [PunRPC]
    private void RPC_DropRandomItem()
    {
        if (inventory != null && inventory.items.Count > 0)
        {
            int randNum = Random.Range(0, inventory.items.Count);
            Item item = inventory.items[randNum];
            if (item != null)
            {
                float dropRadius = 2f;

                Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
                Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
                Vector3 itemDropPoint = transform.position + randomOffset + Vector3.up * 0.5f;

                GameObject itemObj = PhotonNetwork.Instantiate($"Prefabs/Items/{item.prefab.name}", itemDropPoint, Quaternion.identity);
                inventory.RemoveItem(item);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isCatchable);
            stream.SendNext(isAttacking);
            stream.SendNext(isMopping);
            stream.SendNext(isHit);
        }
        else
        {
            isCatchable = (bool)stream.ReceiveNext();
            isAttacking = (bool)stream.ReceiveNext();
            isMopping = (bool)stream.ReceiveNext();
            isHit = (bool)stream.ReceiveNext();
        }
        if (!stream.IsWriting && playerAnimController != null)
        {
            playerAnimController.SetAttack(isAttacking);
            playerAnimController.SetMop(isMopping);
            playerAnimController.SetFallDown(isHit);
        }
    }

    public void GetRoundPoint(bool inPointArea)
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

        if (inPointArea)
        {
            var hash = new ExitGames.Client.Photon.Hashtable();
            hash["RoundPoint"] = roundPoint;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        else
        {
            var hash = new ExitGames.Client.Photon.Hashtable();
            hash["RoundPoint"] = 0;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        itemsCopy.Clear();
        inventory.items.Clear();
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


    public void AddCoin(int amount)
    {
        coin += amount;

        if (testHotbar != null && testHotbar.slots != null && testHotbar.slots.Length > 2)
        {
            var coinSlot = testHotbar.slots[2];
            coinSlot.SetAmount(coin);
        }
    }

    public void SubtractCoin(int amount)
    {
        coin -= amount;

        if (testHotbar != null && testHotbar.slots != null && testHotbar.slots.Length > 2)
        {
            var coinSlot = testHotbar.slots[2];
            coinSlot.SetAmount(coin);
        }
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