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

    int currentIndex = 0;

    // Events
    public Action<int> OnWaypointReached;   // index of waypoint reached
    public Action OnTransitionComplete;

    public bool StartTransition()
    {
        if (waypoints == null || waypoints.Length == 0)
            return false;

        if (!isRunning)
        {
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

                    yield return null;
                }

                // snap to exact position
                transform.position = waypoint.position;

                OnWaypointReached?.Invoke(currentIndex);
                currentIndex++;
            }

            if (loop) currentIndex = 0;

        } while (loop);

        OnTransitionComplete?.Invoke();
        isRunning = false;
    }

    public void StopTransition()
    {
        StopAllCoroutines();
        isRunning = false;
    }
}
