using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Camera cam;
    public float raycastRange = 100f;

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
    }

    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

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

        PerformRaycast();
    }
    void PerformRaycast()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastRange, Color.red, 1f);

        if (Physics.Raycast(ray, out hit, raycastRange))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (hit.collider.CompareTag("CafeteriaNPC"))
                {
                    Vector3 snackSpawnPoint = new Vector3(hit.collider.transform.position.x + 1f, hit.collider.transform.position.y, hit.collider.transform.position.z);

                    GameObject snackObj = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), snackSpawnPoint, Quaternion.identity);
                    snackObj.transform.localScale = Vector3.one * 0.3f;
                    Rigidbody snackRb = snackObj.AddComponent<Rigidbody>();
                    //snackRb.AddForce(Vector3.forward);
                }
            }
        }
    }
}
