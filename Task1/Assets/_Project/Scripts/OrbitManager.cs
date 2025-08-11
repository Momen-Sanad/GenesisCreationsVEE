using UnityEngine;

[ExecuteAlways]
public class OrbitManager : MonoBehaviour
{
    public Transform sun;
    public float[] orbitRadii; // Radii for orbits (index = orbit index)
    public Material[] orbitMaterial; // Material per orbit
    public int circleSegments = 128;
    public GameObject[] markerPrefab; // Marker prefab per orbit
    public float markerScale = 0.2f;

    [HideInInspector] public Vector3[] targetWorldPositions;
    [HideInInspector] public Marker[] markers; // Runtime references to created markers


    void Start() => BuildOrbits();

    /// <summary>
    /// Creates orbit visuals and markers.
    /// </summary>
    [ContextMenu("Rebuild Orbits")]
    public void BuildOrbits()
    {
        // Checks whether orbit setup data is valid.
        if (sun != null && orbitRadii != null && orbitRadii.Length > 0) return;

        PrepareOrbitData();
        ClearExistingChildren();

        for (var i = 0; i < orbitRadii.Length; i++)
            CreateOrbit(i);
    }

    /// <summary>
    /// Returns the target position of an orbit index.
    /// </summary>
    public Vector3 GetTargetPosition(int orbitIndex)
    {
        if (targetWorldPositions != null &&
            orbitIndex >= 0 &&
            orbitIndex < targetWorldPositions.Length)
        {
            return targetWorldPositions[orbitIndex];
        }

        return Vector3.zero;
    }


    /// <summary>
    /// Called when two <see cref="Marker"/> objects collide.
    /// </summary>
    public void HandleMarkerCollision(Marker MarkerA, Marker MarkerB)
    {
        if (MarkerA == null || MarkerB == null) return;
        if (MarkerA.assignedPlanet == null || MarkerB.assignedPlanet == null) return;
        if (MarkerA.processed && MarkerB.processed) return;

        ForcePlanetIntoOrbit(MarkerA.assignedPlanet);
        ForcePlanetIntoOrbit(MarkerB.assignedPlanet);

        MarkerA.processed = true;
        MarkerB.processed = true;
    }


    /// <summary>
    /// Prepares orbit data arrays based on <see cref="orbitRadii"/>.
    /// </summary>
    void PrepareOrbitData()
    {
        circleSegments = Mathf.Max(8, circleSegments);
        targetWorldPositions = new Vector3[orbitRadii.Length];
        markers = new Marker[orbitRadii.Length];
    }

    /// <summary>
    /// Removes old orbit and marker objects.
    /// </summary>
    void ClearExistingChildren()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("Orbit_") || child.name.StartsWith("Marker_"))
                DestroyImmediate(child.gameObject);
        }
    }

    /// <summary>
    /// Creates a single orbit ring and its marker.
    /// </summary>
    void CreateOrbit(int index)
    {
        var radius = orbitRadii[index];
        var orbitGO = CreateOrbitObject(index, radius);
        DrawOrbitRing(orbitGO, radius);
        CreateMarker(index, radius);
    }

    /// <summary>
    /// Creates the GameObject for the orbit ring.
    /// </summary>
    GameObject CreateOrbitObject(int index, float radius)
    {
        var orbitGO = new GameObject($"Orbit_{index}");
        orbitGO.transform.SetParent(transform, false);

        var lineRenderer = orbitGO.AddComponent<LineRenderer>();
        lineRenderer.positionCount = circleSegments + 1;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.02f;

        if (orbitMaterial != null && orbitMaterial.Length > index && orbitMaterial[index] != null)
            lineRenderer.material = orbitMaterial[index];

        lineRenderer.startColor = lineRenderer.endColor = Color.white;
        return orbitGO;
    }


    /// <summary>
    /// Draws a circular orbit line.
    /// </summary>
    void DrawOrbitRing(GameObject orbitGO, float radius)
    {
        var lr = orbitGO.GetComponent<LineRenderer>();
        for (var s = 0; s <= circleSegments; s++)
        {
            var t = s / (float)circleSegments;
            var angle = t * Mathf.PI * 2f;
            var pos = sun.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            lr.SetPosition(s, pos);
        }
    }


    /// <summary>
    /// Creates a marker object for the orbit.
    /// </summary>
    void CreateMarker(int index, float radius)
    {
        var randAngle = Random.Range(0f, Mathf.PI * 2f);
        var markerPos = sun.position + new Vector3(Mathf.Cos(randAngle), 0f, Mathf.Sin(randAngle)) * radius;
        targetWorldPositions[index] = markerPos;

        var prefab = SelectMarkerPrefab(index);
        GameObject markerGO;

        if (prefab != null)
            markerGO = Instantiate(prefab, markerPos, Quaternion.identity, transform);
        
        else
            markerGO = CreateFallbackMarker(markerPos);

        markerGO.name = "Marker_" + index;
        markerGO.transform.localScale = Vector3.one * markerScale;

        EnsureMarkerCollider(markerGO);
        EnsureMarkerRigidbody(markerGO);
        ConfigureMarkerComponent(markerGO, index);
    }

    /// <summary>
    /// Selects the correct marker prefab for the orbit.
    /// </summary>
    GameObject SelectMarkerPrefab(int index)
    {
        if (markerPrefab != null && markerPrefab.Length > index && markerPrefab[index] != null)
            return markerPrefab[index];
        if (markerPrefab != null && markerPrefab.Length > 0 && markerPrefab[0] != null)
            return markerPrefab[0];
        return null;
    }

    /// <summary>
    /// Forces a planet into its orbit position and locks it there.
    /// </summary>
    void ForcePlanetIntoOrbit(PlanetPlacement placement)
    {
        if (placement == null || placement.placed) return;

        placement.transform.position = GetTargetPosition(placement.orbitIndex);

        var rb = placement.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        placement.placed = true;
        placement.sun = sun;
        placement.orbitRadius = orbitRadii[placement.orbitIndex];

        var orbit = placement.GetComponent<PlanetOrbit>();
        if (orbit == null) orbit = placement.gameObject.AddComponent<PlanetOrbit>();
        orbit.Initialize(sun, placement.orbitRadius, placement.orbitAngularSpeed);

        placement.gameObject.tag = "Placed";
    }

    // Made to avoid warnings (totally optional)
    // ------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a simple sphere marker if no prefab is provided.
    /// </summary>
    GameObject CreateFallbackMarker(Vector3 position)
    {
        var markerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        markerGO.transform.SetParent(transform, false);
        markerGO.transform.position = position;
        return markerGO;
    }

    /// <summary>
    /// Ensures the marker has a trigger collider.
    /// </summary>
    void EnsureMarkerCollider(GameObject markerGO)
    {
        var collider = markerGO.GetComponent<Collider>();
        if (collider == null)
        {
            var sphere = markerGO.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
        }
        else collider.isTrigger = true;
    }

    /// <summary>
    /// Ensures the marker has a kinematic rigidbody.
    /// </summary>
    void EnsureMarkerRigidbody(GameObject markerGO)
    {
        var rb = markerGO.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = markerGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    /// <summary>
    /// Configures the <see cref="Marker"/> component for the marker.
    /// </summary>
    void ConfigureMarkerComponent(GameObject markerGO, int index)
    {
        var markerComp = markerGO.GetComponent<Marker>() ?? markerGO.AddComponent<Marker>();
        markerComp.orbitIndex = index;
        markerComp.manager = this;
        markers[index] = markerComp;
    }
}