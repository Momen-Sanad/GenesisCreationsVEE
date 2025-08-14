using UnityEngine;
using TMPro;

// billboarding, scriptableobjects, tips on the planets, make quiz manager using scriptable object.
[System.Serializable]
public class PlanetTooltip
{
    public GameObject planet;       // The planet in the scene
    public GameObject tooltipUI;    // The Tooltip UI GameObject (with TextMeshProUGUI inside)
    public Vector3 offset = new Vector3(2, 2f, 0); // Offset above the planet
}

public class PlanetToolTips : MonoBehaviour
{
    public PlanetTooltip[] planetTooltips; // Array for all planets and tooltips
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        HideAllTooltips();

        // Make sure tooltips are in world space
        foreach (var pt in planetTooltips)
        {
            Canvas canvas = pt.tooltipUI.GetComponentInParent<Canvas>();
            if (canvas != null)
                canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    void LateUpdate()
    {
        // Keep active tooltips positioned & billboarded
        foreach (var pt in planetTooltips)
        {
            if (pt.tooltipUI.activeSelf)
            {
                // Position tooltip above planet
                pt.tooltipUI.transform.position = pt.planet.transform.position + pt.offset;

                // Billboard: face camera
                pt.tooltipUI.transform.LookAt(
                    pt.tooltipUI.transform.position + mainCam.transform.forward,
                    mainCam.transform.up
                );
            }
        }
    }

    public void ShowTooltipFor(GameObject planet)
    {
        Debug.Log(planet + " About to show");

        //HideAllTooltips();

        foreach (var pt in planetTooltips)
        {
            if (pt.planet == planet)
            {
                Debug.Log(planet + " Shown");
                pt.tooltipUI.SetActive(true);
                break;
            }
        }
    }

    void HideAllTooltips()
    {
        foreach (var pt in planetTooltips)
        {
            pt.tooltipUI.SetActive(false);
        }
    }
}
