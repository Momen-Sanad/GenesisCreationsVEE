using UnityEngine;

public class Scaler : MonoBehaviour
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
            
            float scaleFactor = 1 + delta.y * 0.005f; // Sensitivity adjustment
            scaleFactor = Mathf.Clamp(scaleFactor, 0.5f, 2.0f);

            // Call the function to scale using the calculated factor
            ScaleCelestialBody(selectedObject, scaleFactor);

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
    /// Scales the selected object.
    /// </summary>
    private void ScaleCelestialBody(Transform clicked, float scaleFactor) => clicked.localScale *= scaleFactor;
    
}
