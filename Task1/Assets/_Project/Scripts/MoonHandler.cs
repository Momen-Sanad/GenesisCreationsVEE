using UnityEngine;

public class MoonHandler : MonoBehaviour
{
    public MoonTransitionSystem moonTransitionSystem;

    public GameObject moonVisualPrefab;

    public Transform visualParent;

    public float stepDegrees = 45f;

    public bool stopAtFinalPhase = true;

    public DistanceGrabber grabberToDisable;

    
    // Order matters and matches the 8 evenly spaced phases
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

        if(nextPhaseIndex == 0)
        {
            SpawnVisualPhase(realMoon, "New Moon");
            nextPhaseIndex++;
        }

        // Enforce forward only movement
        if (totalDegrees < lastReportedDegrees)
            totalDegrees = lastReportedDegrees;

        // Spawn any phases crossed (robust even if user jumps multiple intervals)
        while (nextPhaseIndex < PhaseNames.Length &&
               totalDegrees >= nextPhaseIndex * stepDegrees)
        {
            SpawnVisualPhase(realMoon, PhaseNames[nextPhaseIndex]);
            nextPhaseIndex++;
        }

        lastReportedDegrees = totalDegrees;

        // If we reached/passed the last phase, finish.
        if (nextPhaseIndex >= PhaseNames.Length)
            CompleteCycle();
    }

    void SpawnVisualPhase(Transform realMoon, string phaseName)
    {
        if (!moonVisualPrefab || !realMoon) return;

        var clone = Instantiate(moonVisualPrefab, realMoon.position, realMoon.rotation, visualParent);
        clone.name = phaseName;

        // Make clone visual-only (not grabbable, no physics)
        var rb = clone.GetComponent<Rigidbody>();
        if (rb) Destroy(rb);

        var col = clone.GetComponent<Collider>();
        if (col) Destroy(col);
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
