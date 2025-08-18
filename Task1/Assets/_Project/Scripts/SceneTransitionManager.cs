using UnityEngine;
//to do in module 1 : make sun go to a ceratin place, then after that coroutine finishes, make all three do their respective transitions

/// <summary>
/// Scene-level orchestrator: manages camera behavior during scene transitions.
/// Optionally detaches the camera from its parent so parent rotations don’t affect it.
/// Disables DragCamera during transition.
/// Smoothly rotates the camera to look at the transition’s moving target.
/// Restores everything when the transition ends.
/// </summary>

[DisallowMultipleComponent]
public class SceneTransitionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    public TransitionSystem transitionSystem;     // Transition system controlling movement
    public TransitionSystem TransitionSystem => transitionSystem; // expose

    [Header("Camera Controls During Transition")]
    [Tooltip("If true, camera will rotate smoothly to look at the moving transition target.")]
    public bool lookAtDuringTransition = true;
    public float lookSpeed = 6f;
    public bool smoothLook = true;

    [Tooltip("If true, temporarily unparent Camera.main during the transition so parent rotations won't affect world rotation.")]
    public bool unparentCameraDuringTransition = false;

    // Cached state to restore camera after transition
    Transform cachedCameraParent;
    Vector3 cachedCameraLocalPos;
    Quaternion cachedCameraLocalRot;

    // Cache for DragCamera
    DragCamera cachedDragCamera;
    bool cachedDragCameraEnabled;

    void Awake()
    {
        if (transitionSystem != null)
        {
            transitionSystem.OnTransitionComplete += HandleTransitionComplete;
            transitionSystem.OnWaypointReached += HandleWaypointReached;
        }
        else
        {
            Debug.LogWarning("[SceneTransitionManager] No TransitionSystem assigned.");
        }
    }

    /// <summary>Call this to start the transition sequence.</summary>
    public void StartSceneTransition()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[SceneTransitionManager] No main camera found.");
            return;
        }

        DisableDragCamera(cam);

        if (unparentCameraDuringTransition)
            DetachCamera(cam);

        if (transitionSystem != null)
            transitionSystem.StartTransition();
        else
            Debug.LogError("[SceneTransitionManager] Cannot start transition — no TransitionSystem.");
    }

    void Update()
    {
        if (lookAtDuringTransition && transitionSystem != null && transitionSystem.isRunning)
        {
            Transform target = GetCurrentWaypoint();
            if (target != null)
                UpdateCameraLookSmooth(Camera.main, target.position);
        }
    }

    Transform GetCurrentWaypoint()
    {
        if (transitionSystem.waypoints == null || transitionSystem.waypoints.Length == 0)
            return null;

        // Currently just returns the last waypoint; can be expanded later
        int idx = Mathf.Clamp(transitionSystem.waypoints.Length - 1, 0, transitionSystem.waypoints.Length - 1);
        return transitionSystem.waypoints[idx];
    }

    public void HandleWaypointReached(int index)
    {
        Debug.Log($"[SceneTransitionManager] {transitionSystem.gameObject.name} reached waypoint {index + 1}");

        // Only care if Sun reaches its first waypoint
        if (transitionSystem.gameObject.name == "Sun" && index == 0)
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.OnSunFirstWaypointReached();
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] GameManager not found in scene.");
            }
        }
    }




    void HandleTransitionComplete()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            RestoreCamera(cam);
            RestoreDragCamera();
        }
    }

    void DetachCamera(Camera cam)
    {
        if (cam == null) return;

        cachedCameraParent = cam.transform.parent;
        cachedCameraLocalPos = cam.transform.localPosition;
        cachedCameraLocalRot = cam.transform.localRotation;

        cam.transform.SetParent(null, true);

        if (cam.transform.parent != null)
            Debug.LogWarning("[SceneTransitionManager] Camera still has a parent after DetachCamera(). Check other scripts.");
    }

    void RestoreCamera(Camera cam)
    {
        if (cam == null) return;

        if (unparentCameraDuringTransition && cachedCameraParent != null)
        {
            cam.transform.SetParent(cachedCameraParent, false);
            cam.transform.localPosition = cachedCameraLocalPos;
            cam.transform.localRotation = cachedCameraLocalRot;
            cachedCameraParent = null;
        }
    }

    void DisableDragCamera(Camera cam)
    {
        if (cam == null) return;

        cachedDragCamera = cam.GetComponent<DragCamera>();
        if (cachedDragCamera != null)
        {
            cachedDragCameraEnabled = cachedDragCamera.enabled;
            cachedDragCamera.enabled = false;
        }
    }

    void RestoreDragCamera()
    {
        if (cachedDragCamera != null)
        {
            cachedDragCamera.enabled = cachedDragCameraEnabled;
            cachedDragCamera = null;
        }
    }

    void UpdateCameraLookSmooth(Camera cam, Vector3 targetPosition)
    {
        if (cam == null) return;

        Vector3 dir = targetPosition - cam.transform.position;
        if (dir.sqrMagnitude <= 1e-8f) return;

        Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);

        if (smoothLook)
        {
            float t = Mathf.Clamp01(Time.deltaTime * lookSpeed);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, desired, t);
        }
        else
        {
            cam.transform.rotation = desired;
        }
    }
}