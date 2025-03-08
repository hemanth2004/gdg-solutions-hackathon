using UnityEngine;

public class ConstantParticleCount : MonoBehaviour
{
    public ParticleSystem particleSystem;

    public int desiredParticleCount = 100;
    public float particleLifespan = 3.0f;
    public float updateInterval = 0.1f; 

    private float timeSinceLastUpdate = 0f;

    private void Start()
    {
        UpdateEmissionRate();
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdateEmissionRate();
            timeSinceLastUpdate = 0f;
        }
    }

    private void UpdateEmissionRate()
    {
        float currentParticleCount = particleSystem.particleCount;
        float emissionRate = desiredParticleCount / particleLifespan;

        var emission = particleSystem.emission;
        var rateOverTime = emission.rateOverTime;

        rateOverTime.constant = emissionRate;
        emission.rateOverTime = rateOverTime;
    }
}
