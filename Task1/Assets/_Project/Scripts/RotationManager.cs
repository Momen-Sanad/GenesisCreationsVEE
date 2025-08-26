using UnityEngine;
using TMPro;

public class RotationManager : MonoBehaviour
{
    public EarthRotator axisRotator;           
    public EarthOrbitRotator orbitRotator;     

    public TMP_Text dayCounterText;

    enum Phase { Phase1_Axis, Phase2_OrbitAvailable, Locked }
    Phase currentPhase = Phase.Phase1_Axis;

    bool isOrbiting = false;
    bool isLocked = false;

    void Start()
    {
        // Phase 1 active by default
        if (axisRotator)
            axisRotator.enabled = true;
        if (orbitRotator)
            orbitRotator.enabled = false;

        // TMP hidden until orbit begins (or until orbit completes)
        if (dayCounterText) 
            dayCounterText.gameObject.SetActive(false);

        // Subscribe to orbit events
        if (orbitRotator)
        {
            orbitRotator.onOrbitStarted += OnOrbitStarted;
            orbitRotator.onOrbitStopped += OnOrbitStopped;
            orbitRotator.onOrbitComplete += OnOrbitComplete;
            orbitRotator.onDayChanged += OnDayChanged;
        }
    }

    void OnDestroy()
    {
        if (orbitRotator)
        {
            orbitRotator.onOrbitStarted -= OnOrbitStarted;
            orbitRotator.onOrbitStopped -= OnOrbitStopped;
            orbitRotator.onOrbitComplete -= OnOrbitComplete;
            orbitRotator.onDayChanged -= OnDayChanged;
        }
    }

    void Update()
    {
        if (isLocked)
            return;

        // Auto-switch to Phase 2 when Phase1 boolean is true.
        if (currentPhase == Phase.Phase1_Axis && axisRotator != null)
        {
            // If the axis rotator has completed its rotation -> start Phase 2
            if (axisRotator.phase1Complete)
            {
                EnterPhase2();
            }
        }

        // keep UI in sync while orbiting
        if (isOrbiting && dayCounterText && orbitRotator)
        {
            dayCounterText.text = $"Day: {orbitRotator.GetCurrentDay()}";
        }
    }

    /// <summary>
    /// Switch manager to Phase 2 (orbit available).
    /// Phase 1 is considered done and axis rotator is disabled permanently.
    /// </summary>
    void EnterPhase2()
    {
        currentPhase = Phase.Phase2_OrbitAvailable;

        // Permanently disable axis rotator once Phase 1 is complete
        if (axisRotator)
            axisRotator.enabled = false;

        // Enable orbit rotator so it can receive grabs
        if (orbitRotator)
            orbitRotator.enabled = true;

        // Keep TMP hidden until the player actually starts dragging (onOrbitStarted will show it)
        if (dayCounterText)
            dayCounterText.gameObject.SetActive(false);
    }

    // Called when Earth is grabbed (orbit begins)
    void OnOrbitStarted()
    {
        if (isLocked) return;

        isOrbiting = true;

        // Show UI and ensure axis rotation is off
        if (dayCounterText)
        {
            dayCounterText.gameObject.SetActive(true);
            if (orbitRotator)
                dayCounterText.text = $"Day: {orbitRotator.GetCurrentDay()}";
        }

        if (axisRotator)
            axisRotator.enabled = false;
    }

    // Called when player releases before completing full orbit
    void OnOrbitStopped()
    {
        if (isLocked) return;

        isOrbiting = false;
    

        // Only re-enable axis rotator if Phase 1 was NOT completed.
        // If phase1Complete is true, Phase 1 is finished and axis rotator should remain disabled.
        if (axisRotator)
        {
            if (!axisRotator.phase1Complete)
            {
                // Return to Phase1 behavior so player can continue axis rotation
                axisRotator.enabled = true;
                currentPhase = Phase.Phase1_Axis;

                // Disable orbitRotator until re-entering Phase2
                if (orbitRotator)
                    orbitRotator.enabled = false;
            }
            else
            {
                // Phase1 is complete => keep axis rotator disabled and allow orbiting again by grabbing.
                currentPhase = Phase.Phase2_OrbitAvailable;
                if (orbitRotator)
                    orbitRotator.enabled = true;
            }
        }
    }

    // Called each time day increments
    void OnDayChanged(int dayCount)
    {
        if (dayCounterText)
            dayCounterText.text = $"Day: {dayCount}";
    }

    // Called when orbit completes (365 days)
    void OnOrbitComplete()
    {
        // lock interactions and keep the UI visible
        isLocked = true;
        currentPhase = Phase.Locked;
        isOrbiting = false;

        if (axisRotator)
            axisRotator.enabled = false;
        
        if (orbitRotator)
            orbitRotator.enabled = false;

        if (dayCounterText)
        {
            dayCounterText.gameObject.SetActive(true);
            if (orbitRotator)
                dayCounterText.text = $"Day: {orbitRotator.GetCurrentDay()}";
        }
    }
}
