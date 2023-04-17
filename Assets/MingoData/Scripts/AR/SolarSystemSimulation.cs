using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.XR.ARKit;
using UnityEngine;
using UnityEngine.UIElements;

public class SolarSystemSimulation : MonoBehaviour
{
    [Serializable]
    public class PlanetData
    {
        public string name;
        public float mass;
        public float diameter;
        public float rotationPeriod;
        public float distanceFromSun;
        public float orbitalPeriod;
        public float orbitalInclination;
        public float orbitalEccentricity;
        public float obliquityToOrbit;
        public string prefabName;
        [HideInInspector] public GameObject planetInstance;
        [HideInInspector] public float rotationSpeed;
        [HideInInspector] public float orbitProgress;
        [HideInInspector] public float rotationProgress;
        [HideInInspector] public int completedOrbits;
        [HideInInspector] public int completedRotations;
        [HideInInspector] public int completedSelfRotations;
        [HideInInspector] public Vector3 rotationAxis;
        [HideInInspector] public float perihelionDistance;
        [HideInInspector] public float aphelionDistance;
    }

    [Serializable]
    public class PlanetDataList
    {
        public List<PlanetData> planets;
    }

    public float sizeScale = 1.0f;
    public float timeScale = 1.0f;
    public TextMeshProUGUI logText;
    public Material orbitLineMaterial;

    public PlanetDataList planetDataList;

    private void Start()
    {
        LoadPlanetData();
        SpawnPlanets();
    }

    private void LoadPlanetData()
    {

        TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystem/planet_data");
        planetDataList = JsonUtility.FromJson<PlanetDataList>(jsonFile.text);
        foreach (var planetData in planetDataList.planets)
        {
            // Convert rotationPeriod from hours to seconds
            planetData.rotationPeriod *= 3600;
            // Convert orbitalPeriod from days to seconds
            planetData.orbitalPeriod *= 86400;
            // Calculate perihelion and aphelion distances
            planetData.perihelionDistance = planetData.distanceFromSun * (1 - planetData.orbitalEccentricity);
            planetData.aphelionDistance = planetData.distanceFromSun * (1 + planetData.orbitalEccentricity);

        }
    }


    private void SpawnPlanets()
    {
        Quaternion rotationCorrection = Quaternion.Euler(-90, 0, 0);

        foreach (var planet in planetDataList.planets)
        {
            GameObject prefab = Resources.Load<GameObject>(planet.prefabName);

            planet.rotationAxis = Quaternion.Euler(0, 0, planet.obliquityToOrbit) * Vector3.up;

            planet.planetInstance = Instantiate(prefab, CalculatePosition(planet, 0f), rotationCorrection * prefab.transform.rotation * Quaternion.Euler(planet.rotationAxis));
            UpdatePlanetScale(planet);

            //planet.rotationSpeed = 360f / planet.rotationPeriod;
            planet.rotationSpeed = (planet.name == "SolarSystem/Sun") ? -360f / planet.rotationPeriod : 360f / planet.rotationPeriod;

            planet.orbitProgress = 0f;
            planet.rotationProgress = 0f;

            CreateOrbitLine(planet);

        }
    }

    public void UpdatePlanetScale(PlanetData planet)
    {
        if (planet.name != "Sun")
        {
            float diameterScale = planet.diameter * sizeScale;
            planet.planetInstance.transform.localScale = new Vector3(diameterScale, diameterScale, diameterScale);
        }
    }



    private void Update()
    {
        float deltaTime = Time.deltaTime * timeScale;

        foreach (var planet in planetDataList.planets)
        {

            float rotationDelta = deltaTime / planet.rotationPeriod * 360f;
            float orbitDelta = deltaTime / planet.orbitalPeriod * 360f;

            // Rotate the planet around itself
            planet.planetInstance.transform.Rotate(planet.rotationAxis, rotationDelta, Space.World);

            // Update the planet's position along the elliptical orbit (skip for the Sun)
            if (planet.name != "SolarSystem/Sun")
            {
                planet.orbitProgress += orbitDelta;
                planet.planetInstance.transform.position = CalculatePosition(planet, planet.orbitProgress);
            }
           
            // Update rotation and orbit progress
            planet.rotationProgress += Mathf.Abs(rotationDelta);

            // Log the completed rotations and orbits
            int completedSelfRotations = Mathf.FloorToInt(planet.rotationProgress / 360f);
            int completedOrbits = Mathf.FloorToInt(planet.orbitProgress / 360f);


            if (completedSelfRotations != planet.completedSelfRotations)
            {
                planet.completedSelfRotations = completedSelfRotations;
                UpdateLogText(planet.name, planet.completedSelfRotations, planet.completedOrbits);
            }

            if (completedOrbits != planet.completedOrbits)
            {
                planet.completedOrbits = completedOrbits;
                UpdateLogText(planet.name, planet.completedSelfRotations, planet.completedOrbits);
            }
        }
    }
    private void CreateOrbitLine(PlanetData planet)
    {
        GameObject orbitLine = new GameObject($"{planet.name} Orbit Line");
        LineRenderer lineRenderer = orbitLine.AddComponent<LineRenderer>();
        lineRenderer.material = orbitLineMaterial;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 360;

        float angleStep = 360f / lineRenderer.positionCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float angle = i * angleStep;
            Vector3 position = CalculatePosition(planet, angle); // Use the same method for orbit lines
            lineRenderer.SetPosition(i, position);
        }
    }


    private Vector3 CalculatePosition(PlanetData planet, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float semiMajorAxis = planet.distanceFromSun;
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(planet.orbitalEccentricity, 2));

        // Calculate distance using the true anomaly (v) instead of the mean anomaly (radians)
        float eccentricAnomaly = 2 * Mathf.Atan(Mathf.Tan(radians / 2) * Mathf.Sqrt((1 - planet.orbitalEccentricity) / (1 + planet.orbitalEccentricity)));
        float distance = semiMajorAxis * (1 - Mathf.Pow(planet.orbitalEccentricity, 2)) / (1 + planet.orbitalEccentricity * Mathf.Cos(eccentricAnomaly));

        float x = distance * Mathf.Cos(eccentricAnomaly);
        float z = distance * Mathf.Sin(eccentricAnomaly);

        // Apply the orbital inclination
        float y = Mathf.Sin(eccentricAnomaly) * semiMinorAxis * Mathf.Tan(planet.orbitalInclination * Mathf.Deg2Rad);

        return new Vector3(x, y, z);
    }

    private void UpdateLogText(string planetName, int completedSelfRotations, int completedOrbits)
    {
        logText.text = "";
        foreach (var planet in planetDataList.planets)
        {
            if (planet.name == planetName)
            {
                logText.text += $"{planet.name}: {completedOrbits} orbits, {completedSelfRotations} rotations\n";
            }
            else
            {
                logText.text += $"{planet.name}: {planet.completedOrbits} orbits, {planet.completedSelfRotations} rotations\n";
            }
        }
    }
}


// todo add lighting
