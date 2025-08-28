using Unity.VisualScripting;
using UnityEngine;

public class RotateEarth : MonoBehaviour
{
    public EarthOrbitRotator orbitRotator;

    [HideInInspector]
    public bool isOrbiting = false;

    [HideInInspector]
    public bool isLocked = false;

    bool orbitStarted = false;   // ensures orbiting works only after OnOrbitStarted
    bool isGrabActive = false;   // track grab state internally

    bool firstTime = true;

    //public GameObject Snow1;
    //public GameObject Desert1;
    //public GameObject Snow2;
    //public GameObject Desert2;

    bool usingPair1 = true; // keep track of which pair is active

    /// <summary>
    /// Subscribes to orbit events and enables orbiting when the component is enabled.
    /// </summary>
    void OnEnable()
    {
        if (orbitRotator != null)
        {
            orbitRotator.enabled = isOrbiting;

            orbitRotator.onOrbitStarted += OnOrbitStarted;
            orbitRotator.onOrbitStopped += OnOrbitStopped;
            orbitRotator.onOrbitComplete += OnOrbitComplete;
        }
    }

    /// <summary>
    /// Cleans up orbit event subscriptions and resets grab state when disabled.
    /// </summary>
    void OnDisable()
    {
        if (orbitRotator != null)
        {
            orbitRotator.onOrbitStarted -= OnOrbitStarted;
            orbitRotator.onOrbitStopped -= OnOrbitStopped;
            orbitRotator.onOrbitComplete -= OnOrbitComplete;
            orbitRotator.enabled = false;
        }

        isGrabActive = false;
    }

    /// <summary>
    /// Handles orbit progress, pair switching, and orbit completion checks.
    /// </summary>
    void Update()
    {
        if (isLocked || !isOrbiting || !orbitStarted || orbitRotator == null)
            return;

        float angle = orbitRotator.GetCurrentAngle();
        Debug.Log("RotateEarth: angle = " + angle);

        // Switch pairs once we pass 90 degrees
        //if (angle >= Mathf.Abs(90f) && usingPair1)
        //{
        //    Debug.Log("Switching to pair2");
        //    DeactivatePair1();
        //    ActivatePair2();
        //    usingPair1 = false;
        //}
        //else if (angle < Mathf.Abs(90f) && !usingPair1)
        //{
        //    Debug.Log("Switching to pair1");
        //    ActivatePair1();
        //    DeactivatePair2();
        //    usingPair1 = true;
        //}

        // Hard stop once we reach 180 degrees
        if (angle >= Mathf.Abs(180f))
        {
            OnOrbitComplete();
        }
    }

    /// <summary>
    /// Called when orbit begins. Initializes pair state and resets grab.
    /// </summary>
    public void OnOrbitStarted()
    {
        orbitStarted = true;
        isOrbiting = true;
        isLocked = false;

        //if (firstTime)
        //{
        //    // Ensure we always start with Pair1 active, Pair2 inactive
        //    ActivatePair1();
        //    DeactivatePair2();
        //    usingPair1 = true;
        //}

        firstTime = false;

        ResetGrab();

        Debug.Log("RotateEarth: Orbit phase started, Earth can now be grabbed.");
    }

    /// <summary>
    /// Resets grab state by toggling the component. 
    /// Used to force a re-init without breaking flow.
    /// </summary>
    private void ResetGrab()
    {
        enabled = false;
        enabled = true;
    }

    /// <summary>
    /// Marks grab as active (user is interacting).
    /// </summary>
    public void OnGrabStarted() => isGrabActive = true;

    /// <summary>
    /// Marks grab as inactive (user released).
    /// </summary>
    public void OnGrabEnded() => isGrabActive = false;

    /// <summary>
    /// Called when orbit is stopped before completion.
    /// </summary>
    void OnOrbitStopped()
    {
        Debug.Log("RotateEarth: orbit stopped.");
    }

    /// <summary>
    /// Called once the orbit completes 180 degrees. Locks orbit and positions Earth opposite the Sun.
    /// </summary>
    void OnOrbitComplete()
    {
        isLocked = true;
        isOrbiting = false;

        // Snap Earth to exactly 180 opposite Sun to avoid drift
        if (orbitRotator != null && orbitRotator.sun != null && orbitRotator.earth != null)
        {
            Vector3 direction = orbitRotator.earth.position - orbitRotator.sun.position;
            Vector3 oppositeDirection = Quaternion.Euler(0, 180f, 0) * direction;
            orbitRotator.earth.position = orbitRotator.sun.position + oppositeDirection;
            orbitRotator.enabled = false;
        }

        Debug.Log("RotateEarth: orbit complete — locked at 180.");
    }

    // --- Pair Control ---
    /// <summary>
    /// Activates Pair1 (Snow1 + Desert1).
    /// </summary>
    //public void ActivatePair1()
    //{
    //    if (Snow1 != null) Snow1.SetActive(true);
    //    if (Desert1 != null) Desert1.SetActive(true);
    //}

    ///// <summary>
    ///// Deactivates Pair1 (Snow1 + Desert1).
    ///// </summary>
    //public void DeactivatePair1()
    //{
    //    if (Snow1 != null) Snow1.SetActive(false);
    //    if (Desert1 != null) Desert1.SetActive(false);
    //}

    ///// <summary>
    ///// Activates Pair2 (Snow2 + Desert2).
    ///// </summary>
    //public void ActivatePair2()
    //{
    //    if (Snow2 != null) Snow2.SetActive(true);
    //    if (Desert2 != null) Desert2.SetActive(true);
    //}

    ///// <summary>
    ///// Deactivates Pair2 (Snow2 + Desert2).
    ///// </summary>
    //public void DeactivatePair2()
    //{
    //    if (Snow2 != null) Snow2.SetActive(false);
    //    if (Desert2 != null) Desert2.SetActive(false);
    //}
}
