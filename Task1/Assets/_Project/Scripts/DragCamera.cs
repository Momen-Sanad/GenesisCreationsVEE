using UnityEngine;

public class DragCamera : MonoBehaviour
{
    [SerializeField] float sensitivity = 2f;
    float pitch = 0f;
    Transform playerBody;

    void Start()
    {
        playerBody = transform.parent; // Player is parent
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to center
        Cursor.visible = false; // Hide cursor
    }

    void Update()
    {
        
        var mouseX = Input.GetAxis("Mouse X") * sensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Rotate player (yaw)
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate camera (pitch)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
