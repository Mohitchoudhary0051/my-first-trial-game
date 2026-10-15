using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3D Third-Person Orbiting Camera Controller (using Unity's New Input System)
/// 
/// SETUP INSTRUCTIONS:
/// 1. Attach this script to your Main Camera.
/// 2. Drag your Player GameObject (or a child pivot object at head height) into the 'Target' field.
/// 3. In the Inspector, assign the 'Look Action' (e.g. Player/Look from InputSystem_Actions asset).
///    - If left unassigned, automatic Mouse / Gamepad fallback input will be used.
/// 4. Adjust Distance, Sensitivity, and Pitch limits as desired.
/// 5. Hit Play! Click in game window to lock cursor, press ESC to unlock cursor.
/// </summary>
public class CameraController : MonoBehaviour
{
    // ==================== TARGET SETTINGS ====================

    [Header("Target")]
    [Tooltip("The Transform the camera will follow (e.g., Player capsule or head pivot)")]
    [SerializeField] private Transform target;

    [Tooltip("Offset applied to target position (e.g. (0, 1.5, 0) to look at head height)")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    // ==================== INPUT SETTINGS ====================

    [Header("Input (New Input System)")]
    [Tooltip("Reference to the Look Action (e.g., Player/Look). If unassigned, mouse/gamepad fallback is used.")]
    [SerializeField] private InputActionReference lookAction;

    [Tooltip("Reference to Scroll Wheel or Zoom Action. Optional.")]
    [SerializeField] private InputActionReference zoomAction;

    [Tooltip("Mouse look sensitivity multiplier")]
    [SerializeField] private float mouseSensitivity = 1.5f;

    [Tooltip("Should the cursor be locked and hidden when playing?")]
    [SerializeField] private bool lockCursor = true;

    // ==================== DISTANCE & ZOOM SETTINGS ====================

    [Header("Distance & Zoom")]
    [Tooltip("Default camera distance from target")]
    [SerializeField] private float distance = 5f;

    [Tooltip("Minimum zoom distance")]
    [SerializeField] private float minDistance = 2f;

    [Tooltip("Maximum zoom distance")]
    [SerializeField] private float maxDistance = 10f;

    [Tooltip("Speed of camera zoom")]
    [SerializeField] private float zoomSpeed = 2f;

    // ==================== PITCH & YAW LIMITS ====================

    [Header("Rotation Limits")]
    [Tooltip("Minimum vertical angle (looking up, e.g. -30 degrees)")]
    [SerializeField] private float minPitch = -30f;

    [Tooltip("Maximum vertical angle (looking down, e.g. 70 degrees)")]
    [SerializeField] private float maxPitch = 70f;

    [Tooltip("Smoothing time for camera movement (0 = instant)")]
    [SerializeField] private float smoothTime = 0.05f;

    // ==================== COLLISION SETTINGS ====================

    [Header("Camera Collision")]
    [Tooltip("Enable raycasting to prevent camera from clipping into terrain and walls")]
    [SerializeField] private bool enableObstacleAvoidance = true;

    [Tooltip("Layer mask for geometry that blocks camera view")]
    [SerializeField] private LayerMask collisionLayers = ~0;

    [Tooltip("SphereCast radius for obstacle collision check")]
    [SerializeField] private float collisionRadius = 0.2f;

    // ==================== PRIVATE VARIABLES ====================

    private float currentYaw;
    private float currentPitch;

    private Vector3 currentVelocity;
    private Vector3 currentPosition;

    private Vector2 lookInput;
    private float zoomInput;

    private void OnEnable()
    {
        if (lookAction != null && lookAction.action != null)
        {
            lookAction.action.Enable();
        }

        if (zoomAction != null && zoomAction.action != null)
        {
            zoomAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (lookAction != null && lookAction.action != null)
        {
            lookAction.action.Disable();
        }

        if (zoomAction != null && zoomAction.action != null)
        {
            zoomAction.action.Disable();
        }
    }

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Initialize starting angles from current camera rotation
        Vector3 angles = transform.eulerAngles;
        currentYaw = angles.y;
        currentPitch = angles.x;

        if (target == null)
        {
            // Try finding GameObject with "Player" tag
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        ReadInput();
        HandleCursorLockToggle();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Apply Look Input (Yaw & Pitch)
        currentYaw += lookInput.x * mouseSensitivity;
        currentPitch -= lookInput.y * mouseSensitivity;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Handle Zoom Input
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            distance = Mathf.Clamp(distance - (zoomInput * zoomSpeed), minDistance, maxDistance);
        }

        // Calculate Target Focus Position
        Vector3 targetFocusPosition = target.position + targetOffset;

        // Calculate Rotation & Desired Offset Vector
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = targetFocusPosition - (rotation * Vector3.forward * distance);

        // Obstacle Avoidance (SphereCast check)
        float currentDistance = distance;
        if (enableObstacleAvoidance)
        {
            Vector3 rayDirection = desiredPosition - targetFocusPosition;
            if (Physics.SphereCast(targetFocusPosition, collisionRadius, rayDirection.normalized, out RaycastHit hit, distance, collisionLayers))
            {
                currentDistance = Mathf.Clamp(hit.distance, minDistance, distance);
                desiredPosition = targetFocusPosition - (rotation * Vector3.forward * currentDistance);
            }
        }

        // Smooth position transition
        currentPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // Apply Position & Rotation to Camera
        transform.position = currentPosition;
        transform.LookAt(targetFocusPosition);
    }

    private void ReadInput()
    {
        lookInput = Vector2.zero;
        zoomInput = 0f;

        // 1. Read Look Input
        if (lookAction != null && lookAction.action != null)
        {
            lookInput = lookAction.action.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue() * 0.1f;
        }

        // 2. Read Zoom Input
        if (zoomAction != null && zoomAction.action != null)
        {
            zoomInput = zoomAction.action.ReadValue<float>();
        }
        else if (Mouse.current != null)
        {
            zoomInput = Mouse.current.scroll.ReadValue().y * 0.01f;
        }
    }

    private void HandleCursorLockToggle()
    {
        // Press ESC to unlock cursor, click game view to lock back
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && lockCursor)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
