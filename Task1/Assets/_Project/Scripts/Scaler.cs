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



    void Update() => HandleObjectScaling();

    /// <summary>
    /// Handles the logic for selecting and scaling celestial bodies
    /// based on mouse input.
    /// </summary>
    private void HandleObjectScaling() {

        // Mouse pressed down — check if clicked on a valid object
        if (Input.GetMouseButtonDown(0)) {
            if (TryGetClickedObject(out selectedObject)) {

                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        // While holding the mouse button, apply scaling
        if (Input.GetMouseButton(0) && isDragging && selectedObject != null)
            ApplyScalingFromMouseMovement();
        

        // Mouse released —> stop scaling
        if (Input.GetMouseButtonUp(0)) {

            isDragging = false;
            selectedObject = null;
        }
    }

    /// <summary>
    /// Applies scaling to the currently selected object
    /// based on the vertical movement of the mouse.
    /// </summary>
    private void ApplyScalingFromMouseMovement() {

        Vector3 delta = Input.mousePosition - lastMousePosition;

        // Calculate scaling factor with sensitivity adjustment
        float scaleFactor = 1 + delta.y * 0.005f;
        scaleFactor = Mathf.Clamp(scaleFactor, 0.5f, 2.0f);

        // Apply scaling
        ScaleCelestialBody(selectedObject, scaleFactor);

        // Update for the next frame
        lastMousePosition = Input.mousePosition;
    }


    /// <summary>
    /// Casts a ray from the camera to check if the user clicked one of the celestial bodies.
    /// </summary>
    bool TryGetClickedObject(out Transform clickedObject) {
        clickedObject = null;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit)) {

            var tag = hit.transform.tag;

            if (tag == "Sun" || tag == "Earth" || tag == "Moon") {

                clickedObject = hit.transform;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Scales the selected object by multiplying its local scale,
    /// and clamps the Y scale to stay within defined limits.
    /// </summary>
    private void ScaleCelestialBody(Transform clicked, float scaleFactor)
    {
        var scale = clicked.localScale * scaleFactor;

        // Clamping
        scale.y = Mathf.Clamp(scale.y, 0.1f, Mathf.Max(scale.x, scale.z));

        clicked.localScale = scale;
    }
}
