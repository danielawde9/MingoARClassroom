using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabs : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public float spawnRadius = 5f;
    public float despawnDistance = 5f;
    public int maxSpawnedPrefabs = 10;

    private readonly Queue<GameObject> pool = new();
    private GameObject parentObject;

    void Start()
    {
        parentObject = new GameObject("spawned_stardust_prefabs");
        for (int i = 0; i < maxSpawnedPrefabs; i++)
        {
            GameObject prefab = Instantiate(prefabToSpawn, parentObject.transform);
            prefab.SetActive(false);
            pool.Enqueue(prefab);
        }
    }

    void Update()
    {
        // Dequeue and repurpose objects that are too far behind the player
        foreach (GameObject prefab in new List<GameObject>(pool))
        {
            if (Vector3.Distance(transform.position, prefab.transform.position) > spawnRadius + despawnDistance)
            {
                pool.Dequeue();
                prefab.SetActive(false);
                pool.Enqueue(prefab);
            }
        }

        // If there are available prefabs in the pool, spawn one in front of the player
        if (pool.Count > 0)
        {
            Vector3 spawnDirection = transform.forward + Random.insideUnitSphere.normalized * spawnRadius;
            Vector3 spawnPosition = transform.position + spawnDirection;
            GameObject spawnedPrefab = pool.Dequeue();
            spawnedPrefab.transform.position = spawnPosition;
            spawnedPrefab.transform.rotation = Quaternion.identity;
            spawnedPrefab.SetActive(true);
        }
    }
}
