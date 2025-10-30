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
    public float skateSpeed = 10f;
    [SerializeField] private float moveSpeed;

    [SerializeField] private GameObject skateboard;
    [SerializeField] private Transform skateboardPos;
    [SerializeField] private float rideYOffset = 0.1f;

    [SerializeField] private Transform playerBody;

    private float originalY;
    private Vector3 originalCenter;

    private float speedMultiplier = 1f;
    private Coroutine speedCoroutine;

    [Header("Audio")]
    public AudioClip[] FootstepAudioClips;
    public AudioSource footstepSource;

    [Header("Gravity")]
    public float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Look")]
    public float mouseSpeed;
    public float yRotation;
    public float xRotation;
    public bool canLook = true;

    [Header("AnimationID")]
    private int _animIDSpeed;
    private int _animIDJump;
    private int _animIDThrow;
    private int _animIDSkate;
    private int _animIDFallDown;

    private PlayerController playerController;
    private CharacterController characterController;
    private PlayerAnimController playerAnimController;
    private Camera activeCam;
    

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimController = GetComponent<PlayerAnimController>();
        playerController = GetComponent<PlayerController>();

        if (!photonView.IsMine)
        {
            if (playerController.fpsCam != null)
                playerController.fpsCam.gameObject.SetActive(false);
            if (playerController.tpsCam != null)
                playerController.tpsCam.gameObject.SetActive(false);
        }
        else
        {
            SetCameraMode(true);
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

        if (playerController.fpsCam != null && playerController.tpsCam != null)
        {
            if (playerController.fpsCam.gameObject.activeSelf)
            {
                if (playerController.rideSkate)
                {
                    SetCameraMode(false);
                    Debug.Log("TPS");
                }
                if (playerController.isFallDown)
                {
                    SetCameraMode(false);
                    Debug.Log("TPS");
                }
            }

            if (playerController.tpsCam.gameObject.activeSelf)
            {
                if (!playerController.isFallDown && !playerController.rideSkate)
                {
                    SetCameraMode(true);
                    Debug.Log("FPS");
                }
            }
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

    void CameraLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSpeed * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSpeed * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        activeCam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void PlayerMove()
    {
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");

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

        playerAnimController.SetSkate(true);
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
            }
        }

        if (currentState == PlayerState.FallDown)
        {
            playerAnimController.SetSkate(false);
            if (!playerAnimController.GetFallDown())
                playerAnimController.SetFallDown(true);

            return;
        }

        if (playerController.rideSkate)
        {
            currentState = PlayerState.Skate;
            playerAnimController.SetSkate(true);
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
        playerAnimController.SetSpeed(blendSpeed);
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

    [PunRPC]
    public void RPC_ToggleSkate(bool skate)
    {
        skateboard.SetActive(skate);
        playerController.rideSkate = skate;
    }

    public void StartSkate()
    {
        if (playerController.rideSkate) return;

        originalCenter = characterController.center;

        photonView.RPC("RPC_ToggleSkate", RpcTarget.AllBuffered, true);

        Vector3 newCenter = originalCenter;
        newCenter.y += rideYOffset;
        characterController.center = newCenter;

        Vector3 bodyPos = playerBody.localPosition;
        bodyPos.y += rideYOffset;
        playerBody.localPosition = bodyPos;

        playerController.rideSkate = true;
        currentState = PlayerState.Skate;
        playerAnimController.SetSkate(true);
    }

    public void OnFallDown()
    {
        playerController.isFallDown = true;
        if (playerController.rideSkate)
        {
            playerAnimController.SetSkate(false);

            if (playerController.photonView.IsMine)
            {
                playerController.photonView.RPC("RPC_DropRandomItem", RpcTarget.MasterClient);
            }

            playerController.rideSkate = false;
        }
        
        currentState = PlayerState.FallDown;
        playerAnimController.SetFallDown(true);
    }

    public void OnFallDownEnd()
    {
        if (skateboard.activeInHierarchy)
        {
            characterController.center = originalCenter;
            playerBody.localPosition = Vector3.zero;
        }

        photonView.RPC("RPC_ToggleSkate", RpcTarget.AllBuffered, false);
        playerController.isHit = false;

        playerAnimController.SetSkate(false);
        playerAnimController.SetFallDown(false);
        playerController.isFallDown = false;
        currentState = PlayerState.Idle;
        Debug.Log("2");
    }

    private void SetCameraMode(bool firstPerson)
    {
        if (!photonView.IsMine) return;

        if (playerController.fpsCam != null && playerController.tpsCam != null)
        {
            playerController.fpsCam.gameObject.SetActive(firstPerson);
            playerController.tpsCam.gameObject.SetActive(!firstPerson);
            activeCam = firstPerson ? playerController.fpsCam : playerController.tpsCam;
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
        {
            int index = Random.Range(0, FootstepAudioClips.Length);
            footstepSource.PlayOneShot(FootstepAudioClips[index]);
        }
    }
}
