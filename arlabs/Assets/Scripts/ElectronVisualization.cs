using UnityEngine;

public class ElectronVisualization : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public ParticleSystem particleSystem;
    public float moveSpeed = 1.0f;

    private float distanceTravelled = 0f;
    private float totalDistance = 0f;
    private ParticleSystem.Particle[] particles;

    private void Start()
    {
        Vector3[] linePositions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(linePositions);

        for (int i = 0; i < linePositions.Length - 1; i++)
        {
            totalDistance += Vector3.Distance(linePositions[i], linePositions[i + 1]);
        }

        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    private void Update()
    {
        int particleCount = particleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            distanceTravelled += moveSpeed * Time.deltaTime;

            if (distanceTravelled >= totalDistance)
            {
                distanceTravelled = distanceTravelled % totalDistance;
            }

            Vector3 targetPosition = CalculatePositionAlongLine(distanceTravelled);
            particles[i].position = targetPosition;
        }

        particleSystem.SetParticles(particles, particleCount);
    }

    private Vector3 CalculatePositionAlongLine(float distance)
    {
        Vector3[] linePositions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(linePositions);

        float currentDistance = 0f;

        for (int i = 0; i < linePositions.Length - 1; i++)
        {
            float segmentDistance = Vector3.Distance(linePositions[i], linePositions[i + 1]);

            if (distance >= currentDistance && distance <= currentDistance + segmentDistance)
            {
                float segmentT = (distance - currentDistance) / segmentDistance;
                return Vector3.Lerp(linePositions[i], linePositions[i + 1], segmentT);
            }

            currentDistance += segmentDistance;
        }

        return linePositions[linePositions.Length - 1];
    }
}
