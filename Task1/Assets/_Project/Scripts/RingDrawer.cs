using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingDrawer : MonoBehaviour
{
    [Header("Reference")]
    public Transform targetBody;

    [Header("Ring Settings")]
    public float targetHeight = 0.55f; // world units from planet center
    public int segments = 100;
    public float ringRadius = 0.5f;    // size of the visual hoop

    [Tooltip("Margain of error for user")]
    public float margin = 0.01f;

    public Color correctColor = Color.blue;
    public Color defaultColor = Color.red;

    LineRenderer lineRenderer;

    public bool IsScaledCorrectly { private set; get; }

    
    public float requiredHoldTime = 0.1f;

     float correctTimer = 0f;

    const string MaterialName = "Sprites/Default";

    void Start() {

        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;

        lineRenderer.positionCount = segments;
        lineRenderer.widthMultiplier = 0.02f;

        lineRenderer.material = new Material(Shader.Find(MaterialName));

        lineRenderer.startColor = defaultColor;
        lineRenderer.endColor = defaultColor;

        DrawCircle();
    }

    void Update() => CheckRingAlignment();

     void CheckRingAlignment() {

        if (targetBody == null) return;

        // Calculate the planet's top position in world space
        var planetTopY = targetBody.position.y + (targetBody.localScale.y * 0.5f);

        // Calculate where the ring should be (center of planet + reference height)
        var targetRingY = targetBody.position.y + targetHeight;

        // If ring is visually within the allowed margin right now (bool)
        var currentlyCorrect = Mathf.Abs(planetTopY - targetRingY) <= margin;

        if (currentlyCorrect) {

            correctTimer += Time.deltaTime;
            SetRingColor(correctColor);

            // Only set the bool when we've been correct continuously for requiredHoldTime
            if (!IsScaledCorrectly && correctTimer >= requiredHoldTime)            
                IsScaledCorrectly = true;
        }

        else {
            correctTimer = 0f;
            IsScaledCorrectly = false;
            SetRingColor(defaultColor);
        }
    }

    /// <summary>
    /// Draws the ring as a circle around the target body using the LineRenderer.
    /// </summary>
    void DrawCircle() {

        lineRenderer.positionCount = segments;

        Vector3[] points = new Vector3[segments];
        var angleStep = 360f / segments;

        // World-space center where the ring center sits
        var center = targetBody.position + new Vector3(0f, targetHeight - ringRadius, 0f);

        for (var i = 0; i < segments; i++) {

            var angle = Mathf.Deg2Rad * i * angleStep;
            var y = Mathf.Sin(angle) * ringRadius;
            var z = Mathf.Cos(angle) * ringRadius;
            points[i] = center + new Vector3(0f, y, z);
        }

        lineRenderer.SetPositions(points);
        IsScaledCorrectly = false;
    }

    /// <summary>
    /// Sets the radius and height of the ring, then redraws it.
    /// </summary>
    public void SetRingRadius(float newRadius) {
        ringRadius = newRadius;
        targetHeight = newRadius;
        IsScaledCorrectly = false;
        DrawCircle();
    }

    /// <summary>
    /// Updates the LineRenderer color for both start and end points.
    /// </summary>
    void SetRingColor(Color color) {

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    /// <summary>
    /// Clears all line segments from this ring's LineRenderer.
    /// </summary>
    public void ClearRing()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
    }
}