using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Camera cam;
    public float raycastRange = 100f;
    public int coin;
    public bool isPanelOn = false;

    private Inventory inventory;
    private Cafeteria cafeteria;

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

        if (photonView.IsMine)
        {
            inventory = FindObjectOfType<Inventory>();
            cafeteria = FindObjectOfType<Cafeteria>();
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
                    Cafeteria cafeteria = hit.collider.gameObject.GetComponent<Cafeteria>();
                    if (cafeteria != null)
                    {
                        cafeteria.OnCafeteriaPanel(this);
                    }
                }

                if (hit.collider.CompareTag("Item"))
                {
                    ItemObj item = hit.collider.gameObject.GetComponent<ItemObj>();
                    inventory.AddItem(item.item);   
                    Destroy(item.gameObject);
                }
            }

        }
    }
}
