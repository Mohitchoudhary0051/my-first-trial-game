using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3D Third-Person Player Controller (using Unity's New Input System)
/// 
/// SETUP INSTRUCTIONS:
/// 1. Create a Capsule (GameObject > 3D Object > Capsule) — this is your player.
/// 2. Add a CharacterController component to the Capsule (Add Component > CharacterController).
/// 3. Attach this script to the Capsule.
/// 4. In the Inspector, assign Move Action and Jump Action (e.g., from InputSystem_Actions asset: Player/Move, Player/Jump).
///    - If left unassigned, automatic keyboard fallback (WASD / Arrow keys, Space) will be used.
///    - Alternatively, add a PlayerInput component set to "Send Messages".
/// 5. Position the Main Camera behind and above the player (e.g., offset (0, 3, -6)).
/// 6. Hit Play and use WASD / Left Stick to move, Space / South Button to jump.
/// 
/// HOW IT WORKS:
/// - Uses Unity's New Input System (UnityEngine.InputSystem).
/// - We use Unity's CharacterController (NOT a Rigidbody) for movement.
/// - The player moves relative to the CAMERA direction.
/// - The player smoothly rotates to face the movement direction.
/// - Gravity and jumping are calculated manually each frame.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ==================== INPUT SETTINGS ====================

    [Header("Input (New Input System)")]
    [Tooltip("Reference to the Move Action (e.g., Player/Move). If unassigned, keyboard fallback is used.")]
    [SerializeField] private InputActionReference moveAction;

    [Tooltip("Reference to the Jump Action (e.g., Player/Jump). If unassigned, keyboard fallback is used.")]
    [SerializeField] private InputActionReference jumpAction;

    // ==================== MOVEMENT SETTINGS ====================

    [Header("Movement")]
    [Tooltip("How fast the player moves (units per second)")]
    [SerializeField] private float moveSpeed = 6f;

    [Tooltip("How quickly the player rotates to face movement direction (higher = snappier)")]
    [SerializeField] private float rotationSpeed = 10f;

    // ==================== JUMP & GRAVITY SETTINGS ====================

    [Header("Jump & Gravity")]
    [Tooltip("How high the player jumps")]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("Gravity strength pulling the player down (positive value, applied as negative Y)")]
    [SerializeField] private float gravity = 20f;

    // ==================== CAMERA REFERENCE ====================

    [Header("Camera")]
    [Tooltip("Drag your Main Camera here. If left empty, the script finds it automatically.")]
    [SerializeField] private Transform cameraTransform;

    // ==================== PRIVATE VARIABLES ====================

    // Reference to the CharacterController component on this GameObject
    private CharacterController controller;

    // Tracks the player's vertical velocity (used for gravity and jumping)
    private Vector3 velocity;

    // Optional cache for PlayerInput component callbacks
    private Vector2 playerInputVector;
    private bool jumpTriggeredFromCallback;

    private void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Enable();
        }

        if (jumpAction != null && jumpAction.action != null)
        {
            jumpAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Disable();
        }

        if (jumpAction != null && jumpAction.action != null)
        {
            jumpAction.action.Disable();
        }
    }

    /// <summary>
    /// Start() runs once when the game begins.
    /// We grab references to components we'll need every frame.
    /// </summary>
    void Start()
    {
        // Get the CharacterController attached to this same GameObject
        controller = GetComponent<CharacterController>();

        // If no camera was assigned in the Inspector, find the main camera automatically
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    /// <summary>
    /// Callback for PlayerInput component using "Send Messages" / "Broadcast Messages".
    /// </summary>
    public void OnMove(InputValue value)
    {
        playerInputVector = value.Get<Vector2>();
    }

    /// <summary>
    /// Callback for PlayerInput component using "Send Messages" / "Broadcast Messages".
    /// </summary>
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpTriggeredFromCallback = true;
        }
    }

    /// <summary>
    /// Update() runs every frame. All player input and movement logic goes here.
    /// </summary>
    void Update()
    {
        // ---------- STEP 1: READ INPUT (New Input System) ----------
        Vector2 inputVector = Vector2.zero;

        if (moveAction != null && moveAction.action != null)
        {
            inputVector = moveAction.action.ReadValue<Vector2>();
        }
        else if (playerInputVector != Vector2.zero)
        {
            inputVector = playerInputVector;
        }
        else if (Keyboard.current != null)
        {
            // Fallback keyboard controls if InputActionReference is not set in Inspector
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputVector.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputVector.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputVector.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputVector.x += 1f;
        }

        float horizontalInput = inputVector.x;
        float verticalInput = inputVector.y;

        // ---------- STEP 2: CALCULATE CAMERA-RELATIVE DIRECTION ----------
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 cameraRight = cameraTransform != null ? cameraTransform.right : Vector3.right;

        cameraForward.y = 0f; // Flatten to horizontal plane
        cameraRight.y = 0f;   // Flatten to horizontal plane

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * verticalInput) + (cameraRight * horizontalInput);

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // ---------- STEP 3: ROTATE PLAYER TO FACE MOVEMENT DIRECTION ----------
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // ---------- STEP 4: APPLY GRAVITY & STEP 5: HANDLE JUMPING ----------
        if (controller.isGrounded)
        {
            velocity.y = -2f; // Small downward force to stay grounded

            bool jumpPressed = false;

            if (jumpAction != null && jumpAction.action != null)
            {
                jumpPressed = jumpAction.action.WasPressedThisFrame();
            }
            else if (jumpTriggeredFromCallback)
            {
                jumpPressed = true;
                jumpTriggeredFromCallback = false; // Reset after consuming
            }
            else if (Keyboard.current != null)
            {
                jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            }

            if (jumpPressed)
            {
                velocity.y = jumpForce;

                // Play jump sound through SoundManager
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayJumpSound();
                }
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        // ---------- STEP 6: MOVE THE PLAYER ----------
        Vector3 finalMovement = (moveDirection * moveSpeed * Time.deltaTime)
                              + (velocity * Time.deltaTime);

        controller.Move(finalMovement);
    }
}

