using System.Collections.Generic;
using UnityEngine;

namespace MingoData.Scripts.Utils
{

    public class SpawnSolarDustPrefabs : MonoBehaviour
    {
        public GameObject prefabToSpawn; // Assign the prefab to spawn in the Inspector

        private readonly List<GameObject> spawnedPrefabs = new List<GameObject>();
        private GameObject parentObject; // Parent object for all spawned prefabs

        private void Start()
        {
            parentObject = new GameObject("Spawned Solar Dust Prefabs");
        }

        private void Update()
        {
            if (spawnedPrefabs.Count < Constants.SolarDustMaxSpawnedPrefabs)
            {
                Vector3 spawnDirection = Random.onUnitSphere;
                Vector3 spawnPosition = transform.position + spawnDirection * Constants.SolarDustSpawnRadius;
                GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, parentObject.transform);
                spawnedPrefabs.Add(spawnedPrefab);
            }

            for (int i = spawnedPrefabs.Count - 1; i >= 0; i--)
            {
                GameObject prefab = spawnedPrefabs[i];

                float distanceFromUser = Vector3.Distance(transform.position, prefab.transform.position);

                if (!(distanceFromUser > Constants.SolarDustSpawnRadius + Constants.SolarDustDeSpawnDistance))
                    continue;
                spawnedPrefabs.RemoveAt(i);
                Destroy(prefab);
            }
        }
    }
}
