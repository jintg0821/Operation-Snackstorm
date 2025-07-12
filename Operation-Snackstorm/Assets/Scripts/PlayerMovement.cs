using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public enum PlayerState { Idle, Walk, Run, Crouch }
    public PlayerState currentState = PlayerState.Idle;

    [Header("Move Speeds")]
    public float crouchSpeed = 2f;
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    private float moveSpeed;

    [Header("Look")]
    public float mouseSpeed;
    public float yRotation;
    public float xRotation;
    public Camera cam;
    public bool canLook = true;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();

        if (!photonView.IsMine)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(false);
            }
        }
        else
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(true);
            }
        }

        Cursor.lockState = CursorLockMode.Locked;   //마우스 커서를 화면 안에서 고정
        Cursor.visible = false;                     //마우스 커서를 보이지 않도록 설정
    }

    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        HandleState();
        CameraLook();
        PlayerMove();
    }

    void CameraLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSpeed * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSpeed * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void PlayerMove()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        Vector3 moveVec = transform.forward * Vertical + transform.right * Horizontal;

        transform.position += moveVec.normalized * moveSpeed * Time.deltaTime;
    }

    void HandleState()
    {
        bool moveInput =
            !Mathf.Approximately(Input.GetAxis("Horizontal"), 0f) ||
            !Mathf.Approximately(Input.GetAxis("Vertical"), 0f);

        bool runKey = Input.GetKey(KeyCode.LeftShift);
        bool crouchKey = Input.GetKey(KeyCode.LeftControl);

        if (crouchKey)
        {
            currentState = moveInput ? PlayerState.Crouch : PlayerState.Crouch;
        }
        else if (moveInput && runKey)
        {
            currentState = PlayerState.Run;
        }
        else if (moveInput)
        {
            currentState = PlayerState.Walk;
        }
        else
        {
            currentState = PlayerState.Idle;
        }


        switch (currentState)
        {
            case PlayerState.Idle:
                moveSpeed = 0f;
                break;
            case PlayerState.Walk:
                moveSpeed = walkSpeed;
                break;
            case PlayerState.Run:
                moveSpeed = runSpeed;
                break;
            case PlayerState.Crouch:
                moveSpeed = crouchSpeed;
                break;
        }
    }
}
