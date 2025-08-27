using System;
using UnityEngine;

public class MoonHandler : MonoBehaviour
{
    public MoonTransitionSystem moonTransitionSystem;

    [Header("References")]
    public Transform visualParent;  
    public DistanceGrabber grabberToDisable;

    [Header("Phases")]
    public float stepDegrees = 45f;
    public bool stopAtFinalPhase = true;

    static readonly string[] PhaseNames = new[]
    {
        "New Moon",
        "Waxing Crescent",
        "First Quarter",
        "Waxing Gibbous",
        "Full Moon",
        "Waning Gibbous",
        "Third Quarter",
        "Waning Crescent"
    };

    int nextPhaseIndex = 0;
    float lastReportedDegrees = 0f;
    bool completed;

    /// <summary>
    /// Called by MoonGrabber each drag update with absolute progress in degrees [0 -> 360].
    /// </summary>
    public void HandleMoonMoved(float totalDegrees, Transform realMoon)
    {
        if (completed)
            return;

        if (nextPhaseIndex == 0)
        {
            // show the first phase
            ActivateMoon(nextPhaseIndex);
            nextPhaseIndex++;
        }

        // forward only movement
        if (totalDegrees < lastReportedDegrees)
            totalDegrees = lastReportedDegrees;

        // Activate any phases crossed (robust even if user jumps multiple intervals)
        while (nextPhaseIndex < PhaseNames.Length &&
               totalDegrees >= nextPhaseIndex * stepDegrees)
        {
            ActivateMoon(nextPhaseIndex);
            nextPhaseIndex++;
        }

        lastReportedDegrees = totalDegrees;

        // If we reached/passed the last phase, finish.
        if (nextPhaseIndex >= PhaseNames.Length)
            CompleteCycle();
    }

    void ActivateMoon(int phaseIndex)
    {
        if (!visualParent || phaseIndex < 0 || phaseIndex >= visualParent.childCount)
            return;

        Transform moon = visualParent.GetChild(phaseIndex);
        if (moon != null)
        {
            moon.gameObject.SetActive(true);
            
            // activate all children of the moon
            for (int i = 0; i < moon.childCount; i++)
                moon.GetChild(i).gameObject.SetActive(true);
            
            moon.name = PhaseNames[phaseIndex]; // rename child
        }
    }

    void CompleteCycle()
    {
        if (completed)
            return;

        completed = true;

        if (stopAtFinalPhase && grabberToDisable)
            grabberToDisable.enabled = false; // prevents further dragging

        OnFullCycleComplete(); // start transition chain
    }

    /// <summary>
    /// Called when phases are complete
    /// </summary>
    public void OnFullCycleComplete()
    {
        if (moonTransitionSystem)
            moonTransitionSystem.HandleMoonCycleComplete();
    }
}