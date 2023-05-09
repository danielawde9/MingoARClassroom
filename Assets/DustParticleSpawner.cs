using UnityEngine;

public class DustParticleSpawner : MonoBehaviour
{
    public ParticleSystem dustParticleSystem;
    public Transform solarSystemCenter;
    public float spawnRadius = 10f;
    public int maxParticles = 100;

    private void Start()
    {
        // Configure the dust particle system
        var mainModule = dustParticleSystem.main;
        mainModule.maxParticles = maxParticles;
        mainModule.loop = true;
        mainModule.playOnAwake = true;

        var emissionModule = dustParticleSystem.emission;
        emissionModule.rateOverTime = maxParticles / mainModule.startLifetime.constant;

        var shapeModule = dustParticleSystem.shape;
        shapeModule.shapeType = ParticleSystemShapeType.SphereShell;
        shapeModule.radius = spawnRadius;

        // Set the particle system's position
        dustParticleSystem.transform.position = solarSystemCenter.position;

        // Start the particle system
        dustParticleSystem.Play();
    }
}
