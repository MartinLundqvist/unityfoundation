using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Movement speed in units per second.
    public float movementSpeed = 10f;
    // How fast the camera rotates in degrees per second per unit of mouse movement.
    public float lookSpeed = 2f;
    // Limits for vertical rotation (in degrees).
    public float verticalLookLimit = 80f;

    // Internal variables to track rotation.
    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        // Only handle rotation if the right mouse button is held down.
        if (Input.GetMouseButton(1))
        {
            // Lock and hide the cursor.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            HandleRotation();
        }
        else
        {
            // When RMB is not pressed, unlock and show the cursor.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Always handle movement.
        HandleMovement();

        // Optional: allow quitting the application or unlocking the cursor with Escape.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleRotation()
    {
        // Get mouse input.
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Adjust yaw and pitch.
        yaw += mouseX * lookSpeed;
        pitch -= mouseY * lookSpeed;
        // Clamp pitch so the camera can't look too far up or down.
        pitch = Mathf.Clamp(pitch, -verticalLookLimit, verticalLookLimit);

        // Apply the rotation.
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        // Get input from WASD or arrow keys.
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Get vertical movement from Q/E keys
        float moveY = 0f;
        if (Input.GetKey(KeyCode.E)) moveY += 1f;
        if (Input.GetKey(KeyCode.Q)) moveY -= 1f;

        // Calculate movement relative to the camera's orientation.
        Vector3 move = transform.right * moveX + transform.forward * moveZ + Vector3.up * moveY;

        // Move the camera (frame-rate independent).
        transform.position += move * movementSpeed * Time.deltaTime;
    }
}
