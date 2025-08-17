using UnityEngine;
using System;
using System.Collections;

public class TransitionSystem : MonoBehaviour
{
    [Tooltip("Waypoints (empty gameobjects) for the transition path")]
    public Transform[] waypoints;

    [Tooltip("Movement speed in units per second")]
    public float moveSpeed = 5f;

    [Tooltip("If true, loop through waypoints continuously")]
    public bool loop = false;

    public bool isRunning { get; private set; } = false;

    [Header("Camera Look Settings")]
    public bool lookAtDuringTransition = true;
    public float lookSpeed = 6f;
    public bool smoothLook = true;
    [Tooltip("If true, temporarily unparent Camera.main during the transition so parent rotations won't affect camera world rotation.")]
    public bool unparentCameraDuringTransition = false;

    // if unparenting, store parent's info to restore later
    Transform cachedCameraParent;
    Vector3 cachedCameraLocalPos;
    Quaternion cachedCameraLocalRot;


    DragCamera cachedDragCamera;
    bool cachedDragCameraEnabled;

    int currentIndex = 0;
    
    // Events for flexibility
    public Action<int> OnWaypointReached;   // called with index of waypoint
    public Action OnTransitionComplete;

    public bool StartTransition()
    {
        if (waypoints == null || waypoints.Length == 0)
            return false;

        if (!isRunning)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                // disable DragCamera (if present)
                cachedDragCamera = (DragCamera) (cam.GetComponent("DragCamera") as MonoBehaviour);
                if (cachedDragCamera != null)
                {
                    cachedDragCameraEnabled = cachedDragCamera.enabled;
                    cachedDragCamera.enabled = false;
                }

                // optionally unparent camera to avoid parent rotations affecting world rotation
                if (unparentCameraDuringTransition && cam.transform.parent != null)
                {
                    cachedCameraParent = cam.transform.parent;
                    cachedCameraLocalPos = cam.transform.localPosition;
                    cachedCameraLocalRot = cam.transform.localRotation;
                    cam.transform.parent = null; // keep world position/rotation as-is
                }
            }

            StartCoroutine(RunTransition());
            return true;
        }

        return false;
    }

    IEnumerator RunTransition()
    {
        isRunning = true;
        currentIndex = 0;

        do
        {
            while (currentIndex < waypoints.Length)
            {
                Transform waypoint = waypoints[currentIndex];

                // move until close enough
                while (Vector3.Distance(transform.position, waypoint.position) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        waypoint.position,
                        moveSpeed * Time.deltaTime
                    );

                    // smooth camera look each frame while moving (if enabled)
                    if (lookAtDuringTransition)
                        UpdateCameraLookSmooth(waypoint.position);

                    yield return null;
                }

                // snap to exact position for the object, but camera still rotates smoothly
                transform.position = waypoint.position;
                if (lookAtDuringTransition)
                    UpdateCameraLookSmooth(waypoint.position);

                OnWaypointReached?.Invoke(currentIndex);
                currentIndex++;
            }

            if (loop) currentIndex = 0;

        } while (loop);

        OnTransitionComplete?.Invoke();
        isRunning = false;

        // restore camera parent if we unparented it
        var cam = Camera.main;
        if (cam != null && unparentCameraDuringTransition && cachedCameraParent != null)
        {
            // restore parent and local transform to preserve previous relative pose
            cam.transform.parent = cachedCameraParent;
            cam.transform.localPosition = cachedCameraLocalPos;
            cam.transform.localRotation = cachedCameraLocalRot;

            cachedCameraParent = null;
        }

        // restore DragCamera component state
        if (cachedDragCamera != null)
        {
            cachedDragCamera.enabled = cachedDragCameraEnabled;
            cachedDragCamera = null;
        }
    }

    /// <summary>
    /// Smoothly rotates Camera.main to look at this moving object (this.transform).
    /// Uses Slerp with a factor of Time.deltaTime * lookSpeed for smooth damping.
    /// </summary>
    void UpdateCameraLookSmooth(Vector3 targetPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // world-space direction from camera to target
        Vector3 dir = targetPosition - cam.transform.position;
        if (dir.sqrMagnitude <= 0.000001f) return; // avoid NaNs if camera is exactly at target

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

    public void StopTransition()
    {
        StopAllCoroutines();
        isRunning = false;

        // restore camera parent if needed
        var cam = Camera.main;
        if (cam != null && unparentCameraDuringTransition && cachedCameraParent != null)
        {
            cam.transform.parent = cachedCameraParent;
            cam.transform.localPosition = cachedCameraLocalPos;
            cam.transform.localRotation = cachedCameraLocalRot;
            cachedCameraParent = null;
        }

        if (cachedDragCamera != null)
        {
            cachedDragCamera.enabled = cachedDragCameraEnabled;
            cachedDragCamera = null;
        }
    }
}