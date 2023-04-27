using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomOrbitalPoints : MonoBehaviour
{
    public MeshFilter orbitalMeshFilter;
    public int numberOfPoints = 100;
    public float atomSpawnDelay = 1.0f;
    public GameObject pointPrefab;
    public TextMeshProUGUI ballText;
    private Mesh _orbitalMesh;
    private List<GameObject> _points;

    private ObjectPool _pointPool;

    [SerializeField]
    private float _bias = 0.1f;


    void Start()
    {

        _orbitalMesh = orbitalMeshFilter.mesh;
        _points = new List<GameObject>();

        _pointPool = new ObjectPool(pointPrefab, numberOfPoints);

        StartCoroutine(InstantiateAndMovePoints());
    }



    IEnumerator InstantiateAndMovePoints()
    {
        WaitForSeconds delay = new WaitForSeconds(atomSpawnDelay);

        for (int i = 0; i < numberOfPoints; i++)
        {
            ballText.text = i.ToString();
            GameObject point = _pointPool.GetObject();
            point.transform.SetParent(transform);
            point.transform.localPosition = Vector3.zero;
            point.transform.localRotation = Quaternion.identity;

            Vector3 randomPoint = GetRandomPointInMesh(_orbitalMesh);
            point.transform.position = randomPoint;

            _points.Add(point);

            yield return delay;
        }

        WaitForSeconds movementDelay = new WaitForSeconds(0.5f);

        while (true)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere *  Time.deltaTime;
                Vector3 newPosition = _points[i].transform.position + randomDirection;

                if (_orbitalMesh.bounds.Contains(transform.InverseTransformPoint(newPosition)))
                {
                    _points[i].transform.position = newPosition;
                }
            }

            yield return movementDelay;
        }
    }
    private Vector3 GetRandomPointInMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;
        Vector3 v0 = vertices[triangles[randomTriangleIndex]];
        Vector3 v1 = vertices[triangles[randomTriangleIndex + 1]];
        Vector3 v2 = vertices[triangles[randomTriangleIndex + 2]];

        float r1 = Random.value;
        float r2 = Random.value;
        float sqrtR1 = Mathf.Sqrt(r1);

        float u = 1 - Mathf.Pow(1 - sqrtR1, 3);
        float v = r2 * Mathf.Pow(sqrtR1, 3);

        Vector3 pointInTriangle = (u * v0) + (v * v1) + ((1 - u - v) * v2);

        Vector3 center = mesh.bounds.center;
        pointInTriangle = Vector3.Lerp(pointInTriangle, center, _bias);

        return transform.TransformPoint(pointInTriangle);
    }


}
public class ObjectPool
{
    private GameObject prefab;
    private Queue<GameObject> pool;

    public ObjectPool(GameObject prefab, int initialSize)
    {
        this.prefab = prefab;
        pool = new Queue<GameObject>(initialSize);

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count == 0)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        GameObject pooledObject = pool.Dequeue();
        pooledObject.SetActive(true);

        return pooledObject;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
