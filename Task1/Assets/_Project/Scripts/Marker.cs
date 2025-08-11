using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Marker : MonoBehaviour
{
    [HideInInspector]
    public int orbitIndex = -1;

    [HideInInspector]
    public OrbitManager manager;

    [HideInInspector]
    public PlanetPlacement assignedPlanet; // Is set when a planet is snapped here

    [HideInInspector]
    public bool processed = false; // prevents double-handling

    void OnTriggerEnter(Collider other)
    {
        if (manager == null) return;

        Marker otherMarker = other.GetComponent<Marker>();
        if (otherMarker != null && otherMarker != this)
        {
            // Notify manager so it can decide what to do
            manager.HandleMarkerCollision(this, otherMarker);
        }
    }
}