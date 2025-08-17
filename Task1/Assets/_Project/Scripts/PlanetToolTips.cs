using UnityEngine;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class PlanetToolTips : MonoBehaviour
{
    [Tooltip("World-space offset from the planet's world position")]
    public Vector3 offset = new Vector3(0f, 0.5f, 0f);

    public TextMeshProUGUI tmpText;

    // Saved once so the canvas keeps its initial world rotation
    public Quaternion initialRotation;

    void Start()
    {
        // ensure canvas is world-space
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.renderMode = RenderMode.WorldSpace;

        if (tmpText == null)
            tmpText = GetComponentInChildren<TextMeshProUGUI>(true);

        // capture initial world rotation so canvas doesn't spin with the planet
        initialRotation = transform.rotation;

        gameObject.SetActive(false);
    }


    /// <summary>
    /// Find parent planet, update position to follow planet + offset 
    /// then lock the canvas rotation to the initial world rotation.
    /// </summary>
    void LateUpdate()
    {
        Transform planet = transform.parent;
        if (planet == null) return;

        transform.position = planet.position + offset;

        transform.rotation = initialRotation;

        // billboard TMP child to face camera (only when active)
        if (tmpText != null && tmpText.gameObject.activeInHierarchy)
        {
            Camera camera = Camera.main;
            
            if (camera == null) 
                return;
            
            Vector3 Direction = tmpText.transform.position - camera.transform.position;

            if (Direction.sqrMagnitude > 0.000001f)
               tmpText.transform.rotation = Quaternion.LookRotation(Direction, camera.transform.up);
        }
    }


    // Find the PlanetToolTips on the planet's children at call time + sanity checks
    
    /// <summary>
    /// Static helper to hide a tooltip from a planet GameObject.
    /// </summary>
    public static void Hide(GameObject planet)
    {
        if (planet == null) return;
        var tooltip = planet.GetComponentInChildren<PlanetToolTips>(true);
        if (tooltip != null) tooltip.Hide();
    }
    public static void Show(GameObject planet)
    {
        if (planet == null) return;
        var tooltip = planet.GetComponentInChildren<PlanetToolTips>(true);
        if (tooltip != null) tooltip.Show();
    }

    // instance helpers
    // this enables: Show(myPlanet);   // no "tips." prefix needed
    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

}