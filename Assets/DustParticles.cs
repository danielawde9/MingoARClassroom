using UnityEngine;

public class DustParticleController : MonoBehaviour
{
    public ParticleSystem dustParticles;
    public float minDistanceToSpawn = 0.5f;

    private Vector3 lastSpawnPosition;
    private ParticleSystem.ForceOverLifetimeModule forceOverLifetime;

    private void Start()
    {
        lastSpawnPosition = transform.position;
        forceOverLifetime = dustParticles.forceOverLifetime;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        float distanceMoved = Vector3.Distance(currentPosition, lastSpawnPosition);

        if (distanceMoved >= minDistanceToSpawn)
        {
            dustParticles.Emit(1);
            lastSpawnPosition = currentPosition;
        }

        // Calculate the force to be applied to the particles
        Vector3 force = (lastSpawnPosition - currentPosition) * dustParticles.main.startSpeedMultiplier;
        forceOverLifetime.enabled = true;
        forceOverLifetime.x = new ParticleSystem.MinMaxCurve(force.x);
        forceOverLifetime.y = new ParticleSystem.MinMaxCurve(force.y);
        forceOverLifetime.z = new ParticleSystem.MinMaxCurve(force.z);
    }
}
