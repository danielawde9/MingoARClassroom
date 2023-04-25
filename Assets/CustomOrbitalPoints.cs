using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomOrbitalPoints : MonoBehaviour
{
    public MeshFilter orbitalMeshFilter;
    public int numberOfPoints = 100;
    public float movementSpeed = 1.0f;
    public GameObject pointPrefab;
    public TextMeshProUGUI ballText;
    private Mesh _orbitalMesh;
    private List<GameObject> _points;
    private Queue<GameObject> _objectPool;

    void Awake()
    {
        _objectPool = new Queue<GameObject>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            GameObject obj = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.SetActive(false);
            _objectPool.Enqueue(obj);
        }
    }

    void Start()
    {
        _orbitalMesh = orbitalMeshFilter.mesh;
        _points = new List<GameObject>();

        StartCoroutine(InstantiatePoints());
    }

    IEnumerator InstantiatePoints()
    {

        int maxCount = numberOfPoints; // set the max count to the value of numberOfPoints
        int count = 0; // initialize a count variable to keep track of the number of balls instantiated


        for (int i = 0; i < numberOfPoints; i++)
        {

            ballText.text = i.ToString();
            GameObject point = _objectPool.Dequeue();
            point.SetActive(true);

            Vector3 randomPoint = GetRandomPointInMesh(_orbitalMesh);
            point.transform.position = randomPoint;

            _points.Add(point);

            if (count == maxCount) // check if the count equals the max count
            {
                break; // break out of the loop
            }

            // Delay between instantiating points
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(MovePoints());
    }

    IEnumerator MovePoints()
    {
        WaitForSeconds delay = new WaitForSeconds(0.02f);

        while (true)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * movementSpeed * Time.deltaTime;
                Vector3 newPosition = _points[i].transform.position + randomDirection;

                if (IsPointInsideMesh(_orbitalMesh, newPosition))
                {
                    _points[i].transform.position = newPosition;
                }
            }

            yield return delay;
        }
    }

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

    //    float u = 1 - sqrtR1;
    //    float v = r2 * sqrtR1;

    //    Vector3 pointInTriangle = (u * v0) + (v * v1) + ((1 - u - v) * v2);
    //    return transform.TransformPoint(pointInTriangle);
    //}


    private Vector3 GetRandomPointInMesh(Mesh mesh)
    {
        Vector3 pointInMesh = Vector3.zero;
        bool pointIsInsideMesh = false;

        // Generate random points inside the mesh's bounding box until one is found that is inside the mesh
        while (!pointIsInsideMesh)
        {
            // Generate a random point inside the mesh's bounding box
            Vector3 randomPointInBounds = new Vector3(Random.Range(mesh.bounds.min.x, mesh.bounds.max.x),
                                                       Random.Range(mesh.bounds.min.y, mesh.bounds.max.y),
                                                       Random.Range(mesh.bounds.min.z, mesh.bounds.max.z));

            // Test if the point is inside the mesh
            if (IsPointInsideMesh(mesh, randomPointInBounds))
            {
                pointInMesh = randomPointInBounds;
                pointIsInsideMesh = true;
            }
        }

        return pointInMesh;
    }

    private bool IsPointInsideMesh(Mesh mesh, Vector3 point)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        int intersectCount = 0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = vertices[triangles[i]];
            Vector3 b = vertices[triangles[i + 1]];
            Vector3 c = vertices[triangles[i + 2]];

            if (RayIntersectsTriangle(point, Vector3.up, a, b, c))
            {
                intersectCount++;
            }
        }

        return intersectCount % 2 != 0;
    }

    private bool RayIntersectsTriangle(Vector3 origin, Vector3 direction, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 edge1 = b - a;
        Vector3 edge2 = c - a;
        Vector3 h = Vector3.Cross(direction, edge2);
        float det = Vector3.Dot(edge1, h);

        if (Mathf.Abs(det) < 0.0001f)
        {
            return false;
        }

        float invDet = 1.0f / det;
        Vector3 t = origin - a;
        float u = Vector3.Dot(t, h) * invDet;

        if (u < 0.0f || u > 1.0f)
        {
            return false;
        }
        Vector3 q = Vector3.Cross(t, edge1);
        float v = Vector3.Dot(direction, q) * invDet;

        if (v < 0.0f || u + v > 1.0f)
        {
            return false;
        }

        float tValue = Vector3.Dot(edge2, q) * invDet;

        return tValue > 0.0001f;
    }
}
