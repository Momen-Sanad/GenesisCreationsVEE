using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetPlacement : MonoBehaviour
{
    public int orbitIndex = -1; // Assigned in inspector: which orbit this planet belongs to
    public float placementThreshold = 0.5f; // Margain of error
    public bool placed = false; 

    [Header("Orbit settings (assigned at placement)")]
    public float orbitRadius;
    public Transform sun;
    public float orbitAngularSpeed = 0.5f;

    Rigidbody rb;


    void Awake() => rb = GetComponent<Rigidbody>();

    /// <summary>
    /// Attempts to snap the planet to its target orbit position.
    /// </summary>
    public bool TrySnapToTarget(OrbitManager manager)
    {
        if (!IsPlacementValid(manager)) return false;

        var target = manager.GetTargetPosition(orbitIndex);
        var dist = Vector3.Distance(transform.position, target);

        if (dist > placementThreshold) return false;

        SnapToPosition(target);
        LockPhysics();
        AssignOrbitParameters(manager);
        MarkAsPlaced();
        NotifyMarker(manager);
        EnsurePlanetOrbitComponent();

        return true;
    }

    /// <summary>
    /// Checks if the planet can be placed in the target orbit.
    /// </summary>
    bool IsPlacementValid(OrbitManager manager) =>
        !placed && manager != null && orbitIndex >= 0 && orbitIndex < manager.orbitRadii.Length;


    /// <summary>
    /// Moves the planet to the given position.
    /// </summary>
    void SnapToPosition(Vector3 target) => transform.position = target;


    /// <summary>
    /// Stops all physics movement and disables gravity.
    /// </summary>
    void LockPhysics()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    /// <summary>
    /// Assigns orbit parameters from the <see cref="OrbitManager"/>.
    /// </summary>
    void AssignOrbitParameters(OrbitManager manager)
    {
        sun = manager.sun;
        orbitRadius = manager.orbitRadii[orbitIndex];
        placed = true;
    }

    /// <summary>
    /// Marks the planet as placed and unselectable
    /// since selectable objects are tagged by "CelestialObject"
    /// </summary>
    void MarkAsPlaced() => gameObject.tag = "Placed";

    /// <summary>
    /// Notifies the marker in the <see cref="OrbitManager"/> that it's now occupied.
    /// </summary>
    void NotifyMarker(OrbitManager manager)
    {
        if (manager.markers != null && orbitIndex >= 0 && orbitIndex < manager.markers.Length)
        {
            var marker = manager.markers[orbitIndex];
            if (marker != null) marker.assignedPlanet = this;
        }
    }

    /// <summary>
    /// Adds or initializes the <see cref="PlanetOrbit"/> component.
    /// </summary>
    void EnsurePlanetOrbitComponent()
    {
        var orbit = GetComponent<PlanetOrbit>();
        if (orbit == null)
            orbit = gameObject.AddComponent<PlanetOrbit>();

        orbit.Initialize(sun, orbitRadius, orbitAngularSpeed);
    }

}
