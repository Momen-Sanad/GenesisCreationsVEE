using UnityEngine;

/// <summary>
/// Handles tilting the Earth by mouse movement while distance-grabbing.
/// User must tilt to ~20 (within margin). Holding that tilt for a set
/// duration triggers the SeasonsManager phase change.
/// </summary>
public class TiltEarth : DistanceGrabber
{
    [Header("References")]
    public GameObject Earth;
    public SeasonsManager Manager;

    [Header("Tilt settings")]
    public float dragSensitivity = 0.2f;     // degrees per pixel of horizontal movement
    public float minAnglePerFrame = 0.001f;  // ignore jitter
    public float targetTilt = 20f;           // required tilt angle
    public float tiltMargin = 2f;            // margin around target
    public Vector3 tiltAxis = Vector3.right;

    [Header("Wait settings")]
    public float holdTime = 3f;              // seconds to hold in target range

    [SerializeField]
    TiltIndicator indicator;

    // internal state
    Quaternion initialRotation;
    float accumulatedAngle = 0f;

    float holdTimer = 0f;
    bool isInRange = false;

    /// <summary>
    /// Validate references and set fallback camera if none assigned.
    /// </summary>
    void Start()
    {
        if (Earth == null)
            return;
    }

    // DistanceGrabber overrides

    /// <summary>
    /// Detects if the clicked object (via raycast) is the Earth.
    /// </summary>
    protected override bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;
        if (Earth == null) return false;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Transform t = hit.transform;
            while (t != null)
            {
                if (t == Earth.transform)
                {
                    clickedObject = Earth.transform;
                    return true;
                }
                t = t.parent;
            }
        }
        return false;
    }

    /// <summary>
    /// Resets rotation tracking and timers at the start of a grab.
    /// </summary>
    protected override void OnGrabStart(Transform obj)
    {
        accumulatedAngle = 0f;
        initialRotation = Earth != null ? Earth.transform.rotation : Quaternion.identity;
        holdTimer = 0f;
        isInRange = false;
    }

    /// <summary>
    /// Applies tilt incrementally based on mouse movement, checks if tilt 
    /// reaches target ~20, and manages the hold timer.
    /// </summary>
    protected override void OnGrabUpdate(Transform obj)
    {
        if (obj == null || Earth == null)
            return;

        var delta = Input.mousePosition - lastMousePosition;
        var angleDelta = delta.x * dragSensitivity;

        // Apply tilt if movement is above noise threshold
        if (Mathf.Abs(angleDelta) >= minAnglePerFrame)
        {
            accumulatedAngle += angleDelta;
            var axisWorld = Earth.transform.TransformDirection(tiltAxis.normalized);
            Earth.transform.rotation = Quaternion.AngleAxis(angleDelta, axisWorld) * Earth.transform.rotation;
        }

        // Measure net tilt (signed angle around tilt axis since grab-start)
        var netTilt = GetTiltAroundAxisDegrees();

        // Check if near +20 or -20
        var nearPositive = Mathf.Abs(netTilt - targetTilt) <= tiltMargin;
        var nearNegative = Mathf.Abs(netTilt + targetTilt) <= tiltMargin;

        if (nearPositive || nearNegative)
        {
            if (!isInRange)
            {
                // First time entering the target zone
                holdTimer = 0f;
                isInRange = true;
                indicator?.SetTiltInRange(true);
                Debug.Log("Entered valid range");
            }

            holdTimer += Time.deltaTime;

            // Trigger once hold duration is satisfied
            if (holdTimer >= holdTime)
            {
                Debug.Log("20 degree tilt achieved!");
                holdTimer = 0f;
                indicator?.DisableAll();
                Manager?.OnEarthTiltFinished();
            }
        }
        else
        {
            // Out of range -> reset hold
            indicator?.SetTiltInRange(false);
            if (isInRange) Debug.Log("Exited valid range.");
            isInRange = false;
            holdTimer = 0f;
        }
    }

    /// <summary>
    /// Resets state when grab ends.
    /// </summary>
    protected override void OnGrabEnd(Transform obj)
    {
        isInRange = false;
        holdTimer = 0f;
    }

    /// <summary>
    /// Computes signed tilt angle around the configured tilt axis,
    /// comparing Earth’s current orientation vs its grab/start orientation.
    /// </summary>
    float GetTiltAroundAxisDegrees()
    {
        // axis in world space relative to the initial rotation
        var axisWorld = initialRotation * tiltAxis.normalized;
            
        // choose a perpendicular reference vector
        var refVec = (Vector3.Dot(axisWorld, initialRotation * Vector3.forward) > 0.9f)
                         ? initialRotation * Vector3.up
                         : initialRotation * Vector3.forward;

        // project reference vectors onto the plane orthogonal to tilt axis
        var v0 = Vector3.ProjectOnPlane(refVec, axisWorld).normalized;
        var v1 = Vector3.ProjectOnPlane(Earth.transform.rotation * refVec, axisWorld).normalized;

        // signed angle around axis gives net tilt
        return Vector3.SignedAngle(v0, v1, axisWorld);
    }

    /// <summary>
    /// Cleanup on disable — resets grab/tilt state.
    /// </summary>
    void OnDisable()
    {
        isDragging = false;
        selectedObject = null;
        isInRange = false;
        holdTimer = 0f;
    }
}
