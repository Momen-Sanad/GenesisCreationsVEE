using UnityEngine;

public class YScaler : MonoBehaviour
{
    [Header("Objects to scale")]
    public Transform Sun;
    public Transform Earth;
    public Transform Moon;

    private Transform selectedObject = null;

    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Update()
    {
        // Check if user is pressing down
        if (Input.GetMouseButtonDown(0))
        {
            // Check if user is pressing on a viable object (sun/moon/earth)
            if (TryGetClickedObject(out selectedObject))
            {
                /// This will allow calling of ScaleCelestialBody(Transform, float)
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(0) && isDragging && selectedObject != null)
        {
            // Equation to calculate how much to scale based on the click position and current position
            Vector3 delta = Input.mousePosition - lastMousePosition;

            float yDelta = delta.y * 0.01f; // Sensitivity adjustment

            // Call the function to scale using the calculated delta
            ScaleCelestialBodyY(selectedObject, yDelta);

            lastMousePosition = Input.mousePosition;
        }

        // User stopped holding left click
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectedObject = null;
        }
    }

    /// <summary>
    /// Casts a ray from the camera to check if the user clicked one of the celestial bodies.
    /// </summary>
    /// <param name="clickedObject">The object clicked, if any.</param>
    /// <returns>True if a valid celestial body was clicked.</returns>
    private bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Transform obj = hit.transform;
            string tag = obj.tag;

            if (tag == "Sun" || tag == "Earth" || tag == "Moon")
            {
                clickedObject = obj;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Scales the selected object's Y axis only.
    /// </summary>
    private void ScaleCelestialBodyY(Transform clicked, float yDelta)
    {
        Vector3 scale = clicked.localScale;

        // Clamp Y so it doesn't exceed X/Z and avoids flattening to 0
        scale.y = Mathf.Clamp(scale.y + yDelta, 0.1f, Mathf.Max(scale.x, scale.z));

        clicked.localScale = scale;
    }
}
