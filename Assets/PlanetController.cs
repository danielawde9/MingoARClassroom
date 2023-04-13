using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PlanetController : MonoBehaviour
{
    private float orbitAcceleration = 1.0f;
    private int orbitCount;
    private int selfRotationCount;
    private float maxOrbitSpeed;

    private float selfRotationSpeed;
    private float axialTilt;
    private float inclination;
    private float semiMajorAxis;
    private float eccentricity;
    private float orbitAngle;
    private float longitudeOfAscendingNode;
    private float argumentOfPerihelion;
    private float meanAnomalyAtEpoch;
    private LineRenderer orbitLine;
    public float orbitLineUpdateInterval = 0.5f;
    private float orbitLineUpdateTime;
    private Transform centralBody; 
    private float totalOrbitAngle = 0f;
    private float totalRotationAngle = 0f;

    public TextMeshProUGUI debugText;


    private void Start()
    {
        Transform parent = transform.parent;
        if (parent != null && parent.GetComponent<PlanetController>() != null)
        {
            centralBody = parent;
        }
        else
        {
            centralBody = transform.root;
        }

        InitializeOrbitLine();
    }

    private void Update()
    {
        RotateAroundSun();
        RotateAroundAxis();

        UpdateDebugText();
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
            float trueAnomaly = angle;
            float angleInRadians = trueAnomaly + argumentOfPerihelion * Mathf.Deg2Rad;

            float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity);
            float radius = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * Mathf.Cos(angleInRadians));
            float x = centralBody.position.x + radius * Mathf.Cos(angleInRadians);
            float y = centralBody.position.y + Mathf.Sin(Mathf.Deg2Rad * inclination) * semiMinorAxis * Mathf.Sin(angleInRadians);
            float z = centralBody.position.z + radius * Mathf.Sin(angleInRadians) * Mathf.Cos(Mathf.Deg2Rad * inclination);

            float adjustedX = x * Mathf.Cos(longitudeOfAscendingNode * Mathf.Deg2Rad) - z * Mathf.Sin(longitudeOfAscendingNode * Mathf.Deg2Rad);
            float adjustedZ = x * Mathf.Sin(longitudeOfAscendingNode * Mathf.Deg2Rad) + z * Mathf.Cos(longitudeOfAscendingNode * Mathf.Deg2Rad);

            Vector3 pointPosition = new(adjustedX, y, adjustedZ);
            orbitLine.SetPosition(i, pointPosition);
        }
    }

    public void OrbitAccelerationMultiplier(float multiplier)
    {
        orbitAcceleration = multiplier;
    }

    public void SetData(SolarSystemController.CelestialBody data)
    {
        maxOrbitSpeed = data.maxOrbitSpeed;
        selfRotationSpeed = data.selfRotationSpeed;
        axialTilt = data.axialTilt;
        inclination = data.inclination;
        semiMajorAxis = data.semiMajorAxis;
        eccentricity = data.eccentricity;
        longitudeOfAscendingNode = data.longitudeOfAscendingNode;
        argumentOfPerihelion = data.argumentOfPerihelion;
        meanAnomalyAtEpoch = data.meanAnomalyAtEpoch;
    }

    private void RotateAroundSun()
    {
        //float angleInRadians = orbitAngle * Mathf.Deg2Rad;
        meanAnomalyAtEpoch += Time.deltaTime * maxOrbitSpeed * orbitAcceleration;

        float trueAnomaly = TrueAnomaly(meanAnomalyAtEpoch, eccentricity);
        float angleInRadians = trueAnomaly + argumentOfPerihelion * Mathf.Deg2Rad;


        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity);
        float radius = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * Mathf.Cos(angleInRadians));
        float currentOrbitSpeed = maxOrbitSpeed * Mathf.Sqrt((semiMajorAxis * (1 - eccentricity)) / radius);

        totalOrbitAngle += Time.deltaTime * currentOrbitSpeed * orbitAcceleration;

        if (totalOrbitAngle >= 360f)
        {
            orbitCount++;
            Debug.Log($"{gameObject.name} has orbited {orbitCount} times.");
            totalOrbitAngle -= 360f;
        }

        float x = centralBody.position.x + radius * Mathf.Cos(angleInRadians);
        float y = centralBody.position.y + Mathf.Sin(Mathf.Deg2Rad * inclination) * semiMinorAxis * Mathf.Sin(angleInRadians);
        float z = centralBody.position.z + radius * Mathf.Sin(angleInRadians) * Mathf.Cos(Mathf.Deg2Rad * inclination);

        float adjustedX = x * Mathf.Cos(longitudeOfAscendingNode * Mathf.Deg2Rad) - z * Mathf.Sin(longitudeOfAscendingNode * Mathf.Deg2Rad);
        float adjustedZ = x * Mathf.Sin(longitudeOfAscendingNode * Mathf.Deg2Rad) + z * Mathf.Cos(longitudeOfAscendingNode * Mathf.Deg2Rad);

        transform.position = new Vector3(adjustedX, y, adjustedZ);

        //Debug.Log($"{gameObject.name} Rotation Speed: {maxOrbitSpeed} degrees per day");


    }


    private void UpdateDebugText()
    {
        if (gameObject.name == "Earth")
        {

        float currentRotationSpeed = selfRotationSpeed * orbitAcceleration;
        float currentOrbitSpeed = maxOrbitSpeed * orbitAcceleration;
        debugText.text =
            $"{gameObject.name}\n" +
                         $"Current Rotation Speed: {currentRotationSpeed}\n" +
                         $"Current Orbit Speed: {currentOrbitSpeed}\n";
        }

    }


    private void RotateAroundAxis()
    {
        float rotationThisFrame = Time.deltaTime * selfRotationSpeed * orbitAcceleration;
        transform.Rotate(0f, rotationThisFrame, 0f, Space.Self);
        transform.localRotation = Quaternion.Euler(axialTilt, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);

        float currentRotation = transform.eulerAngles.y;

        if (currentRotation >= 360f)
        {
            selfRotationCount++;
            Debug.Log($"{gameObject.name} has rotated around its axis {selfRotationCount} times.");
            transform.Rotate(0f, -360f, 0f, Space.Self);
        }


    }

    private float TrueAnomaly(float meanAnomaly, float eccentricity, int iterations = 1000)
    {
        float E = meanAnomaly;
        for (int i = 0; i < iterations; i++)
        {
            E -= (E - eccentricity * Mathf.Sin(E) - meanAnomaly) / (1 - eccentricity * Mathf.Cos(E));
        }
        return 2 * Mathf.Atan2(Mathf.Sqrt(1 + eccentricity) * Mathf.Sin(E / 2), Mathf.Sqrt(1 - eccentricity) * Mathf.Cos(E / 2));
    }


}
