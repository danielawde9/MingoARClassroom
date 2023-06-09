using System.Collections.Generic;
using UnityEngine;
namespace MingoData.Scripts.ARUtil
{

    public class SpawnPrefabs : MonoBehaviour
    {
        public GameObject prefabToSpawn; // Assign the prefab to spawn in the Inspector
        public float spawnRadius = 5f; // Radius of the area in which the prefabs will be spawned
        public float despawnDistance = 5f; // Distance the user must move past the prefab to despawn it
        public int maxSpawnedPrefabs = 10; // Maximum number of prefabs to be spawned at a time

        private readonly List<GameObject> spawnedPrefabs = new List<GameObject>();
        private GameObject parentObject; // Parent object for all spawned prefabs

        void Start()
        {
            parentObject = new GameObject("spawned_prefabs");
        }

        void Update()
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
                float distanceFromUser = Vector3.Distance(transform.position, prefab.transform.position);

                if (!(distanceFromUser > spawnRadius + despawnDistance))
                    continue;
                spawnedPrefabs.RemoveAt(i);
                Destroy(prefab);
            }
        }
    }

}
