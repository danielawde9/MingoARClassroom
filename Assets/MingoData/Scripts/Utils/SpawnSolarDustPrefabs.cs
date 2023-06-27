using System.Collections.Generic;
using UnityEngine;

namespace MingoData.Scripts.Utils
{

    public class SpawnSolarDustPrefabs : MonoBehaviour
    {
        public GameObject prefabToSpawn; // Assign the prefab to spawn in the Inspector
        public float spawnRadius = 1f; // Radius of the area in which the prefabs will be spawned
        public float deSpawnDistance = 1f; // Distance the user must move past the prefab to de spawn it
        public int maxSpawnedPrefabs = 100; // Maximum number of prefabs to be spawned at a time

        private readonly List<GameObject> spawnedPrefabs = new List<GameObject>();
        private GameObject parentObject; // Parent object for all spawned prefabs

        private void Start()
        {
            parentObject = new GameObject("Spawned Solar Dust Prefabs");
        }

        private void Update()
        {
            if (spawnedPrefabs.Count < maxSpawnedPrefabs)
            {
                Vector3 spawnDirection = Random.onUnitSphere;
                Vector3 spawnPosition = transform.position + spawnDirection * spawnRadius;
                GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, parentObject.transform);
                spawnedPrefabs.Add(spawnedPrefab);
            }

            for (int i = spawnedPrefabs.Count - 1; i >= 0; i--)
            {
                GameObject prefab = spawnedPrefabs[i];
                prefab.name = "Solar Dust" + i;

                float distanceFromUser = Vector3.Distance(transform.position, prefab.transform.position);

                if (!(distanceFromUser > spawnRadius + deSpawnDistance))
                    continue;
                spawnedPrefabs.RemoveAt(i);
                Destroy(prefab);
            }
        }
    }
}
