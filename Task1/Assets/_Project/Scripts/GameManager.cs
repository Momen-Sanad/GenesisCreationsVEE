using UnityEditor.Playables;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip successSound;
    private int SoundPlayed = 0;

    [Header("Planet Rings")]
    public RingDrawer earthRing;
    public RingDrawer moonRing;
    public RingDrawer sunRing;

    public Scaler scalerScript;

    public float newEarthRingRadius = 2;
    public float newMoonRingRadius = 1;
    public float newSunRingRadius = 5;

    private YScaler[] yScalerScripts;

    void Start()
    {
        // Find all YScaler scripts in the scene automatically
        yScalerScripts = FindObjectsByType<YScaler>(FindObjectsSortMode.None);

        // Ensure scalerScript is initially disabled
        if (scalerScript != null)
            scalerScript.enabled = false;
    }


    void Update()
    {
        HandlePlanetScalingLogic();
    }

    /// <summary>
    /// Handles the logic for checking if all planets are scaled correctly
    /// and triggering the appropriate actions (enabling/disabling scripts, playing sounds, updating rings).
    /// </summary>
    private void HandlePlanetScalingLogic()
    {
        if (!AllPlanetsScaledCorrectly())
            return;

        Debug.Log("Test");

        EnableScalerDisableYScaler();
        HandleSuccessSound();
        UpdateRingRadius();
    }

    /// <summary>
    /// Enables the Scaler script and disables all YScaler scripts in the scene.
    /// </summary>
    private void EnableScalerDisableYScaler()
    {
        if (scalerScript != null)
            scalerScript.enabled = true;

        foreach (var ys in yScalerScripts)
            ys.enabled = false;
    }

    /// <summary>
    /// Handles playing the success sound for task progression.
    /// Ensures each sound is only played once in the correct order.
    /// Disables this script when final sound is played.
    /// </summary>
    private void HandleSuccessSound()
    {
        if (SoundPlayed == 1)
        {
            Debug.Log("sound played");
            AudioSource.PlayClipAtPoint(successSound, transform.position);
            SoundPlayed = 2;
            enabled = false; // Stop checking after triggered
            return;
        }

        if (SoundPlayed == 0)
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position);
            Debug.Log("sound played");
            SoundPlayed = 1;
        }
    }

    /// <summary>
    /// Updates the radius of the Earth, Moon, and Sun rings to new values.
    /// </summary>
    private void UpdateRingRadius()
    {
        earthRing.SetRingRadius(newEarthRingRadius);
        moonRing.SetRingRadius(newMoonRingRadius);
        sunRing.SetRingRadius(newSunRingRadius);
    }

    /// <summary>
    /// Checks if all planets exist and have been scaled correctly.
    /// </summary>
    /// <returns>True if all planets are present and scaled correctly; otherwise false.</returns>
    private bool AllPlanetsScaledCorrectly() =>
    
        earthRing != null && moonRing != null && sunRing != null &&
        earthRing.IsScaledCorrectly &&
        moonRing.IsScaledCorrectly &&
        sunRing.IsScaledCorrectly;
    
}
