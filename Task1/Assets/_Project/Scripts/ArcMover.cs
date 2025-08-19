using UnityEngine;

public class ArcMover : MonoBehaviour
{
    [Header("Objects to move")]
    public Transform Sun;
    public Light directionalLight;

    Transform pivot;
    Transform selectedObject;

    [Header("Movement Settings")]
    public float rotationSpeed = 0.2f;
    public float arcRadius = 10f;
    public float minElevation = -10f;
    public float maxElevation = 180f;

    [Header("Day/Night Light Settings")]
    public float dayIntensity = 1.0f;
    public float nightIntensity = 0.05f;
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.1f, 0.15f, 0.4f);

    float horizontalDeg;
    float verticalDeg;
    float radius;

    Vector3 lastMousePosition;
    bool isDragging;

    void Start()
    {
        InitPivot();
        InitSunPosition();
        lastMousePosition = Input.mousePosition;
    }


    void Update()
    {
        HandleMouseDown();
        HandleDragging();
        HandleMouseUp();
    }

    /// <summary>
    /// Initializes pivot (camera or fallback to self).
    /// </summary>
    void InitPivot() =>
        pivot = Camera.main ? Camera.main.transform : transform;

    /// <summary>
    /// Initializes spherical coordinates of the sun relative to pivot.
    /// </summary>
    void InitSunPosition()
    {
        var offset = Sun.position - pivot.position;
        radius = offset.magnitude;

        if (radius <= 0.0001f)
            radius = arcRadius;
        else
            arcRadius = radius;

        horizontalDeg = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        verticalDeg = Mathf.Asin(Mathf.Clamp(offset.y / radius, -1f, 1f)) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Handles mouse down: tries to select the sun.
    /// </summary>
    void HandleMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (TryGetClickedObject(out selectedObject))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// Handles mouse dragging while sun is selected.
    /// </summary>
    void HandleDragging()
    {
        if (!Input.GetMouseButton(0) || !isDragging || selectedObject == null) return;

        UpdateAngles();
        UpdateSunPosition();
        UpdateDirectionalLight();

        lastMousePosition = Input.mousePosition;
    }

    /// <summary>
    /// Handles mouse up: releases the drag.
    /// </summary>
    void HandleMouseUp()
    {
        if (!Input.GetMouseButtonUp(0)) return;
        isDragging = false;
        selectedObject = null;
    }

    /// <summary>
    /// Updates horizontal and vertical spherical angles based on mouse movement.
    /// </summary>
    void UpdateAngles()
    {
        var delta = Input.mousePosition - lastMousePosition;
        horizontalDeg += delta.x * rotationSpeed;
        verticalDeg -= delta.y * rotationSpeed;
        verticalDeg = Mathf.Clamp(verticalDeg, minElevation, maxElevation);
    }

    /// <summary>
    /// Updates the sun position using spherical coordinates.
    /// </summary>
    void UpdateSunPosition()
    {
        var elevRad = verticalDeg * Mathf.Deg2Rad;
        var azimRad = horizontalDeg * Mathf.Deg2Rad;
        var horizontal = radius * Mathf.Cos(elevRad);

        var newOffset = new Vector3(
            horizontal * Mathf.Sin(azimRad),
            radius * Mathf.Sin(elevRad),
            horizontal * Mathf.Cos(azimRad)
        );

        Sun.position = pivot.position + newOffset;
    }

    /// <summary>
    /// Updates the directional light rotation, intensity, and color based on sun elevation.
    /// </summary>
    void UpdateDirectionalLight()
    {
        if (!directionalLight) return;

        var lightDir = (pivot.position - Sun.position).normalized;
        directionalLight.transform.rotation = Quaternion.LookRotation(lightDir);

        var t = Mathf.Clamp01(Vector3.Dot((Sun.position - pivot.position).normalized, Vector3.up));
        directionalLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
        directionalLight.color = Color.Lerp(nightColor, dayColor, t);
    }

    /// <summary>
    /// Tries to detect if the clicked object is the sun.
    /// </summary>
    bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag("Sun"))
        {
            clickedObject = hit.transform;
            return true;
        }
        return false;
    }
}
