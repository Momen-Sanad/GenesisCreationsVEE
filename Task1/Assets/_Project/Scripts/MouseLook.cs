using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float sensitivity = 2f;   // Mouse sensitivity
    public float yClamp = 80f;       // Vertical clamp (prevent flipping)

    float xRotation = 0f; // Pitch
    float yRotation = 0f; // Yaw
    bool isLooking = false;

    void Start()
    {
        // Initialize rotation
        var euler = transform.localRotation.eulerAngles;
        yRotation = euler.y;
        xRotation = euler.x;
    }

    void Update()
    {
        // Start looking when right mouse button is held
        if (Input.GetMouseButtonDown(1))
            isLooking = true;
        
        if (Input.GetMouseButtonUp(1))
            isLooking = false;


        if (isLooking)
        {
            var mouseX = Input.GetAxis("Mouse X") * sensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            // Adjust yaw and pitch
            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -yClamp, yClamp);

            // Apply rotation
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}
