using System;
using System.Collections.Generic;
using TMPro;
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
        }
    }

    private void SpawnPlanets()
    {
        foreach (var planet in planetDataList.planets)
        {
            GameObject prefab = Resources.Load<GameObject>(planet.prefabName);
            planet.planetInstance = Instantiate(prefab, CalculatePosition(planet, 0f), Quaternion.identity);
            planet.planetInstance.transform.localScale = new Vector3(1 * scale, 1 * scale, 1 * scale); ;
            planet.rotationSpeed = 360f / planet.rotationPeriod;
            planet.orbitProgress = 0f;
            planet.rotationProgress = 0f;
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

            //planet.planetInstance.transform.Rotate(0, planet.rotationSpeed * deltaTime, 0, Space.Self);

            //// Update planet position and rotation around the sun
            //Vector3 newPosition = CalculatePosition(planet, deltaTime);
            //planet.planetInstance.transform.position = newPosition;
            //planet.planetInstance.transform.RotateAround(Vector3.zero, Vector3.up, 360f * deltaTime / planet.orbitalPeriod);

            //// Update self-rotation
            //float selfRotationSpeed = 360f * (deltaTime / planet.rotationPeriod);
            //planet.planetInstance.transform.Rotate(Vector3.up, selfRotationSpeed);


            //LogProgress(planet, deltaTime);


            //CreateOrbitLine(planet);
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
        return new Vector3(x, 0, z);
    }

    //private void LogProgress(PlanetData planet, float deltaTime)
    //{
    //    float orbitDelta = deltaTime / planet.orbitalPeriod;
    //    float rotationDelta = deltaTime / planet.rotationPeriod;

    //    planet.orbitProgress += orbitDelta;
    //    planet.rotationProgress += rotationDelta;

    //    if (planet.orbitProgress >= 1)
    //    {
    //        planet.orbitProgress %= 1;
    //        planet.completedOrbits++;
    //        UpdateLogText();
    //    }

    //    if (planet.rotationProgress >= 1)
    //    {
    //        planet.rotationProgress %= 1;
    //        planet.completedRotations++;
    //        UpdateLogText();
    //    }
    //}

    //private void UpdateLogText()
    //{
    //    logText.text = "";
    //    foreach (var planet in planetDataList.planets)
    //    {
    //        logText.text += $"{planet.name}: {planet.completedOrbits} orbits, {planet.completedRotations} rotations\n";
    //    }
    //}

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
    private LineRenderer CreateOrbitLine(PlanetData planet)
    {
        GameObject orbitLineObject = new GameObject(planet.name + "_OrbitLine");
        LineRenderer orbitLine = orbitLineObject.AddComponent<LineRenderer>();

        orbitLine.material = orbitLineMaterial;
        orbitLine.widthMultiplier = 0.1f;
        orbitLine.positionCount = 180;
        orbitLine.loop = true;
        orbitLine.useWorldSpace = true;

        float angleStep = 360f / orbitLine.positionCount;
        for (int i = 0; i < orbitLine.positionCount; i++)
        {
            float angle = angleStep * i;
            float deltaTime = angle / 360f * planet.orbitalPeriod;
            Vector3 newPosition = CalculatePosition(planet, deltaTime);

            orbitLine.SetPosition(i, newPosition);
        }

        return orbitLine;
    }



}


// todo fix prefabs 90 angle