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
        _orbitalMesh = Mesh.Instantiate(orbitalMeshFilter.mesh) as Mesh;
        _orbitalMesh.MarkDynamic();

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


    //private Vector3 GetRandomPointInMesh(Mesh mesh)
    //{

    //    Vector3 center = mesh.bounds.center;
    //    Vector3 size = mesh.bounds.size;

    //    Vector3 point;

    //    do
    //    {
    //        float x = Random.Range(-size.x / 2, size.x / 2);
    //        float y = Random.Range(-size.y / 2, size.y / 2);
    //        float z = Random.Range(-size.z / 2, size.z / 2);

    //        point = new Vector3(x, y, z);
    //        point = Vector3.Lerp(point, center, _bias);

    //    }

    //    //while (!IsPointInsideMesh(mesh, point));
    //    while (!_orbitalMesh.bounds.Contains(transform.InverseTransformPoint(point)));

    //    return transform.TransformPoint(point);
    //}


    //private bool IsPointInsideMesh(Mesh mesh, Vector3 point)
    //{
    //    int[] triangles = mesh.triangles;
    //    Vector3[] vertices = mesh.vertices;
    //    int intersectCount = 0;

    //    for (int i = 0; i < triangles.Length; i += 3)
    //    {
    //        Vector3 a = vertices[triangles[i]];
    //        Vector3 b = vertices[triangles[i + 1]];
    //        Vector3 c = vertices[triangles[i + 2]];

    //        if (RayIntersectsTriangle(point, Vector3.up, a, b, c))
    //        {
    //            intersectCount++;
    //        }
    //    }

    //    return intersectCount % 2 != 0;
    //}

    //private bool RayIntersectsTriangle(Vector3 origin, Vector3 direction, Vector3 a, Vector3 b, Vector3 c)
    //{
    //    Vector3 edge1 = b - a;
    //    Vector3 edge2 = c - a;
    //    Vector3 h = Vector3.Cross(direction, edge2);
    //    float det = Vector3.Dot(edge1, h);

    //    if (Mathf.Abs(det) < 0.0001f)
    //    {
    //        return false;
    //    }

    //    float invDet = 1.0f / det;
    //    Vector3 t = origin - a;
    //    float u = Vector3.Dot(t, h) * invDet;

    //    if (u < 0.0f || u > 1.0f)
    //    {
    //        return false;
    //    }
    //    Vector3 q = Vector3.Cross(t, edge1);
    //    float v = Vector3.Dot(direction, q) * invDet;

    //    if (v < 0.0f || u + v > 1.0f)
    //    {
    //        return false;
    //    }

    //    float tValue = Vector3.Dot(edge2, q) * invDet;

    //    return tValue > 0.0001f;
    //}



    //void InstantiatePoints()
    //{
    //    for (int i = 0; i < numberOfPoints; i++)
    //    {
    //        ballText.text = i.ToString();
    //        GameObject point = _pointPool.GetObject();
    //        point.transform.SetParent(transform);
    //        point.transform.localPosition = Vector3.zero;
    //        point.transform.localRotation = Quaternion.identity;

    //        Vector3 randomPoint = GetRandomPointInMesh(_orbitalMesh);
    //        point.transform.position = randomPoint;

    //        _points.Add(point);
    //    }
    //}

    //IEnumerator MovePoints()
    //{
    //    WaitForSeconds delay = new WaitForSeconds(0.1f);

    //    while (_points.Count < numberOfPoints)
    //    {
    //        for (int i = 0; i < _points.Count; i++)
    //        {
    //            Vector3 randomDirection = Random.insideUnitSphere * movementSpeed * Time.deltaTime;
    //            Vector3 newPosition = _points[i].transform.position + randomDirection;

    //            if (IsPointInsideMesh(_orbitalMesh, newPosition))
    //            {
    //                _points[i].transform.position = newPosition;
    //            }
    //        }

    //        yield return delay;
    //    }
    //}


    //private Vector3 GetRandomPointInMesh(Mesh mesh)
    //{
    //    Vector3[] vertices = mesh.vertices;
    //    int[] triangles = mesh.triangles;

    //    int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;
    //    Vector3 v0 = vertices[triangles[randomTriangleIndex]];
    //    Vector3 v1 = vertices[triangles[randomTriangleIndex + 1]];
    //    Vector3 v2 = vertices[triangles[randomTriangleIndex + 2]];

    //    float r1 = Random.value;
    //    float r2 = Random.value;
    //    float sqrtR1 = Mathf.Sqrt(r1);

    //    float u = 1 - Mathf.Pow(1 - sqrtR1, 3);
    //    float v = r2 * Mathf.Pow(sqrtR1, 3);

    //    Vector3 pointInTriangle = (u * v0) + (v * v1) + ((1 - u - v) * v2);
    //    return transform.TransformPoint(pointInTriangle);
    //}

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
