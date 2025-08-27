using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoonHandler : MonoBehaviour
{
    public MoonTransitionSystem moonTransitionSystem;

    public GameObject moonVisualPrefab;
    public Transform visualParent;

    public float stepDegrees = 45f;
    public bool stopAtFinalPhase = true;
    public DistanceGrabber grabberToDisable;

    public Sprite[] phaseSprites = new Sprite[8];

    // increased default size so world-space UI starts larger
    public Vector2 phaseImageSize = new Vector2(100f, 100f);

    // scale multiplier applied to RectTransform
    public float phaseImageScale = 1f;

    // world offset for labels/images under the moon
    public float labelYOffset = -230f;

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

        if (nextPhaseIndex == 0)
        {
            // spawn the first phase
            SpawnVisualPhase(realMoon, PhaseNames[0]);
            nextPhaseIndex++;
        }

        // forward only movement
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

    GameObject SpawnVisualPhase(Transform realMoon, string phaseName)
    {
        if (!moonVisualPrefab || !realMoon) return null;

        var clone = Instantiate(moonVisualPrefab, realMoon.position, realMoon.rotation, visualParent);
        clone.name = phaseName;

        // Make clone visual-only (not grabbable, no physics)
        var rb = clone.GetComponent<Rigidbody>();
        if (rb) Destroy(rb);

        var col = clone.GetComponent<Collider>();
        if (col) Destroy(col);

        // Find the Canvas attached to the clone and call PlanetToolTips
        var canvas = clone.GetComponentInChildren<Canvas>(true);
        if (canvas)
        {
            var toolTips = canvas.GetComponent<PlanetToolTips>();
            if (toolTips)
            {
                // Delay a few frames so Start() inside PlanetToolTips finishes first
                StartCoroutine(ActivateToolTipsNextFrame(toolTips));
            }
        }

        
        var tmpUI = clone.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmpUI != null)
            tmpUI.text = phaseName;

        // Attach sprite/image for this phase
        int phaseIndex = Array.IndexOf(PhaseNames, phaseName);
        AttachPhaseImageToCanvas(clone, phaseIndex);

        return clone;
    }

    /// <summary>
    /// Finds or creates an Image inside the clone's canvas, and sets a sprite for the supplied phaseIndex.
    /// If phaseIndex is out of range or no sprite assigned, image will be hidden.
    /// </summary>
    void AttachPhaseImageToCanvas(GameObject moonClone, int phaseIndex)
    {
        if (moonClone == null) return;

        // Try find canvas 
        var canvas = moonClone.GetComponentInChildren<Canvas>(true);
        if (canvas == null)
            return;
        

        Image image;
        
        // create image
        
        var imageGO = new GameObject("PhaseImage", typeof(RectTransform), typeof(Image));
        imageGO.transform.SetParent(canvas.transform, false);
        var rt = imageGO.GetComponent<RectTransform>();

        //make rect behave like when you set it with Rect tool
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // in a world-space canvas, sizeDelta uses world units
        rt.sizeDelta = phaseImageSize;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, phaseImageSize.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, phaseImageSize.y);

        rt.localPosition = new Vector3(0f, -Mathf.Abs(labelYOffset), 0f);
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * phaseImageScale;

        // rotate to face the main camera once (only at creation)
        if (Camera.main != null)
        {
            // forward from image to camera => image faces camera
            imageGO.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - imageGO.transform.position);
        }

        image = imageGO.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        
        
        // Safety
        if (image == null)
            return;

        // Set sprite if we have a valid index and sprite assigned
        if (phaseIndex >= 0 && phaseSprites != null && phaseIndex < phaseSprites.Length && phaseSprites[phaseIndex] != null)
        {
            image.sprite = phaseSprites[phaseIndex];
            image.enabled = true;
        }
        else
        {
            // hide if no sprite assigned
            image.enabled = false;
        }
    }

    IEnumerator ActivateToolTipsNextFrame(PlanetToolTips toolTips)
    {
        // wait 5 frames
        for (int i = 0; i < 5; i++)
            yield return null;

        Debug.Log("[MoonHandler] activated canvas (after 5 frames)");
        toolTips.gameObject.SetActive(true);
        toolTips.Show();
    }

    /// <summary>
    /// Utility: spawn an image on a moon or moon visual. Applies same anchors/size/scale as AttachPhaseImageToCanvas.
    /// </summary>
    public GameObject SpawnImageInCanvas(GameObject moonVisualOrRealMoon, Sprite sprite,
                                        Vector2 sizeWorld = default, Vector3 localOffset = default)
    {
        if (!moonVisualOrRealMoon || !sprite) 
            return null;

        if (sizeWorld == default(Vector2) || sizeWorld == Vector2.zero) sizeWorld = phaseImageSize;
        if (localOffset == default(Vector3)) localOffset = Vector3.zero;

        // try to find an existing Canvas under the provided object
        var canvas = moonVisualOrRealMoon.GetComponentInChildren<Canvas>(true);

        if (canvas == null)
        {
            var canvasGO = new GameObject("MoonCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvasGO.transform.SetParent(moonVisualOrRealMoon.transform, false);

            var canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1f, 1f);
            canvasRect.localPosition = Vector3.zero;
            canvasRect.localRotation = Quaternion.identity;
            canvasRect.localScale = Vector3.one;
        }

        // create the UI Image
        var imageGO = new GameObject("PhaseImage", typeof(RectTransform), typeof(Image));
        imageGO.transform.SetParent(canvas.transform, false);

        var rt = imageGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.sizeDelta = sizeWorld;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeWorld.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeWorld.y);

        rt.localPosition = new Vector3(0f, -labelYOffset, 0f) + localOffset;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * phaseImageScale;

        // face the main camera once when created
        if (Camera.main != null)
        {
            imageGO.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - imageGO.transform.position);
        }

        var img = imageGO.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        return imageGO;
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
