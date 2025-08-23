using UnityEngine;

/// <summary>
/// Generic base class for distance grabbing using mouse and raycasting.
/// Provides overridable hooks for grab start, update, and end.
/// </summary>
public abstract class DistanceGrabber : MonoBehaviour
{
    protected Transform selectedObject;
    protected bool isDragging;
    protected Vector3 lastMousePosition;

    void Update()
    {
        HandleMouseDown();
        if (isDragging) HandleDragging();
        HandleMouseUp();
    }

    void HandleMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (TryGetClickedObject(out selectedObject))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
            OnGrabStart(selectedObject);
        }
    }

    void HandleDragging()
    {
        if (!Input.GetMouseButton(0) || selectedObject == null) return;

        OnGrabUpdate(selectedObject);
        lastMousePosition = Input.mousePosition;
    }

    void HandleMouseUp()
    {
        if (!Input.GetMouseButtonUp(0)) return;

        if (isDragging)
        {
            OnGrabEnd(selectedObject);
            isDragging = false;
            selectedObject = null;
        }
    }

    /// <summary>
    /// Override this for object-specific grab start behavior.
    /// </summary>
    protected abstract void OnGrabStart(Transform obj);

    /// <summary>
    /// Override this for object-specific grab update behavior.
    /// </summary>
    protected abstract void OnGrabUpdate(Transform obj);

    /// <summary>
    /// Override this for object-specific grab end behavior.
    /// </summary>
    protected abstract void OnGrabEnd(Transform obj);

    /// <summary>
    /// Override this to define which objects can be grabbed.
    /// </summary>
    protected abstract bool TryGetClickedObject(out Transform clickedObject);
}
