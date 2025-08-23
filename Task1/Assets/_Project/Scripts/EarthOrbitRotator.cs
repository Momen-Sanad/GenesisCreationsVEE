using UnityEngine;
using System;

public class EarthOrbitRotator : MonoBehaviour
{
    public Transform earth;     
    public Transform sun;        

    public float orbitSpeed = 10f;

    // callbacks
    Action onOrbitComplete;
    Action<int> onDayChanged;

    // internal state
    float orbitRadius = 0f;
    float initialAngleRad = 0f;            
    float accumulatedAngleDegrees = 0f;    // how many degrees advanced since StartOrbit
    bool isOrbiting = false;

    int dayCounter = 0;
    const int totalDays = 365;
    const float degreesPerDay = 360f / totalDays;


    // getters
    public int GetCurrentDay() => dayCounter;
    public bool IsOrbiting() => isOrbiting;
    
    void Awake()
    {
        if (!earth) 
           earth = this.transform;
    }

    /// <summary>
    /// Start orbiting. onComplete will be called when one full orbit (360° / 365 days) is finished.
    /// onDayUpdate is called each time the day counter increments.
    /// </summary>
    public void StartOrbit(Action onComplete, Action<int> onDayUpdate)
    {
        if (sun == null || earth == null)
            return;

        // compute radius and initial angle (in XZ plane)
        var dir = earth.position - sun.position;
        orbitRadius = new Vector2(dir.x, dir.z).magnitude;


        initialAngleRad = Mathf.Atan2(dir.z, dir.x); // radians
        accumulatedAngleDegrees = 0f;
        dayCounter = 0;

        this.onOrbitComplete = onComplete;
        this.onDayChanged = onDayUpdate;

        isOrbiting = true;

        // initial day callback
        onDayChanged?.Invoke(dayCounter);
    }


    public void StopOrbit() => isOrbiting = false;
    

    void Update()
    {
        if (!isOrbiting)
            return;

        // advance angle (degrees)
        var deltaDeg = orbitSpeed * Time.deltaTime;
        accumulatedAngleDegrees += deltaDeg;

        // clamp -> no overshoot beyond 360
        if (accumulatedAngleDegrees >= 360f)
            accumulatedAngleDegrees = 360f;

        // compute new angle in radians
        var totalAngleRad = initialAngleRad + accumulatedAngleDegrees * Mathf.Deg2Rad;

        // compute new XZ pos
        var newX = sun.position.x + Mathf.Cos(totalAngleRad) * orbitRadius;
        var newZ = sun.position.z + Mathf.Sin(totalAngleRad) * orbitRadius;
        earth.position = new Vector3(newX, earth.position.y, newZ);

        // update day counter
        int newDay = Mathf.FloorToInt(accumulatedAngleDegrees / degreesPerDay);
        // clamp newDay into [0, totalDays]
        newDay = Mathf.Clamp(newDay, 0, totalDays);

        if (newDay > dayCounter)
        {
            dayCounter = newDay;
            onDayChanged?.Invoke(dayCounter);
        }

        // if finished full revolution
        // ensure exact starting position to avoid visible tiny drift
        if (Mathf.Approximately(accumulatedAngleDegrees, 360f) || accumulatedAngleDegrees >= 360f)
        {
            var startPosition = sun.position + new Vector3(Mathf.Cos(initialAngleRad) * orbitRadius, earth.position.y, Mathf.Sin(initialAngleRad) * orbitRadius);
            earth.position = startPosition;

            // set dayCounter to final value (365)
            dayCounter = totalDays;
            onDayChanged?.Invoke(dayCounter);

            isOrbiting = false;

            // call completion callback
            onOrbitComplete?.Invoke();
        }
    }

}