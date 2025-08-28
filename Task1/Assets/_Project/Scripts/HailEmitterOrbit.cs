using UnityEngine;

public class HailEmitterOrbit : MonoBehaviour
{
    public Transform earth;                  // Reference to the Earth
    public RotateEarth rotateEarth;          // Reference to Earth's rotation/orbit manager
    public float orbitRadius = 1f;           // Distance from Earth's center
    public Vector3 orbitAxis = Vector3.right; // Orbit axis (e.g., X = sideways orbit)
    public float orbitScale = 1f;            // Sensitivity to orbit angle

    private float lastEarthAngle = 0f;       // Store previous orbit angle

    void Start()
    {
        if (earth == null || rotateEarth == null || rotateEarth.orbitRotator == null)
        {
            Debug.LogError("HailEmitterOrbit: Earth or OrbitRotator reference not set!");
            enabled = false;
            return;
        }

        // Initialize to Earth’s current orbital angle
        lastEarthAngle = rotateEarth.orbitRotator.GetCurrentAngle();
    }

    void Update()
    {
        if (earth == null) return;

        // Get Earth’s current orbit angle (not tilt)
        float currentAngle = rotateEarth.orbitRotator.GetCurrentAngle();

        // Calculate change in orbit angle since last frame
        float deltaAngle = Mathf.DeltaAngle(lastEarthAngle, currentAngle);

        if (Mathf.Abs(deltaAngle) > 0.01f) // Only move if orbit angle actually changed
        {
            // Rotate the hail emitter around Earth according to the orbit change
            transform.RotateAround(
                earth.position,
                orbitAxis,
                deltaAngle * orbitScale
            );
        }

        // Update last angle for next frame
        lastEarthAngle = currentAngle;
    }
}
