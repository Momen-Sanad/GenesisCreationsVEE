using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    Transform sun;
    float radius;
    public float angularSpeed = 0.5f;
    float angle; 

    public void Initialize(Transform sunTransform, float r, float speed)
    {
        Debug.Log($"Initializing {gameObject.name} orbit with radius {radius}");

        sun = sunTransform;
        radius = r;
        angularSpeed = speed;

        // Compute initial angle from current position
        var dir = transform.position - sun.position;
        angle = Mathf.Atan2(dir.z, dir.x);
    }

    void Update()
    {
        if (sun == null) return;

        // Updates orbit position based on sun, radius, and angularSpeed
        angle += angularSpeed * Time.deltaTime;
        var pos = sun.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        transform.position = pos;
    }
}
