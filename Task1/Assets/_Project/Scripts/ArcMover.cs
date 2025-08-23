using UnityEngine;

/// <summary>
/// Sun movement constrained to plane z = fixedWorldZ. 
/// Rotates around pivot along a fixed radius.
/// Inherits from DistanceGrabber for grab/drag handling.
/// </summary>
public class ArcMover : DistanceGrabber
{
    [Header("Objects to move")]
    public Transform Sun;
    public Light directionalLight;

    [Header("Movement Settings")]
    public float Sensitivity = 1.5f;
    public float minRadius = 0.5f;
    public float minAngleDeg = 10f;
    public float maxAngleDeg = 180f;

    Transform pivot;
    float fixedWorldZ;
    float radiusXY;
    float currentAngleDeg;

    void Start()
    {
        pivot = Camera.main ? Camera.main.transform : transform;

        if (!Sun)
        {
            enabled = false;
            return;
        }

        if (minAngleDeg > maxAngleDeg)
        {
            var tmp = minAngleDeg;
            minAngleDeg = maxAngleDeg;
            maxAngleDeg = tmp;
        }

        fixedWorldZ = Sun.position.z;

        var pivotProjected = new Vector3(pivot.position.x, pivot.position.y, fixedWorldZ);
        var offsetXY = new Vector3(Sun.position.x - pivotProjected.x, Sun.position.y - pivotProjected.y, 0f);
        radiusXY = Mathf.Max(minRadius, offsetXY.magnitude);

        currentAngleDeg = Mathf.Atan2(offsetXY.y, offsetXY.x) * Mathf.Rad2Deg;

        lastMousePosition = Input.mousePosition;
        UpdateDirectionalLight();
    }


    protected override void OnGrabUpdate(Transform obj)
    {
        var cam = Camera.main;
        if (cam == null) return;

        if (!TryGetPlanePoint(cam, Input.mousePosition, out var currPoint) ||
            !TryGetPlanePoint(cam, lastMousePosition, out var prevPoint))
            return;

        var pivotProjected = GetPivotProjected();

        var prevDir2D = new Vector2(prevPoint.x - pivotProjected.x, prevPoint.y - pivotProjected.y);
        var currDir2D = new Vector2(currPoint.x - pivotProjected.x, currPoint.y - pivotProjected.y);

        if (prevDir2D.sqrMagnitude < 1e-6f || currDir2D.sqrMagnitude < 1e-6f)
            return;

        var signedAngle = Vector2.SignedAngle(prevDir2D.normalized, currDir2D.normalized);
        ApplyRotationAndUpdate(signedAngle);
    }


    protected override bool TryGetClickedObject(out Transform clickedObject)
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

    bool TryGetPlanePoint(Camera cam, Vector3 screenPos, out Vector3 worldPoint)
    {
        worldPoint = default;
        var plane = new Plane(Vector3.forward, new Vector3(0f, 0f, fixedWorldZ));
        var ray = cam.ScreenPointToRay(screenPos);
        var hit = plane.Raycast(ray, out float enter);
        if (!hit) return false;
        worldPoint = ray.GetPoint(enter);
        return true;
    }

    Vector3 GetPivotProjected() => new Vector3(pivot.position.x, pivot.position.y, fixedWorldZ);

    void ApplyRotationAndUpdate(float signedAngle)
    {
        currentAngleDeg += signedAngle * Sensitivity;
        currentAngleDeg = Mathf.Clamp(currentAngleDeg, minAngleDeg, maxAngleDeg);

        var newPos = ComputeSunPosition(currentAngleDeg);
        Sun.position = newPos;
        UpdateDirectionalLight();
    }

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
    protected override void OnGrabEnd(Transform obj)
    {
        // Reset state if needed
    }
    protected override void OnGrabStart(Transform obj)
    {
        // Nothing special needed for Sun at grab start
    }
}
