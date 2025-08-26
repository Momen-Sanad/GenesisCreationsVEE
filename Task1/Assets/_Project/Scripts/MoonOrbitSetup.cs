using UnityEngine;
using TMPro; 


[DisallowMultipleComponent]
public class MoonOrbitSetup : MonoBehaviour
{
    public MoonHandler moonHandler;
    public MoonGrabber moonGrabber;

    public OrbitManager orbitManager;
    public int orbitIndex = 0;
    public float angularSpeed = 0.5f;

    [Header("Starting Angle Control")]
    [Tooltip("Leave -1 to use Moon's current position")]
    public float startAngleDegrees = -1f;

    [Header("Moon Cycle")]
    public TextMeshProUGUI dayCounterText;
    public int maxDays = 28;

    float totalAngleTravelled; // how far the moon has moved
    int currentDay = 0;
    bool cycleCompleted = false;

    PlanetOrbit orbit;
    PlanetToolTips toolTips;


    void Start()
    {
        if (moonGrabber)
            moonGrabber.enabled = false;

        if (moonHandler)
            moonHandler.enabled = false;

        if (!orbitManager)
            orbitManager = FindFirstObjectByType<OrbitManager>();

        if (!orbitManager|| !orbitManager.sun)
            return;

        if (orbitIndex < 0 || orbitIndex >= orbitManager.orbitRadii.Length)
            return;

        var radius = orbitManager.orbitRadii[orbitIndex];

        // set initial moon position
        if (startAngleDegrees >= 0f)
        {
            var rad = startAngleDegrees * Mathf.Deg2Rad;
            transform.position = orbitManager.sun.position +
                                 new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        }
        else
        {
            Debug.Log("[MoonOrbitSetup] Using Moon's scene position as start point.");
        }

        orbit = GetComponent<PlanetOrbit>();
        if (!orbit)
            orbit = gameObject.AddComponent<PlanetOrbit>();

        orbit.Initialize(orbitManager.sun, radius, angularSpeed);

        UpdateDayCounter();

        toolTips = GetComponentInChildren<PlanetToolTips>(true);

        if (toolTips)
        {
            toolTips.Show();
            UpdateDayCounter();
        }
    }

    void Update()
    {
        if (orbit == null || orbitManager == null || orbitManager.sun == null) return;

        if (toolTips && !toolTips.isActiveAndEnabled)
        {
            Debug.Log("hi");
            toolTips.Show();
            toolTips.gameObject.SetActive(true);
        }

        // use the actual orbit speed to stay in sync
        float omega;
        if (orbit != null)
            omega = orbit.angularSpeed;
        else
            omega = angularSpeed;
        
        totalAngleTravelled += omega * Time.deltaTime;

        // clamp so we end exactly at 2pi
        if (totalAngleTravelled > 2 * Mathf.PI)
            totalAngleTravelled = 2 * Mathf.PI;

        // fraction of full orbit
        var fraction = totalAngleTravelled / (2 * Mathf.PI);

        // map to days
        var newDay = Mathf.Min(maxDays, Mathf.FloorToInt(fraction * maxDays));

        if (newDay != currentDay)
        {
            currentDay = newDay;
            UpdateDayCounter();
        }

        // stop after one full cycle (or when days hit max), fire handler once
        if (!cycleCompleted && (totalAngleTravelled >= 2 * Mathf.PI || currentDay >= maxDays))
        {
            cycleCompleted = true;

            // ensure the UI shows 28 exactly
            currentDay = maxDays;
            UpdateDayCounter();

            toolTips.Hide();
            
            moonGrabber.enabled = true;

            // null-safe invoke
            moonHandler?.OnFullCycleComplete();

            // finally stop motion
            orbit.enabled = false;
            enabled = false;
        }

    }

    void UpdateDayCounter()
    {
        if (toolTips && toolTips.tmpText)
            toolTips.tmpText.text = $"{currentDay} days";

        else if (dayCounterText)
            dayCounterText.text = $"{currentDay} days";
    }
}
