using UnityEngine;
using TMPro;

public class RotationManager : MonoBehaviour
{
    [Header("References")]
    public EarthRotator axisRotator;
    public EarthOrbitRotator orbitRotator;

    [Header("UI")]
    public TMP_Text dayCounterText;

    bool isOrbiting = false;
    bool isLocked = false;

    void Start()
    {
        // Hide day counter at the start
        if (dayCounterText != null)
            dayCounterText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isLocked) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isOrbiting)
            {
                // stop orbit mode and return to manual rotation (if not locked)
                orbitRotator.StopOrbit();
                axisRotator.enabled = true;
                isOrbiting = false;

                // hide UI because user left orbit mode
                if (dayCounterText != null)
                    dayCounterText.gameObject.SetActive(false);
            }
            else
            {
                // start orbiting:
                // disable axis rotation and enable orbit rotator
                axisRotator.enabled = false;

                // StartOrbit now registers callbacks for day update and orbit completion
                orbitRotator.StartOrbit(OnOrbitComplete, OnDayChanged);
                isOrbiting = true;

                // show UI while orbiting
                if (dayCounterText != null)
                {
                    dayCounterText.gameObject.SetActive(true);
                    dayCounterText.text = $"Day: {orbitRotator.GetCurrentDay()}";
                }
            }
        }
    }

    // Called when Earth completes orbit (365 days)
    void OnOrbitComplete()
    {
        // lock out further interactions
        axisRotator.enabled = false;
        isLocked = true;

        // ensure final day value displayed
        if (dayCounterText != null)
            dayCounterText.text = $"Day: {orbitRotator.GetCurrentDay()}";
    }

    // Called each time day increases
    void OnDayChanged(int dayCount)
    {
        if (dayCounterText != null)
            dayCounterText.text = $"Day: {dayCount}";
    }
}
