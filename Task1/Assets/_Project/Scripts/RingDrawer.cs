using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingDrawer : MonoBehaviour
{
    [Header("Reference")]
    public Transform targetBody;

    [Header("Ring Settings")]
    public float referenceRadius = 1f;
    public int segments = 100;

    [Tooltip("Margain of error for user")]
    public float margin = 0.5f;

    public Color correctColor = Color.blue;
    public Color defaultColor = Color.red;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;

        lineRenderer.positionCount = segments;
        lineRenderer.widthMultiplier = 0.02f;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.startColor = defaultColor;
        lineRenderer.endColor = defaultColor;

        DrawCircle();
    }

    void Update()
    {
        if (targetBody == null) return;

        float currentRadius = targetBody.localScale.x;

        if (Mathf.Abs(currentRadius - referenceRadius) <= margin)
        {
            lineRenderer.startColor = correctColor;
            lineRenderer.endColor = correctColor;
        }
        else
        {
            lineRenderer.startColor = defaultColor;
            lineRenderer.endColor = defaultColor;
        }

        // Optionally, keep position synced
        //transform.position = targetBody.position;
        //transform.rotation = Quaternion.identity;
        //transform.localScale = Vector3.one;
    }

    void DrawCircle()
    {
        Vector3[] points = new Vector3[segments];

        float angleStep = 360f / segments;
        Vector3 center = transform.position;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;

            float x = Mathf.Cos(angle) * referenceRadius;
            float z = Mathf.Sin(angle) * referenceRadius;
            float y = 0f;

            points[i] = center + new Vector3(x, y, z);

        }

        lineRenderer.SetPositions(points);
    }
}