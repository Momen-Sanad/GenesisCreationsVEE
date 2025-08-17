using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(LineRenderer))]
public class ObjectPickup : MonoBehaviour
{
    [Header("Quiz Settings")]
    public QuizActivator quizManager;
    int placedPlanetsCount = 0;

    public float maxReach = 4f;
    public float holdDistance = 2f;
    public float moveSpeed = 10f;

    Camera localCamera;
    Rigidbody heldObject;
    LineRenderer line;
    [SerializeField]
    OrbitManager orbitManager;

    public TransitionSystem transitionSystem;

    private List<GameObject> interactedPlanets = new List<GameObject>();

    public static bool isHoveringObject = false; // For camera control

    void Start()
    {
        localCamera = Camera.main;
        //quizManager.GetComponent<QuizManager>();
        SetupLineRenderer();
    }

    void Update()
    {
        DrawLaser();
        HandleInput();
    }

    /// <summary>
    /// Configures the <see cref="line"/> renderer for the pickup laser.
    /// </summary>
    void SetupLineRenderer()
    {
        line = GetComponent<LineRenderer>();
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        line.positionCount = 2;
        line.material = new Material(Shader.Find("Unlit/Color")) { color = Color.red };
    }

    /// <summary>
    /// Handles mouse input for picking up and dropping objects.
    /// </summary>
    void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (heldObject == null) TryPickup();
            else MoveHeldObject();
        }
        else if (heldObject != null) Drop();
    }

    /// <summary>
    /// Draws the laser and updates <see cref="isHoveringObject"/> status.
    /// </summary>
    void DrawLaser()
    {
        //Debug.Log("DrawLaser");
        //if (localCamera)
        //    Debug.Log("CameraFOund");
        var offsetOrigin = localCamera.transform.position + Vector3.down * 0.2f;
        var ray = new Ray(offsetOrigin, localCamera.transform.forward);
        line.SetPosition(0, ray.origin);

        if (Physics.Raycast(ray, out var hit, maxReach))
        {
            line.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("CelestialBody"))
            {
                isHoveringObject = true;
                line.material.color = Color.green;
            }
            else
            {
                isHoveringObject = false;
                line.material.color = Color.red;
            }
        }
        else
        {
            isHoveringObject = false;
            line.SetPosition(1, ray.origin + ray.direction * maxReach);
            line.material.color = Color.red;
        }
    }

    /// <summary>
    /// Attempts to pick up an object in front of the camera.
    /// </summary>
    void TryPickup()
    {
        var ray = new Ray(localCamera.transform.position, localCamera.transform.forward);

        if (Physics.Raycast(ray, out var hit, maxReach) && hit.collider.CompareTag("CelestialBody"))
        {
            var rb = hit.collider.GetComponent<Rigidbody>();

            if (rb != null && rb.useGravity && !rb.isKinematic)
            {
                var placement = rb.GetComponent<PlanetPlacement>();
                if (placement != null && placement.placed) 
                   return;

                heldObject = rb;
                heldObject.linearVelocity = Vector3.zero;
                heldObject.angularVelocity = Vector3.zero;

                // show tooltip
                PlanetToolTips.Show(hit.collider.gameObject);

                // record unique interacted planet
                if (!interactedPlanets.Contains(hit.collider.gameObject))
                    interactedPlanets.Add(hit.collider.gameObject);
            }
        }
    }

    /// <summary>
    /// Moves the held object to a position in front of the camera.
    /// </summary>
    void MoveHeldObject()
    {
        var targetPos = localCamera.transform.position + localCamera.transform.forward * holdDistance;
        var direction = targetPos - heldObject.position;
        heldObject.linearVelocity = direction * moveSpeed;
    }

    /// <summary>
    /// Drops the held object, attempting to snap it to a marker if it's a <see cref="PlanetPlacement"/>.
    /// </summary>
    void Drop()
    {
        if (heldObject == null) return;

        var placement = heldObject.GetComponent<PlanetPlacement>();
        if (placement != null && placement.TrySnapToTarget(FindFirstObjectByType<OrbitManager>()))
        {
            placedPlanetsCount++;
            Debug.Log("i got placed " + placedPlanetsCount);
            if (placedPlanetsCount == 8)
            {
                HandleFinish();
            }

            heldObject = null;
            return;
        }

        heldObject = null;
    }

    public void ClearPlanetTooltips()
    {
        if (interactedPlanets == null || interactedPlanets.Count == 0) return;

        // iterate and hide
        for (int i = 0; i < interactedPlanets.Count; i++)
        {
            var planet = interactedPlanets[i];
            if (planet != null)
            {
                PlanetToolTips.Hide(planet);
            }
        }

        // clear the list after hiding
        interactedPlanets.Clear();
    }

    public void HandleFinish()
    {
        orbitManager.ClearExistingChildren();
        ClearPlanetTooltips();
        StartCoroutine(HandleFinishRoutine());
    }

    private IEnumerator HandleFinishRoutine()
    {
        Debug.Log("about to transition");

        bool started = transitionSystem.StartTransition();
        Debug.Log("StartTransition returned: " + started);
        Debug.Log("transitioned");

        // Wait until transition is finished
        yield return new WaitUntil(() => !transitionSystem.isRunning);

        Debug.Log("check build orbits");
        Debug.Log("building orbits");
        orbitManager.BuildOrbits();
        orbitManager.ClearExistingMarker();
    }


}
