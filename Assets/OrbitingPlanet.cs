using UnityEngine;

public class OrbitingPlanet : MonoBehaviour
{
    public Transform sun; // Assign the Sun object in the Inspector
    public float speedFactor = 0.001f;
    public float orbitSpeed = 1.0f; // Speed of the orbit
    public float selfRotationSpeed = 1f; // Speed of self-rotation
    public float inclination; // In degrees
    public float semiMajorAxis; // In Unity units
    public float eccentricity;
    private  float distanceFromSun; // In Unity units

    private float angle; // Current angle of the planet in its orbit
    private LineRenderer trajectoryLine;
    private int completedOrbits = 0;

    public float sunVortexSpeed = 0.01f;
    public float sunVortexAngle = 60.0f;

    public int numTrajectoryPoints = 1000;
    private void Start()
    {
        angle = Random.Range(0, 360);
        SetupTrajectoryLineRenderer();
    }

    private void Update()
    {
        sun.position += new Vector3(Mathf.Cos(Mathf.Deg2Rad * sunVortexAngle) * sunVortexSpeed, Mathf.Sin(Mathf.Deg2Rad * sunVortexAngle) * sunVortexSpeed, 0) * Time.deltaTime;

        float previousAngle = angle;

        // Orbit
        angle += Time.deltaTime * orbitSpeed * speedFactor;
        distanceFromSun = semiMajorAxis;
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity);
        float radius = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * Mathf.Cos(angle));
        float x = sun.position.x + radius * Mathf.Cos(angle);
        float y = sun.position.y + Mathf.Sin(Mathf.Deg2Rad * inclination) * semiMinorAxis * Mathf.Sin(angle);
        float z = sun.position.z + radius * Mathf.Sin(angle) * Mathf.Cos(Mathf.Deg2Rad * inclination);
        transform.position = new Vector3(x, y, z);

        // Self-rotation
        transform.Rotate(Vector3.forward, selfRotationSpeed * Time.deltaTime);

        // Check if the planet has completed a full orbit
        if (previousAngle > angle)
        {
            completedOrbits++;
            Debug.Log(gameObject.name + " completed " + completedOrbits + " orbits.");
        }
    }
    public int GetCompletedOrbits()
    {
        return completedOrbits;
    }

    private void SetupTrajectoryLineRenderer()
    {
        trajectoryLine = gameObject.AddComponent<LineRenderer>();
        trajectoryLine.positionCount = numTrajectoryPoints;
        trajectoryLine.startWidth = 0.02f;
        trajectoryLine.endWidth = 0.02f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < numTrajectoryPoints; i++)
        {
            float t = i / (float)numTrajectoryPoints;
            float orbitAngle = t * 360f;
            float x = sun.position.x + distanceFromSun * (1 + eccentricity) * Mathf.Cos(orbitAngle) / (1 + eccentricity * Mathf.Cos(orbitAngle));
            float z = sun.position.z + distanceFromSun * Mathf.Sin(orbitAngle);
            float y = sun.position.y + Mathf.Sin(Mathf.Deg2Rad * inclination) * distanceFromSun * Mathf.Sin(orbitAngle);
            trajectoryLine.SetPosition(i, new Vector3(x, y, z));
        }
    }
}