using UnityEngine;
using System;

/// <summary>
/// Concrete implementation of DistanceGrabber for the Earth.
/// Allows grabbing with the mouse, and rotates Earth around its axis
/// to simulate a day/night cycle.
/// Exposes "phase1Complete" which becomes true after 360° of axis rotation
/// (accumulated across grabs).
/// </summary>
public class EarthRotator : DistanceGrabber
{
    public float rotationSensitivity = 0.2f;

    // Phase 1 completion state
    public bool phase1Complete = false;

    // persistent across grabs: total absolute degrees rotated around axis
    float accumulatedAxisDegrees = 0f;
    const float degreesToCompletePhase = 360f;

    // Allow subscribing to the event when phase1 completes
    public Action onPhase1Complete;

    protected override void OnGrabStart(Transform obj)
    {
        Debug.Log("Started grabbing Earth");
    }

    protected override void OnGrabUpdate(Transform obj)
    {
        if (!obj.CompareTag("Earth")) return;

        // Mouse delta since last frame
        var mouseDelta = Input.mousePosition - lastMousePosition;

        // Compute rotation delta (degrees)
        var deltaDegrees = -mouseDelta.x * rotationSensitivity;

        // Apply rotation to the earth
        obj.Rotate(Vector3.up, deltaDegrees, Space.Self);

        // Accumulate absolute rotation towards phase completion if not already completed
        if (!phase1Complete)
        {
            accumulatedAxisDegrees += (deltaDegrees);

            if (Mathf.Abs(accumulatedAxisDegrees) >= degreesToCompletePhase)
            {
                // clamp to +360 or -360 depending on sign to avoid overshoot
                accumulatedAxisDegrees = Mathf.Sign(accumulatedAxisDegrees) * degreesToCompletePhase;
                phase1Complete = true;
                Debug.Log($"Phase 1 complete: accumulatedAxisDegrees = {accumulatedAxisDegrees}°");

                // Notify listeners
                onPhase1Complete?.Invoke();
            }
        }
    }

    protected override void OnGrabEnd(Transform obj)
    {
        Debug.Log("Released Earth");
    }

    protected override bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;

        // Raycast from mouse position into the scene
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("Earth"))
            {
                clickedObject = hit.transform;
                return true;
            }
        }
        return false;
    }
}
