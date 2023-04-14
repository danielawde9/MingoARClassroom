using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.XR.ARKit;
using UnityEngine;

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

    public float scale = 1.0f;
    public float timeScale = 1.0f;
    public TextMeshProUGUI logText;
    public Material orbitLineMaterial;

    private PlanetDataList planetDataList;

    private void Start()
    {
        LoadPlanetData();
        SpawnPlanets();

        CreateOrbitLines();
    }

    private void LoadPlanetData()
    {

        TextAsset jsonFile = Resources.Load<TextAsset>("planet_data");
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
            // Apply the rotation correction when instantiating the prefab
            planet.planetInstance = Instantiate(prefab, CalculatePosition(planet, 0f), prefab.transform.rotation * rotationCorrection);
            planet.planetInstance.transform.localScale = new Vector3(1 * scale, 1 * scale, 1 * scale);
            planet.rotationSpeed = 360f / planet.rotationPeriod;
            planet.orbitProgress = 0f;
            planet.rotationProgress = 0f;

            // Set the rotation axis based on obliquity to orbit
            planet.rotationAxis = Quaternion.Euler(0, 0, planet.obliquityToOrbit) * Vector3.up;

        }
    }
    private void CreateOrbitLines()
    {
        foreach (var planet in planetDataList.planets)
        {
            CreateOrbitLine(planet);
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
            planet.planetInstance.transform.Rotate(0, rotationDelta, 0, Space.Self);

            // Rotate the planet around the Sun
            Vector3 sunPosition = Vector3.zero;
            planet.planetInstance.transform.RotateAround(sunPosition, Vector3.up, orbitDelta);

            // Update rotation and orbit progress
            planet.rotationProgress += Mathf.Abs(rotationDelta);
            planet.orbitProgress += Mathf.Abs(orbitDelta);

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

    private Vector3 CalculatePosition(PlanetData planet, float deltaTime)
    {
        planet.orbitProgress += deltaTime / planet.orbitalPeriod;
        float angle = (planet.orbitProgress * 2 * Mathf.PI) % (2 * Mathf.PI);
        float semiMajorAxis = planet.distanceFromSun * scale;
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(planet.orbitalEccentricity, 2));
        float x = semiMajorAxis * Mathf.Cos(angle);
        float z = semiMinorAxis * Mathf.Sin(angle);
        // Apply the orbital inclination
        float y = Mathf.Sin(angle) * semiMinorAxis * Mathf.Tan(planet.orbitalInclination * Mathf.Deg2Rad);


        return new Vector3(x, y, z);
    }


    private void CreateOrbitLine(PlanetData planet)
    {
        GameObject orbitLine = new GameObject($"{planet.name} Orbit Line");
        LineRenderer lineRenderer = orbitLine.AddComponent<LineRenderer>();
        lineRenderer.material = orbitLineMaterial;
        lineRenderer.widthMultiplier = 0.5f;
        lineRenderer.positionCount = 360;

        float angleStep = 360f / lineRenderer.positionCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float angle = i * angleStep;
            Vector3 position = CalculateOrbitLinePosition(planet, angle );
            lineRenderer.SetPosition(i, position);
        }
    }

    private Vector3 CalculateOrbitLinePosition(PlanetData planet, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float semiMajorAxis = planet.distanceFromSun * scale;
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(planet.orbitalEccentricity, 2));
        float x = semiMajorAxis * Mathf.Cos(radians);
        float z = semiMinorAxis * Mathf.Sin(radians);
        // Apply the orbital inclination
        float y = Mathf.Sin(radians) * semiMinorAxis * Mathf.Tan(planet.orbitalInclination * Mathf.Deg2Rad);

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


// todo fix prefabs 90 angle
// todo add also the planet orbit
