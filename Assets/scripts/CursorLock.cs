using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Locks and hides the mouse cursor for third-person cameras (Cinemachine / Orbit Camera).
/// 
/// SETUP INSTRUCTIONS:
/// 1. Attach this script to your Main Camera or a Manager object in the scene.
/// 2. Hit Play! The mouse cursor will lock to the screen center for camera rotation.
/// 3. Press Escape to unlock the cursor, click anywhere in the Game View to lock it back.
/// </summary>
public class CursorLock : MonoBehaviour
{
    [Tooltip("Automatically lock cursor on Start")]
    [SerializeField] private bool lockOnStart = true;

    private void Start()
    {
        if (lockOnStart)
        {
            SetCursorState(true);
        }
    }

    private void Update()
    {
        // Unlock cursor with Escape key
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorState(false);
        }

        // Lock cursor when clicking back into the game window
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorState(true);
            }
        }
    }

    public void SetCursorState(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
