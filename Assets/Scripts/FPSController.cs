using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class FPSController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 1.4f;
    public float gravity = -20f;

    [Header("Slide")]
    public float slideStartSpeed = 11f;
    public float slideEndSpeed = 4f;
    public float slideDuration = 0.75f;
    public float slideSteeringAmount = 0.15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.01f;
    public float maxLookAngle = 85f;

    [Header("Crouch")]
    public float standingHeight = 1.8f;
    public float crouchingHeight = 1.0f;
    public float standingCameraHeight = 1.6f;
    public float crouchingCameraHeight = 1.0f;
    public float crouchTransitionSpeed = 10f;

    [Header("Ladder Lift")]
    public float ladderLiftSpeed = 3f;
    private bool isInLadderLiftZone;

    private CharacterController controller;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Vector3 velocity;
    private float cameraPitch;

    private bool isSprinting;
    private bool sprintToggled;

    private bool isCrouching;
    private bool crouchHeld;

    private bool isSliding;
    private float slideTimer;
    private Vector3 slideDirection;

    public void SetInLadderLiftZone(bool value)
    {
        isInLadderLiftZone = value;

        if (isInLadderLiftZone)
        {
            StopSprint();
            StopSlide();
        }

        if (!isInLadderLiftZone && velocity.y > 0f)
        {
            velocity.y = 0f;
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        InputActionMap playerMap = playerInput.actions.FindActionMap("Player", true);

        moveAction = playerMap.FindAction("Move", true);
        lookAction = playerMap.FindAction("Look", true);
        jumpAction = playerMap.FindAction("Jump", true);
        sprintAction = playerMap.FindAction("Sprint", true);
        crouchAction = playerMap.FindAction("Crouch", true);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
        {
            Debug.LogError("Player Camera is not assigned.");
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        ReadInput();
        HandleLook();
        HandleMovement();
        HandleCrouch();
    }

    private void ReadInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        crouchHeld = crouchAction.ReadValue<float>() > 0.5f;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool movingForward = moveInput.y > 0.1f;
        bool primarilyForward = moveInput.y + 0.05f >= Mathf.Abs(moveInput.x);

        bool validSprintMovement =
            isMoving &&
            movingForward &&
            primarilyForward &&
            !isInLadderLiftZone;

        if (sprintAction.WasPressedThisFrame() && validSprintMovement && !isSliding)
        {
            sprintToggled = true;
        }

        if (!validSprintMovement || isSliding || isInLadderLiftZone)
        {
            sprintToggled = false;
        }

        if (crouchAction.WasPressedThisFrame() && sprintToggled && controller.isGrounded && !isSliding)
        {
            StartSlide();
        }

        isSprinting =
            sprintToggled &&
            validSprintMovement &&
            !isSliding &&
            !crouchHeld;

        isCrouching = crouchHeld || isSliding;
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isSliding)
        {
            HandleSlideMovement();
        }
        else
        {
            HandleNormalMovement();
        }

        HandleVerticalMovement(isGrounded);
    }

    private void HandleNormalMovement()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        move = Vector3.ClampMagnitude(move, 1f);

        float currentSpeed = walkSpeed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleSlideMovement()
    {
        slideTimer -= Time.deltaTime;

        float slideProgress = 1f - Mathf.Clamp01(slideTimer / slideDuration);
        float currentSlideSpeed = Mathf.Lerp(slideStartSpeed, slideEndSpeed, slideProgress);

        Vector3 steeringDirection =
            transform.right * moveInput.x +
            transform.forward * Mathf.Max(0f, moveInput.y);

        if (steeringDirection.sqrMagnitude > 0.01f)
        {
            slideDirection = Vector3.Lerp(
                slideDirection,
                steeringDirection.normalized,
                slideSteeringAmount * Time.deltaTime
            ).normalized;
        }

        controller.Move(slideDirection * currentSlideSpeed * Time.deltaTime);

        if (slideTimer <= 0f)
        {
            StopSlide();
        }
    }

    private void HandleVerticalMovement(bool isGrounded)
    {
        bool jumpHeld = jumpAction.ReadValue<float>() > 0.5f;

        if (isInLadderLiftZone && jumpHeld)
        {
            velocity.y = ladderLiftSpeed;
        }
        else
        {
            if (jumpAction.WasPressedThisFrame() && isGrounded && !isCrouching && !isInLadderLiftZone)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        float targetCameraHeight = isCrouching ? crouchingCameraHeight : standingCameraHeight;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );

        controller.center = new Vector3(0f, controller.height / 2f, 0f);

        Vector3 cameraPosition = playerCamera.localPosition;
        cameraPosition.y = Mathf.Lerp(
            cameraPosition.y,
            targetCameraHeight,
            Time.deltaTime * crouchTransitionSpeed
        );

        playerCamera.localPosition = cameraPosition;
    }

    private void StartSlide()
    {
        sprintToggled = false;
        isSprinting = false;

        isSliding = true;
        isCrouching = true;

        slideTimer = slideDuration;
        slideDirection = transform.forward;

        if (velocity.y > 0f)
        {
            velocity.y = 0f;
        }
    }

    private void StopSlide()
    {
        isSliding = false;
        isCrouching = crouchHeld;
    }

    private void StopSprint()
    {
        sprintToggled = false;
        isSprinting = false;
    }
}