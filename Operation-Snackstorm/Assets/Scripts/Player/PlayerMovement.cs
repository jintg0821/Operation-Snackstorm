using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState 
{ 
    Idle, 
    Walk, 
    Run, 
    Crouch,
    Skate,
    FallDown
}

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public PlayerState currentState = PlayerState.Idle;
    public bool miniGameStart = false;

    [Header("Move Speeds")]
    public float crouchSpeed = 2f;
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    private float skateSpeed = 10f;
    private float moveSpeed;

    [SerializeField] private GameObject skateboard;
    [SerializeField] private float rideYOffset = 0.1f;
    

    [SerializeField] private Transform playerBody;

    private float originalY;
    private Vector3 originalCenter;

    private float speedMultiplier = 1f;
    private Coroutine speedCoroutine;

    [Header("Gravity")]
    public float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Look")]
    public float mouseSpeed;
    public float yRotation;
    public float xRotation;
    public Camera cam;
    public bool canLook = true;

    [Header("AnimationID")]
    private int _animIDSpeed;
    private int _animIDJump;
    private int _animIDThrow;
    private int _animIDSkate;
    private int _animIDFallDown;

    [SerializeField] private Animator animator;
    private PlayerController playerController;
    private CharacterController characterController;

    void Start()
    {
        AssignAnimationIDs();
        cam = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

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
            playerController = GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        if (playerController.isInLibrary && currentState == PlayerState.Run && !TrashCleanupMission.Instance.isMissionActive && !playerController.isPunishmentImmune)
        {
            playerController.RequestPunishment();
        }

        if (!playerController.isPanelOn && !playerController.miniGameStart)
        {
            CameraLook();
            if (playerController.rideSkate)
                SkateMove();
            else
            {
                if (currentState != PlayerState.Skate && currentState != PlayerState.FallDown)
                    PlayerMove();
            }
        }

        if (playerController.miniGameStart)
        {
            CameraLook();
        }
        HandleState();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDThrow = Animator.StringToHash("isThrow");
        _animIDSkate = Animator.StringToHash("Skate");
        _animIDFallDown = Animator.StringToHash("FallDown");
    }

    void CameraLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSpeed * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSpeed * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void PlayerMove()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        Vector3 moveVec = transform.forward * Vertical + transform.right * Horizontal;

        characterController.Move(moveVec.normalized * (moveSpeed * speedMultiplier) * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void SkateMove()
    {
        Vector3 moveVec = transform.forward * skateSpeed * Time.deltaTime;
        characterController.Move(moveVec);

        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        animator.SetBool(_animIDSkate, true);
    }

    void HandleState()
    {
        bool moveInput =
            !Mathf.Approximately(Input.GetAxis("Horizontal"), 0f) ||
            !Mathf.Approximately(Input.GetAxis("Vertical"), 0f);

        bool runKey = Input.GetKey(KeyCode.LeftShift);
        bool crouchKey = Input.GetKey(KeyCode.LeftControl);

        if (!playerController.rideSkate && currentState != PlayerState.Skate && currentState != PlayerState.FallDown)
        {
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
                Debug.Log("1");
            }
        }

        if (currentState == PlayerState.FallDown)
        {
            if (!animator.GetBool(_animIDFallDown))
                animator.SetBool(_animIDFallDown, true);

            return;
        }

        if (playerController.rideSkate)
        {
            currentState = PlayerState.Skate;
            animator.SetBool(_animIDSkate, true);
            return;
        }

        float blendSpeed = 0f;

        switch (currentState)
        {
            case PlayerState.Idle:
                moveSpeed = 0f;
                blendSpeed = 0f;
                break;
            case PlayerState.Walk:
                moveSpeed = walkSpeed;
                blendSpeed = walkSpeed;
                break;
            case PlayerState.Run:
                moveSpeed = runSpeed;
                blendSpeed = runSpeed;
                break;
            case PlayerState.Crouch:
                moveSpeed = crouchSpeed;
                blendSpeed = crouchSpeed;
                break;
        }
        animator.SetFloat(_animIDSpeed, blendSpeed);
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        if (speedCoroutine != null)
            StopCoroutine(speedCoroutine);

        speedCoroutine = StartCoroutine(SpeedModifierRoutine(multiplier, duration));
    }

    private IEnumerator SpeedModifierRoutine(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
    }

    public void StartSkate()
    {
        if (playerController.rideSkate) return;

        originalCenter = characterController.center;

        skateboard.SetActive(true);

        Vector3 newCenter = originalCenter;
        newCenter.y += rideYOffset;
        characterController.center = newCenter;

        Vector3 bodyPos = playerBody.localPosition;
        bodyPos.y += rideYOffset;
        playerBody.localPosition = bodyPos;

        playerController.rideSkate = true;
        currentState = PlayerState.Skate;
        animator.SetBool(_animIDSkate, true);
    }

    public void OnFallDown()
    {
        playerController.rideSkate = false;
        currentState = PlayerState.FallDown;
        animator.SetBool(_animIDSkate, false);
        animator.SetBool(_animIDFallDown, true);

        if (playerController.photonView.IsMine)
        {
            playerController.photonView.RPC("RPC_DropRandomItem", RpcTarget.MasterClient);
        }
    }

    public void OnFallDownEnd()
    {
        characterController.center = originalCenter;
        playerBody.localPosition = Vector3.zero;

        skateboard.SetActive(false);

        animator.SetBool(_animIDSkate, false);
        animator.SetBool(_animIDFallDown, false);
        currentState = PlayerState.Idle;
        Debug.Log("2");
    }
}
