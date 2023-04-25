using UnityEngine;

public class CustomOrbitalParticleEmitter : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public MeshFilter orbitalMeshFilter;
    public int numberOfParticles = 100;
    public float movementSpeed = 0.1f;

    private ParticleSystem.Particle[] particles;

    private void Start()
    {
        particles = new ParticleSystem.Particle[numberOfParticles];
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        Bounds orbitalBounds = orbitalMeshFilter.mesh.bounds;

        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector3 randomPosition = GetRandomPointInBounds(orbitalBounds);
            emitParams.position = randomPosition;
            particleSystem.Emit(emitParams, 1);
        }

        particleSystem.GetParticles(particles);
    }

    private void Update()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            Vector3 direction = (particles[i].position - transform.position).normalized;
            Vector3 newPosition = particles[i].position + (direction * movementSpeed * Time.deltaTime);
            particles[i].position = newPosition;
        }

        particleSystem.SetParticles(particles, particles.Length);
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        Vector3 randomPoint;

        do
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            randomPoint = new Vector3(x, y, z);
        } while (!IsPointInsideMesh(orbitalMeshFilter.mesh, randomPoint));

        return randomPoint;
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

