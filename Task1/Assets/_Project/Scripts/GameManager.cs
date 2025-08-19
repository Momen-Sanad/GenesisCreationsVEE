using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip successSound;
    int SoundPlayed = 0;

    [Header("Planet Rings")]
    public RingDrawer earthRing;
    public RingDrawer moonRing;
    public RingDrawer sunRing;

    [Header("Scaling")]
    public Scaler scalerScript;

    public float newEarthRingRadius = 2;
    public float newMoonRingRadius = 1;
    public float newSunRingRadius = 5;

    YScaler[] yScalerScripts;

    [Header("Transitions")]
    [Tooltip("Assign the SceneTransitionManager objects for all planets here.")]
    public SceneTransitionManager[] transitionManagers;
    int sunWaypointIndex = 0;

    void Start()
    {
        // Find all YScaler scripts in the scene automatically
        yScalerScripts = FindObjectsByType<YScaler>(FindObjectsSortMode.None);

        // Ensure scalerScript is initially disabled
        if (scalerScript)
            scalerScript.enabled = false;
    }

    void Update() =>
        HandlePlanetScalingLogic();
    

    /// <summary>
    /// Handles the logic for checking if all planets are scaled correctly
    /// and triggering the appropriate actions (enabling/disabling scripts, playing sounds, updating rings).
    /// </summary>
    void HandlePlanetScalingLogic()
    {
        if (!AllPlanetsScaledCorrectly())
            return;

        Debug.Log("All planets scaled correctly 1");

        EnableScalerDisableYScaler();
        HandleSuccessSound();
        UpdateRingRadius();

        if (SoundPlayed == 2)
        {
            Debug.Log("All planets scaled correctly 2");

            ClearAllRings();

            // Start Sun only at the beginning
            foreach (var manager in transitionManagers)
            {
                if (manager.TransitionSystem.gameObject.name == "Sun")
                {
                    manager.StartSceneTransition();
                    break;
                }
            }

            DisableScalers();

        }
    }

    public void OnSunFirstWaypointReached()
    {
        Debug.Log("[GameManager] Sun reached its first waypoint! Starting other planets...");

        foreach (var manager in transitionManagers)
        {
            if (manager.TransitionSystem.gameObject.name != "Sun")
            {
                manager.StartSceneTransition();
            }
        }
    }

    void DisableScalers()
    {
        if (scalerScript)
            scalerScript.enabled = false;

        foreach (var ys in yScalerScripts)
            ys.enabled = false;
    }
    
    void EnableScalerDisableYScaler()
    {
        if (scalerScript)
            scalerScript.enabled = true;

        foreach (var ys in yScalerScripts)
            ys.enabled = false;
    }

    void HandleSuccessSound()
    {
        if (SoundPlayed == 1)
        {
            Debug.Log("Playing second success sound.");
            AudioSource.PlayClipAtPoint(successSound, transform.position);
            SoundPlayed = 2;
            enabled = false; // Stop checking after triggered
            return;
        }

        if (SoundPlayed == 0)
        {
            Debug.Log("Playing first success sound.");
            AudioSource.PlayClipAtPoint(successSound, transform.position);
            SoundPlayed = 1;
        }
    }

    void UpdateRingRadius()
    {
        earthRing.SetRingRadius(newEarthRingRadius);
        moonRing.SetRingRadius(newMoonRingRadius);
        sunRing.SetRingRadius(newSunRingRadius);
    }

    //void StartAllTransitions()
    //{
    //    if (transitionManagers == null || transitionManagers.Length == 0)
    //        return;

    //    foreach (var manager in transitionManagers)
    //    {
    //        if (manager)
    //            manager.StartSceneTransition();
    //    }
    //}

    bool AllPlanetsScaledCorrectly() =>
        earthRing != null && moonRing != null && sunRing != null &&
        earthRing.IsScaledCorrectly &&
        moonRing.IsScaledCorrectly &&
        sunRing.IsScaledCorrectly;

    void ClearAllRings()
    {
        if (earthRing)
            earthRing.ClearRing();

        if (moonRing)
            moonRing.ClearRing();

        if (sunRing)
            sunRing.ClearRing();
    }

    public void HandleWaypointReached()
    {
        Debug.Log("[GameManager] Sun reached first waypoint — starting Moon & Earth transitions");

        foreach (var manager in transitionManagers)
        {
            if (manager.gameObject.name == "Moon" || manager.gameObject.name == "Earth")
                manager.StartSceneTransition();
            
        }
    }

    public void SetSunWaypoint(int index)
    {
        sunWaypointIndex = index;
    }
}
