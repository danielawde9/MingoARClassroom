using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManagerScript : MonoBehaviour
{
    public GameObject particlePrefab;
    public int particleCount = 100;
    public float spawnRadius = 1000f;

    private void Start()
    {
        SpawnParticles();
    }

    private void SpawnParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 randomPosition = Random.insideUnitSphere * spawnRadius;
            Instantiate(particlePrefab, randomPosition, Quaternion.identity);
        }
    }
}
