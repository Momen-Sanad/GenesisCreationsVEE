using UnityEngine;

/// <summary>
/// Sun movement constrained to plane z = fixedWorldZ. Rotates around pivot along a fixed radius.
/// Dragging rotates the Sun around the circumference (no in/out movement).
/// Sensitivity is controlled via the inspector.
/// </summary>
public class ArcMover : MonoBehaviour
{
    [Header("Objects to move")]
    public Transform Sun;
    public Light directionalLight;

    [Header("Movement Settings")]
    public float Sensitivity = 1.5f;
    public float minRadius = 0.5f; // safety minimum radius

    Transform pivot;
    Transform selectedObject;
    bool isDragging;
    Vector3 lastMousePosition;

    float fixedWorldZ;
    float radiusXY;
    float currentAngleDeg;

    /// <summary>
    /// Initialize pivot, lock world Z and compute initial radius and angle in XY plane.
    /// </summary>
    void Start()
    {
        pivot = Camera.main ? Camera.main.transform : transform;

        if (!Sun)
        {
            enabled = false;
            return;
        }

        // Lock Z at Sun's initial world Z
        fixedWorldZ = Sun.position.z;

        // Compute initial radius/angle in XY plane around pivot projected onto z = fixedWorldZ
        // Project the pivot to the same Z as the Sun, compute XY offset and set radius/angle.
        var pivotProjected = new Vector3(pivot.position.x, pivot.position.y, fixedWorldZ);
        var offsetXY = new Vector3(Sun.position.x - pivotProjected.x, Sun.position.y - pivotProjected.y, 0f);
        radiusXY = Mathf.Max(minRadius, offsetXY.magnitude);

        currentAngleDeg = Mathf.Atan2(offsetXY.y, offsetXY.x) * Mathf.Rad2Deg;

        lastMousePosition = Input.mousePosition;
        UpdateDirectionalLight();
    }

    /// <summary>
    /// handle mouse press, dragging, and release.
    /// </summary>
    void Update()
    {
        HandleMouseDown();
        HandleDragging();
        HandleMouseUp();
    }

    /// <summary>
    /// Detect mouse button down and pick the Sun if clicked.
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
    /// When dragging, orchestrates the raycasts and rotation update by calling small helpers.
    /// </summary>
    void HandleDragging()
    {
        if (!ShouldProcessDrag()) return;

        var cam = Camera.main;
        if (cam == null) return;

        // Bulky behaviour moved to helpers:
        // Raycast current & previous mouse rays to the plane
        // Validate hit points and direction vectors
        // Compute signed angle delta and apply rotation (keeping radius fixed)
        if (!TryGetPlanePoint(cam, Input.mousePosition, out var currPoint) ||
            !TryGetPlanePoint(cam, lastMousePosition, out var prevPoint))
        {
            // If either ray missed the plane -> update lastMousePosition and bail out.
            lastMousePosition = Input.mousePosition;
            return;
        }

        var pivotProjected = GetPivotProjected();

        var prevDir2D = new Vector2(prevPoint.x - pivotProjected.x, prevPoint.y - pivotProjected.y);
        var currDir2D = new Vector2(currPoint.x - pivotProjected.x, currPoint.y - pivotProjected.y);

        if (!AreDirectionsValid(prevDir2D, currDir2D))
        {
            lastMousePosition = Input.mousePosition;
            return;
        }

        // compute signed angle and apply rotation + updates
        var signedAngle = ComputeSignedAngle(prevDir2D, currDir2D);
        ApplyRotationAndUpdate(signedAngle);
    }

    /// <summary>
    /// Check whether we should process dragging
    /// </summary>
    bool ShouldProcessDrag() => Input.GetMouseButton(0) && isDragging && selectedObject != null;

    /// <summary>
    /// Raycast the camera's screen point to the plane z = fixedWorldZ. Returns world point on the plane.
    /// </summary>
    bool TryGetPlanePoint(Camera cam, Vector3 screenPos, out Vector3 worldPoint)
    {
        worldPoint = default;
        var plane = new Plane(Vector3.forward, new Vector3(0f, 0f, fixedWorldZ));
        var ray = cam.ScreenPointToRay(screenPos);
        var hit = plane.Raycast(ray, out float enter);
        if (!hit)
        {
            return false;
        }
        worldPoint = ray.GetPoint(enter);
        return true;
    }

    /// <summary>
    /// Project the pivot transform onto the plane z = fixedWorldZ.
    /// </summary>
    Vector3 GetPivotProjected() => new Vector3(pivot.position.x, pivot.position.y, fixedWorldZ);

    /// <summary>
    /// Quick validity check that both 2D direction vectors are non-zero (within tolerance).
    /// </summary>
    bool AreDirectionsValid(Vector2 a, Vector2 b) => a.sqrMagnitude >= 1e-6f && b.sqrMagnitude >= 1e-6f;

    /// <summary>
    /// Compute the signed angle (degrees) between the two 2D directions around Z (XY-plane).
    /// </summary>
    float ComputeSignedAngle(Vector2 prevDir2D, Vector2 currDir2D) => Vector2.SignedAngle(prevDir2D.normalized, currDir2D.normalized);

    /// <summary>
    /// Apply the signed angle with sensitivity to the current angle, compute the new Sun position
    /// on the fixed radius and update Sun, light and lastMousePosition.
    /// </summary>
    void ApplyRotationAndUpdate(float signedAngle)
    {
        // apply sensitivity
        currentAngleDeg += signedAngle * Sensitivity;

        // Keep radius fixed
        var newPos = ComputeSunPosition(currentAngleDeg);
        Sun.position = newPos;

        UpdateDirectionalLight();

        // update last mouse pos for next frame
        lastMousePosition = Input.mousePosition;
    }

    /// <summary>
    /// Compute the Sun world position on the circle of radiusXY around the pivot for the given angle in degrees.
    /// </summary>
    Vector3 ComputeSunPosition(float angleDeg)
    {
        var angRad = angleDeg * Mathf.Deg2Rad;
        var p = GetPivotProjected();
        return new Vector3(
            p.x + radiusXY * Mathf.Cos(angRad),
            p.y + radiusXY * Mathf.Sin(angRad),
            fixedWorldZ
        );
    }


    /// <summary>
    /// Stop dragging and release selection.
    /// </summary>
    void HandleMouseUp()
    {
        if (!Input.GetMouseButtonUp(0)) return;
        isDragging = false;
        selectedObject = null;
    }

    /// <summary>
    /// Update the directional light's rotation, intensity and color based on Sun position.
    /// </summary>
    void UpdateDirectionalLight()
    {
        if (!directionalLight) return;

        var lightDir = (pivot.position - Sun.position).normalized;
        directionalLight.transform.rotation = Quaternion.LookRotation(lightDir);
        var nightColor = new Color(0.1f, 0.15f, 0.4f);

        var t = Mathf.Clamp01(Vector3.Dot((Sun.position - pivot.position).normalized, Vector3.up));
        directionalLight.intensity = Mathf.Lerp(0.05f, 1f, t);
        directionalLight.color = Color.Lerp(nightColor, Color.white, t);
    }

    /// <summary>
    /// Raycast from the current mouse position and return true if the Sun (or object tagged "Sun") was hit.
    /// </summary>
    bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;
        var cam = Camera.main;
        if (cam == null) return false;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.transform == Sun || hit.transform.CompareTag("Sun"))
            {
                clickedObject = hit.transform;
                return true;
            }
        }
        return false;
    }
}
