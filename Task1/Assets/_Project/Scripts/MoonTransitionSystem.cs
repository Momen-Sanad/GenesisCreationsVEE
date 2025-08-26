using UnityEngine;

public class MoonTransitionSystem : MonoBehaviour
{
    public TransitionSystem transitionSystem;

    public GameObject earthParent;

    /// <summary>
    /// Triggered by MoonHandler when a full 28-day cycle is reached
    /// </summary>
    public void HandleMoonCycleComplete()
    {
        if (earthParent != null)
        {
            // Disable "Earth" MeshRenderer
            var earthChild = earthParent.transform.Find("Earth");
            if (earthChild != null)
            {
                var meshRenderer = earthChild.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    meshRenderer.enabled = false;
            }

            // Disable "Glow" ParticleSystem
            var glowChild = earthParent.transform.Find("Glow");
            if (glowChild != null)
            {
                var ps = glowChild.GetComponent<ParticleSystem>();
                if (ps != null)
                    ps.Stop(); // stops particles
                glowChild.gameObject.SetActive(false); // hides Glow object
            }
        }

        // Then do the transition
        if (transitionSystem != null)
        {
            transitionSystem.StartTransition();
        }
    }
}
