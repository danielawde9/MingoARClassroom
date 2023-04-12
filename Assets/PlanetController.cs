using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    public float orbitAcceleration = 1.0f;
    private int orbitCount;
    private int selfRotationCount;
    private float speedFactor;
    //private float orbitSpeed;
    private float maxOrbitSpeed;

    private float selfRotationSpeed;
    private float inclination;
    private float semiMajorAxis;
    private float eccentricity;
    private float distanceFromSun;
    private float orbitAngle;
    private Transform sun;
    private LineRenderer orbitLine;
    public float orbitLineUpdateInterval = 0.5f;
    private float orbitLineUpdateTime;
    private void Start()
    {
        sun = transform.parent;

        InitializeOrbitLine();
    }

    private void Update()
    {
        RotateAroundSun();
        RotateAroundAxis();

        // Update the orbit line at a specific interval
        if (Time.time >= orbitLineUpdateTime)
        {
            UpdateOrbitLine();
            orbitLineUpdateTime = Time.time + orbitLineUpdateInterval;
        }
    }

    private void InitializeOrbitLine()
    {
        orbitLine = gameObject.AddComponent<LineRenderer>();
        orbitLine.useWorldSpace = true;
        orbitLine.loop = true;
        orbitLine.startWidth = 0.01f;
        orbitLine.endWidth = 0.01f;
        orbitLine.positionCount = 360;
    }
    private void UpdateOrbitLine()
    {
        for (int i = 0; i < 360; i++)
        {
            float angle = i * Mathf.Deg2Rad;

            float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity);
            float radius = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * Mathf.Cos(angle));
            float x = sun.position.x + radius * Mathf.Cos(angle);
            float y = sun.position.y + Mathf.Sin(Mathf.Deg2Rad * inclination) * semiMinorAxis * Mathf.Sin(angle);
            float z = sun.position.z + radius * Mathf.Sin(angle) * Mathf.Cos(Mathf.Deg2Rad * inclination);

            Vector3 pointPosition = new Vector3(x, y, z);
            orbitLine.SetPosition(i, pointPosition);
        }
    }

    public void SetData(SolarSystemController.CelestialBody data)
    {
        speedFactor = data.speedFactor;
        maxOrbitSpeed = data.maxOrbitSpeed;
        selfRotationSpeed = data.selfRotationSpeed;
        inclination = data.inclination;
        semiMajorAxis = data.semiMajorAxis;
        eccentricity = data.eccentricity;
        distanceFromSun = data.distanceFromSun;
    }

    private void RotateAroundSun()
    {
        float previousAngle = orbitAngle;
        float angleInRadians = orbitAngle * Mathf.Deg2Rad;

        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity);
        float radius = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * Mathf.Cos(angleInRadians));
        float currentOrbitSpeed = maxOrbitSpeed * Mathf.Sqrt((semiMajorAxis * (1 - eccentricity)) / radius);

        orbitAngle += Time.deltaTime * currentOrbitSpeed * speedFactor * orbitAcceleration;

        float x = sun.position.x + radius * Mathf.Cos(angleInRadians);
        float y = sun.position.y + Mathf.Sin(Mathf.Deg2Rad * inclination) * semiMinorAxis * Mathf.Sin(angleInRadians);
        float z = sun.position.z + radius * Mathf.Sin(angleInRadians) * Mathf.Cos(Mathf.Deg2Rad * inclination);
        transform.position = new Vector3(x, y, z);

        if (previousAngle < 360 && orbitAngle >= 360)
        {
            orbitCount++;
            Debug.Log($"{gameObject.name} has orbited {orbitCount} times.");
        }

    }


    private void RotateAroundAxis()
    {
        float previousRotation = transform.eulerAngles.y;
        transform.Rotate(Vector3.up, Time.deltaTime * selfRotationSpeed * speedFactor * orbitAcceleration);

        if (previousRotation < 360 && transform.eulerAngles.y >= 360)
        {
            selfRotationCount++;
            Debug.Log($"{gameObject.name} has rotated around its axis {selfRotationCount} times.");
        }
    }

    private void CreateOrbitLine()
    {
        orbitLine = gameObject.AddComponent<LineRenderer>();
        orbitLine.useWorldSpace = true;
        orbitLine.loop = true;
        orbitLine.startWidth = 0.01f;
        orbitLine.endWidth = 0.01f;
        orbitLine.positionCount = 360;

        for (int i = 0; i < 360; i++)
        {
            float angle = i * Mathf.Deg2Rad;
            float xPos = sun.position.x + distanceFromSun * (1 - eccentricity) * Mathf.Cos(angle);
            float zPos = sun.position.z + distanceFromSun * Mathf.Sin(angle);
            Vector3 pointPosition = new Vector3(xPos, sun.position.y + inclination, zPos);
            orbitLine.SetPosition(i, pointPosition);
        }
    }
}
