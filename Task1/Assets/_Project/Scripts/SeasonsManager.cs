using UnityEngine;

/// <summary>
/// Manages seasonal phases by coordinating EarthTilt and RotateEarth.
/// </summary>
public class SeasonsManager : MonoBehaviour
{
    public TiltEarth earthTilt;
    public RotateEarth rotateEarth;
    public GameObject newCamera;

    void Awake()
    {
        // Ensure Phase 2 is locked and disabled by default.
        rotateEarth.enabled= false;
    }

    public void OnEarthTiltFinished()
    {
        // disable phase 1
        earthTilt.enabled = false;

        // enable phase 2
        Debug.Log("Earth tilt completed. Starting Phase 2...");


        // move + rotate camera to match Newcamera
        if (newCamera)
        {
            Camera.main.transform.position = newCamera.transform.position;
            Camera.main.transform.rotation = newCamera.transform.rotation;
        }

        // allow orbiting
        if (rotateEarth != null)
        {
            rotateEarth.isLocked = false;
            rotateEarth.enabled = true;

            // Start orbiting via the proper entry point
            rotateEarth.OnOrbitStarted();
        }
    }
}
