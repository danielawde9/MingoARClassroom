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
        while (pool.Count > 0)
        {
            Vector3 spawnDirection = Random.onUnitSphere;
            Vector3 spawnPosition = transform.position + spawnDirection * spawnRadius;
            GameObject spawnedPrefab = pool.Dequeue();
            spawnedPrefab.transform.position = spawnPosition;
            spawnedPrefab.transform.rotation = Quaternion.identity;
            spawnedPrefab.SetActive(true);
        }

        foreach (GameObject prefab in pool)
        {
            float distanceFromUser = Vector3.Distance(transform.position, prefab.transform.position);

            if (distanceFromUser > spawnRadius + despawnDistance)
            {
                prefab.SetActive(false);
                pool.Enqueue(prefab);
            }
        }
    }
}
