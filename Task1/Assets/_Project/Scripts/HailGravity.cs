using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class HailGravity : MonoBehaviour
{
    public Transform earth;       // Assign your Earth GameObject here
    public float gravityStrength = 9.81f;  // Tune this value for how strong the pull is

    public RotateEarth rotateEarth;
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (earth == null) return;

        int count = ps.particleCount;
        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        // Get all live particles
        ps.GetParticles(particles, count);

        Vector3 earthPos = earth.position;
        float dt = Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            // Direction from particle to Earth center
            Vector3 seasonOffset = earth.right * Mathf.Sin(rotateEarth.orbitRotator.GetCurrentAngle() * Mathf.Deg2Rad);
            Vector3 dir = ((earthPos - particles[i].position) + seasonOffset).normalized;


            // Apply acceleration toward Earth
            particles[i].velocity += dir * gravityStrength * dt;
        }

        // Push updated particles back to system
        ps.SetParticles(particles, count);
    }
}
