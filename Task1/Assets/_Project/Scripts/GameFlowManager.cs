using UnityEngine;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    public ObjectPickup objectPickup;
    public OrbitManager orbitManager;
    public SceneTransitionManager sceneTransitionManager;
    public QuizActivator quizManager;

    int placedPlanetsCount = 0;

    void OnEnable()
    {
        objectPickup.OnPlanetPlaced += HandlePlanetPlaced;
    }

    void OnDisable()
    {
        objectPickup.OnPlanetPlaced -= HandlePlanetPlaced;
    }

    /// <summary>
    /// Called whenever a planet is placed successfully.
    /// </summary>
    void HandlePlanetPlaced(GameObject planet)
    {
        placedPlanetsCount++;
        Debug.Log("i got placed " + placedPlanetsCount);

        if (placedPlanetsCount == 8)
        {
            HandleFinish();
        }
    }

    /// <summary>
    /// Handles the logic of clearing orbits, tooltips, and starting transition/quiz.
    /// </summary>
    void HandleFinish()
    {
        orbitManager.ClearExistingChildren();
        objectPickup.ClearPlanetTooltips();
        StartCoroutine(HandleFinishRoutine());
    }

    /// <summary>
    /// Waits for scene transition, then rebuilds orbits and clears markers.
    /// </summary>
    IEnumerator HandleFinishRoutine()
    {
        Debug.Log("about to transition");

        sceneTransitionManager.StartSceneTransition();
        Debug.Log("StartSceneTransition called");

        // Wait until transition is finished
        yield return new WaitUntil(() => !sceneTransitionManager.transitionSystem.isRunning);

        Debug.Log("check build orbits");
        Debug.Log("building orbits");
        orbitManager.BuildOrbits();

        if (orbitManager.markers[0])
            orbitManager.ClearExistingMarker();
        
        quizManager.StartQuiz();
    }
}
