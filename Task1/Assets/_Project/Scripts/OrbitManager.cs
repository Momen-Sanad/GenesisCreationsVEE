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

    [Tooltip("Angle (in degrees) along each orbit where the marker will be placed.")]
    public float markerAngleDegrees = 200f;

    [HideInInspector] public Vector3[] targetWorldPositions;
    [HideInInspector] public Marker[] markers; // Runtime references to created markers

    void Start()
    {

        BuildOrbits();
        BuildMarkers();
    }

    /// <summary>
    /// Creates orbit visuals (LineRenderers) and prepares marker target positions.
    /// Markers are NOT instantiated here (call BuildMarkers()).
    /// </summary>
    [ContextMenu("Rebuild Orbits")]
    public void BuildOrbits()
    {
        if (!ValidateBasicSetup()) return;

        PrepareOrbitData();

        ClearExistingOrbits();

        for (var i = 0; i < orbitRadii.Length; i++)
            CreateOrbit(i);
    }

    /// <summary>
    /// Creates marker GameObjects (prefab or fallback), sets up collider/rigidbody/Marker,
    /// and populates markers[] and targetWorldPositions if needed.
    /// </summary>
    [ContextMenu("Rebuild Markers")]
    public void BuildMarkers()
    {
        if (!ValidateBasicSetup()) return;

        // Ensure arrays exist and have the correct length
        PrepareOrbitData();

        // If target positions aren't computed (null or wrong length) compute them now so ForcePlanetIntoOrbit can use them.
        if (targetWorldPositions == null || targetWorldPositions.Length != orbitRadii.Length)
            targetWorldPositions = new Vector3[orbitRadii.Length];

        ClearExistingMarker();

        for (var i = 0; i < orbitRadii.Length; i++)
            CreateMarker(i, orbitRadii[i]);
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

        if (orbitRadii == null || orbitRadii.Length == 0)
        {
            targetWorldPositions = new Vector3[0];
            markers = new Marker[0];
            return;
        }

        targetWorldPositions = new Vector3[orbitRadii.Length];
        markers = new Marker[orbitRadii.Length];
    }

    /// <summary>
    /// Removes old orbit and marker objects (keeps compatibility).
    /// </summary>
    public void ClearExistingChildren()
    {
        ClearExistingOrbits();
        ClearExistingMarker();
    }

    /// <summary>
    /// Removes old orbit objects only.
    /// </summary>
    public void ClearExistingOrbits()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);

            if (child.name.StartsWith("Orbit_"))
                DestroyImmediate(child.gameObject);
        }
    }

    /// <summary>
    /// Removes old marker objects only.
    /// </summary>
    public void ClearExistingMarker()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);

            if (child.name.StartsWith("Marker_"))
                DestroyImmediate(child.gameObject);
        }
    }

    /// <summary>
    /// Creates a single orbit ring (LineRenderer). Does NOT create markers.
    /// </summary>
    void CreateOrbit(int index)
    {
        var radius = orbitRadii[index];
        var orbitGO = CreateOrbitObject(index, radius);
        DrawOrbitRing(orbitGO, radius);

        // compute the target marker position so GetTargetPosition works even if markers are not instantiated
        targetWorldPositions[index] = ComputeMarkerPosition(radius, markerAngleDegrees);
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
        if (lr == null) return;

        for (var s = 0; s <= circleSegments; s++)
        {
            var t = s / (float)circleSegments;
            var angle = t * Mathf.PI * 2f;
            var pos = sun.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            lr.SetPosition(s, pos);
        }
    }

    /// <summary>
    /// Creates a marker object for the orbit index.
    /// </summary>
    void CreateMarker(int index, float radius)
    {
        // defensive checks
        if (index < 0 || index >= orbitRadii.Length) return;
        if (sun == null) return;

        var markerPos = ComputeMarkerPosition(radius, markerAngleDegrees);
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
    /// Compute marker position given radius and angle (degrees) on orbit around sun.
    /// </summary>
    Vector3 ComputeMarkerPosition(float radius, float angleDegrees)
    {
        if (sun == null) return Vector3.zero;
        var angleRad = angleDegrees * Mathf.Deg2Rad;
        return sun.position + new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * radius;
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
        if (placement.orbitIndex < 0 || placement.orbitIndex >= orbitRadii.Length) return;

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

        // ensure markers array is large enough
        if (markers == null || markers.Length != orbitRadii.Length)
            markers = new Marker[orbitRadii.Length];

        markers[index] = markerComp;
    }

    /// <summary>
    /// Basic validation with helpful debug logs.
    /// </summary>
    bool ValidateBasicSetup()
    {
        if (sun == null)
        {
            Debug.LogWarning("[OrbitManager] Sun Transform is not assigned.");
            return false;
        }

        if (orbitRadii == null || orbitRadii.Length == 0)
        {
            Debug.LogWarning("[OrbitManager] orbitRadii is empty.");
            return false;
        }

        return true;
    }
}