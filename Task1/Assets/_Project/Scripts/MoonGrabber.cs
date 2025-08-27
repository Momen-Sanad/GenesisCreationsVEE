using UnityEngine;

public class MoonGrabber : DistanceGrabber
{
    public Transform earth;          
    public Transform planet;         
    public MoonHandler moonHandler;  

    public float orbitRadius = 5f;         
    [Range(0.1f, 3f)] 
    public float sensitivity = 1f; // 1 = drag across screen -> 360 degrees
    public bool forwardOnly = true;

    float angleRad;         
    float totalDegrees = 0; 

    protected override void OnGrabStart(Transform obj)
    {
        // Sync angle to current position so there is no jump on first drag.
        Vector3 dir = planet.position - earth.position;
        angleRad = Mathf.Atan2(dir.z, dir.x);
        Debug.Log("Started Grabbing");
    }

    protected override void OnGrabUpdate(Transform obj)
    {
        var dx = (Input.mousePosition - lastMousePosition).x;

        // Pixels -> radians: one full screen width == 2pi radians (scaled by sensitivity)
        var radPerPixel = (2f * Mathf.PI / Mathf.Max(1, Screen.width)) * sensitivity;

        var deltaRad = -dx * radPerPixel;
        if (forwardOnly && deltaRad < 0f) deltaRad = 0f;

        var prevRad = angleRad;
        angleRad += deltaRad;

        // Convert applied delta to degrees and clamp remaining progress to max 360°
        var advancedDeg = Mathf.Clamp((angleRad - prevRad) * Mathf.Rad2Deg, 0f, 360f - totalDegrees);
        totalDegrees += advancedDeg;

        // Lock at 360 when done
        if (Mathf.Approximately(advancedDeg, 0f) && totalDegrees >= 360f)
            return;

        // Recompute angle after clamp so position matches the clamped progress
        angleRad = prevRad + advancedDeg * Mathf.Deg2Rad;

        // Place the moon on the orbit
        planet.position = earth.position + new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * orbitRadius;

        // Inform handler so it can spawn the phase visuals at intervals
        moonHandler?.HandleMoonMoved(totalDegrees, planet);
    }


    // useless
    protected override void OnGrabEnd(Transform obj) => Debug.Log("Grab Ended");

    protected override bool TryGetClickedObject(out Transform clickedObject)
    {
        clickedObject = null;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit) && hit.transform == planet)
        {
            clickedObject = planet;
            return true;
        }
        return false;
    }
}
