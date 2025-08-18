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
        // If camera is not parented during a transition, do nothing.
        if (transform.parent == null) return;

        var mouseX = Input.GetAxis("Mouse X") * sensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        if (playerBody == null) playerBody = transform.parent; // re-cache if needed

        playerBody.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

}
