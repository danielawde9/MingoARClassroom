using UnityEngine;

public class ConstrainedParticleSystem : MonoBehaviour
{
    public int maxParticles = 10;
    private ParticleSystem particleSystem;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        int currentParticleCount = particleSystem.particleCount;

        if (currentParticleCount >= maxParticles)
        {
            ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
            emissionModule.enabled = false;
        }
        else
        {
            ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
            emissionModule.enabled = true;
        }
    }
}
