using UnityEngine;
using System;

/// <summary>
/// Orbit the Earth around the Sun using mouse drag (inherits DistanceGrabber).
/// Belt behavior: only X and Z change (Y is preserved).
/// Tracks days passed (0 -> 365) based on accumulated angle.
/// </summary>
public class EarthOrbitRotator : DistanceGrabber
{
    public Transform sun;               
    public Transform earth;            

    public float orbitSensitivity = 0.2f; // how many degrees per pixel moved
    public float maxRadiusTolerance = 0.0001f;

    // callbacks RotationManager can subscribe to:
    public Action<int> onDayChanged;
    public Action onOrbitStarted;
    public Action onOrbitStopped;   // when player releases before completing full orbit
    public Action onOrbitComplete;  // when full 360° / 365 days completed

    // internal state
    float orbitRadius = 0f;

    // baseAngleRad is computed once at Awake() and stays constant across grabs.
    // accumulatedAngleDegrees is the total degrees moved since the start of the app (persisting across grabs).
    float baseAngleRad = 0f;                
    float accumulatedAngleDegrees = 0f;     

    bool isOrbiting = false;
    int dayCounter = 0;
    const int totalDays = 365;
    readonly float degreesPerDay = 360f / totalDays;

    Renderer earthRenderer;

    void Awake()
    {
        // default earth transform if not supplied
        if (!earth)
            earth = this.transform;
        
        earthRenderer = earth.GetComponentInChildren<Renderer>();

        // initialize base angle & orbit radius from current earth position once
        if (sun != null && earth != null)
        {
            var dir = earth.position - sun.position;
            orbitRadius = new Vector2(dir.x, dir.z).magnitude;


            // This is the constant reference angle in radians for the simulation.
            baseAngleRad = Mathf.Atan2(dir.z, dir.x);

            // accumulatedAngleDegrees starts at 0 (no movement yet); it persists across grabs.
            accumulatedAngleDegrees = 0f;
            dayCounter = 0;
        }
    }

    
    protected override bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            // allow clicking either by exact earth transform or tag "Earth"
            if (hit.transform == earth || hit.transform == earth.transform || hit.transform.CompareTag("Earth"))
            {
                clickedObject = hit.transform;
                return true;
            }
        }

        return false;
    }

    protected override void OnGrabStart(Transform obj)
    {
        if (!sun || !earth)
            return;

        // Recompute orbitRadius in case external systems changed positions while not dragging
        var dir = earth.position - sun.position;
        orbitRadius = new Vector2(dir.x, dir.z).magnitude;

        if (orbitRadius <= maxRadiusTolerance || float.IsNaN(orbitRadius) || float.IsInfinity(orbitRadius))
            return;

        // Setting isOrbiting true allows ApplyDeltaAngle to modify accumulatedAngleDegrees.
        isOrbiting = true;

        // defensive: ensure visible and scaled
        //if (earthRenderer != null && !earthRenderer.enabled)
        //    earthRenderer.enabled = true;
        //if (earth.localScale.sqrMagnitude <= Mathf.Epsilon)
        //    earth.localScale = Vector3.one;

        onOrbitStarted?.Invoke();
        onDayChanged?.Invoke(dayCounter); // update UI with current day
    }

    protected override void OnGrabUpdate(Transform obj)
    {
        // lastMousePosition comes from DistanceGrabber (previous frame)
        var mouseDelta = Input.mousePosition - lastMousePosition;

        // Horizontal mouse movement changes angle around sun (XZ plane)
        float deltaAngle = mouseDelta.x * orbitSensitivity; // degrees

        ApplyDeltaAngle(deltaAngle);
    }

    protected override void OnGrabEnd(Transform obj)
    {
        if (isOrbiting)
        {
            isOrbiting = false;

            if (dayCounter < totalDays)
                onOrbitStopped?.Invoke();
        }
    }

   
    // Apply a delta to the persistent accumulatedAngleDegrees and update position + day counter.
    void ApplyDeltaAngle(float deltaDeg)
    {
        if (!isOrbiting)
            return;

        // Update total accumulated degrees
        accumulatedAngleDegrees += deltaDeg;

        // Clamp between 0 and 360 so we can detect full orbit
        if (accumulatedAngleDegrees < 0f) accumulatedAngleDegrees = 0f;
        if (accumulatedAngleDegrees > 360f) accumulatedAngleDegrees = 360f;

        // compute the absolute angle in radians using the constant baseAngleRad
        var totalAngleRad = baseAngleRad + accumulatedAngleDegrees * Mathf.Deg2Rad;

        // compute new position on the XZ belt
        var newX = sun.position.x + Mathf.Cos(totalAngleRad) * orbitRadius;
        var newZ = sun.position.z + Mathf.Sin(totalAngleRad) * orbitRadius;
        earth.position = new Vector3(newX, earth.position.y, newZ);

        // update day counter
        var newDay = Mathf.FloorToInt(accumulatedAngleDegrees / degreesPerDay);
        newDay = Mathf.Clamp(newDay, 0, totalDays);

        if (newDay > dayCounter)
        {
            dayCounter = newDay;
            onDayChanged?.Invoke(dayCounter);
        }

        // check completion
        if (accumulatedAngleDegrees >= 360f || dayCounter >= totalDays)
        {
            // snap to exact starting position (using baseAngleRad) to avoid tiny drift
            Vector3 startPosition = sun.position + 
                new Vector3(Mathf.Cos(baseAngleRad) * orbitRadius, earth.position.y, Mathf.Sin(baseAngleRad) * orbitRadius);
            
            earth.position = startPosition;

            // finalize day count
            dayCounter = totalDays;
            onDayChanged?.Invoke(dayCounter);

            isOrbiting = false;

            onOrbitComplete?.Invoke();
        }
    }

    // helpers for external queries
    public int GetCurrentDay()
    {
        return dayCounter;
    }
}