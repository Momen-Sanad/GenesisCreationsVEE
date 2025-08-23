using UnityEngine;

/// <summary>
/// Concrete implementation of DistanceGrabber for the Earth.
/// Allows grabbing with the mouse, and rotates Earth around its axis
/// to simulate a day/night cycle.
/// </summary>
public class EarthRotator : DistanceGrabber
{
    [Header("Earth Rotation Settings")]
    public float rotationSensitivity = 0.2f; 


    protected override void OnGrabStart(Transform obj)
    {
        Debug.Log("Started grabbing Earth");
    }

    protected override void OnGrabUpdate(Transform obj)
    {
        if (obj.CompareTag("Earth"))
        {
            // Mouse delta since last frame
            var mouseDelta = Input.mousePosition - lastMousePosition;

            // Rotate Earth around its Y-axis by horizontal mouse movement
            obj.Rotate(Vector3.up, -mouseDelta.x * rotationSensitivity, Space.Self);

        }
    }

    protected override void OnGrabEnd(Transform obj)
    {
        Debug.Log("Released Earth");
    }

    protected override bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;

        // Raycast from mouse position into the scene
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("Earth"))
            {
                clickedObject = hit.transform;
                return true;
            }
        }
        return false;
    }
}
