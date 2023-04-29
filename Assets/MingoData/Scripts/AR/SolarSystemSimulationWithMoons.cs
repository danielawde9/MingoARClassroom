using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SolarSystemSimulationWithMoons : MonoBehaviour
{
    [Serializable]
    public class MoonData
    {
        public string name;
        public float mass;
        public float diameter;
        public float rotationPeriod;
        public float distanceFromPlanet;
        public float orbitalPeriod;
        [HideInInspector] public GameObject moonInstance;
        [HideInInspector] public float rotationSpeed;
        [HideInInspector] public float orbitProgress;
        [HideInInspector] public float rotationProgress;
        public string prefabName;
        [HideInInspector] public int completedOrbits;
        [HideInInspector] public int completedRotations;

    }

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
        public List<MoonData> moons;

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
    public float distanceScale = 1.0f;

    public TextMeshProUGUI logText;
    public Material orbitLineMaterial;

    public UnityEngine.UI.Slider sizeScaleSlider;
    public UnityEngine.UI.Slider timeScaleSlider;
    public UnityEngine.UI.Slider distanceScaleSlider;

    public PlanetDataList planetDataList;

    private void Start()
    {
        sizeScaleSlider.onValueChanged.AddListener(UpdateSizeScale);
        timeScaleSlider.onValueChanged.AddListener(UpdateTimeScale);
        distanceScaleSlider.onValueChanged.AddListener(UpdateDistanceScale);

        LoadPlanetData();
        SpawnPlanets();
    }

    private void LoadPlanetData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystemWithMoon/planet_data_with_moon");
        planetDataList = JsonUtility.FromJson<PlanetDataList>(jsonFile.text);
        Debug.Log($"Spawning {planetDataList}");
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

    private void UpdateSizeScale(float value)
    {
        sizeScale = value;
        foreach (var planet in planetDataList.planets)
        {
            UpdatePlanetScale(planet);
        }
    }

    private void UpdateTimeScale(float value)
    {
        timeScale = value;
    }

    private void UpdateDistanceScale(float value)
    {
        distanceScale = value;
        foreach (var planet in planetDataList.planets)
        {
            UpdateOrbitLine(planet);
        }
    }

    private void SpawnPlanets()
    {
        //Quaternion rotationCorrection = Quaternion.Euler(-90, 0, 0);
        Quaternion rotationCorrection = Quaternion.Euler(0, 0, 0);

        foreach (var planet in planetDataList.planets)
        {
            GameObject prefab = Resources.Load<GameObject>(planet.prefabName);

            planet.rotationAxis = Quaternion.Euler(0, 0, planet.obliquityToOrbit) * Vector3.up;

            planet.planetInstance = Instantiate(prefab, CalculatePosition(planet, 0f), rotationCorrection * prefab.transform.rotation * Quaternion.Euler(planet.rotationAxis));
            UpdatePlanetScale(planet);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found for {planet.name}");
                continue;
            }

            Debug.Log($"Spawning {planet.name}");

            planet.rotationSpeed = (planet.name ==
                "Sun") ? -360f / planet.rotationPeriod : 360f / planet.rotationPeriod;

            planet.orbitProgress = 0f;
            planet.rotationProgress = 0f;

            CreateOrbitLine(planet);

            // Spawn moons
            foreach (var moon in planet.moons)
            {
                GameObject moonPrefab = Resources.Load<GameObject>(moon.prefabName);
                if (moonPrefab == null)
                {
                    Debug.LogError($"Moon prefab not found for {moon.name}");
                    continue;
                }

                moon.rotationSpeed = 360f / moon.rotationPeriod;
                moon.orbitProgress = 0f;
                moon.rotationProgress = 0f;

                moon.moonInstance = Instantiate(moonPrefab, planet.planetInstance.transform);
                moon.moonInstance.name = moon.name;
                UpdateMoonScale(moon);
            }

        }
    }

    public void UpdateMoonScale(MoonData moon)
    {
        float diameterScale = moon.diameter * sizeScale;
        moon.moonInstance.transform.localScale = new Vector3(diameterScale, diameterScale, diameterScale);
    }

    public void UpdatePlanetScale(PlanetData planet)
    {
        if (planet.name != "Sun")
        {
            float diameterScale = planet.diameter * sizeScale;
            planet.planetInstance.transform.localScale = new Vector3(diameterScale, diameterScale, diameterScale);
        }
    }

    private void UpdateOrbitLine(PlanetData planet)
    {
        LineRenderer lineRenderer = GameObject.Find($"{planet.name} Orbit Line").GetComponent<LineRenderer>();

        float angleStep = 360f / lineRenderer.positionCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float angle = i * angleStep;
            Vector3 position = CalculatePosition(planet, angle);
            lineRenderer.SetPosition(i, position);
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime * timeScale;

        foreach (var planet in planetDataList.planets)
        {
            float rotationDelta = deltaTime / planet.rotationPeriod * 360f;
            float orbitDelta = deltaTime / planet.orbitalPeriod * 360f;

            planet.planetInstance.transform.Rotate(planet.rotationAxis, rotationDelta, Space.World);

            if (planet.name != "Sun")
            {
                planet.orbitProgress += orbitDelta;
                planet.planetInstance.transform.position = CalculatePosition(planet, planet.orbitProgress);
            }

            planet.rotationProgress += Mathf.Abs(rotationDelta);

            int completedSelfRotations = Mathf.FloorToInt(planet.rotationProgress / 360f);
            int completedOrbits = Mathf.FloorToInt(planet.orbitProgress / 360f);

            if (completedSelfRotations != planet.completedSelfRotations)
            {
                planet.completedSelfRotations = completedSelfRotations;
                UpdateLogText(planet.name, planet.completedSelfRotations, planet.completedOrbits.ToString());
            }

            if (completedOrbits != planet.completedOrbits)
            {
                planet.completedOrbits = completedOrbits;
                UpdateLogText(planet.name, planet.completedSelfRotations, planet.completedOrbits.ToString());
            }


            // Update moons
            foreach (var moon in planet.moons)
            {
                float moonRotationDelta = deltaTime / moon.rotationPeriod * 360f;
                float moonOrbitDelta = deltaTime / moon.orbitalPeriod * 360f;

                moon.moonInstance.transform.Rotate(planet.rotationAxis, moonRotationDelta, Space.World);
                moon.moonInstance.transform.RotateAround(planet.planetInstance.transform.position, Vector3.up, moonOrbitDelta); moon.rotationProgress += Mathf.Abs(moonRotationDelta);

                int completedMoonRotations = Mathf.FloorToInt(moon.rotationProgress / 360f);

                if (completedMoonRotations != moon.completedRotations)
                {
                    moon.completedRotations = completedMoonRotations;
                    UpdateLogText(moon.name, moon.completedRotations, $"Rotations around {planet.name}");
                }
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
            Vector3 position = CalculatePosition(planet, angle);
            lineRenderer.SetPosition(i, position);
        }
    }

    private Vector3 CalculatePosition(PlanetData planet, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        float semiMajorAxis = planet.distanceFromSun;
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(planet.orbitalEccentricity, 2));

        float eccentricAnomaly = 2 * Mathf.Atan(Mathf.Tan(radians / 2) * Mathf.Sqrt((1 - planet.orbitalEccentricity) / (1 + planet.orbitalEccentricity)));
        float distance = semiMajorAxis * (1 - Mathf.Pow(planet.orbitalEccentricity, 2)) / (




    1 + planet.orbitalEccentricity * Mathf.Cos(eccentricAnomaly));

        float x = distance * Mathf.Cos(eccentricAnomaly);
        float z = distance * Mathf.Sin(eccentricAnomaly);

        // Apply the orbital inclination
        float y = Mathf.Sin(eccentricAnomaly) * semiMinorAxis * Mathf.Tan(planet.orbitalInclination * Mathf.Deg2Rad);

        return new Vector3(x * distanceScale, y * distanceScale, z * distanceScale);
    }


    private void UpdateLogText(string celestialObjectName, int completedRotations, string rotationType)
    {
        logText.text = "";
        foreach (var planet in planetDataList.planets)
        {
            logText.text += $"{planet.name}: {planet.completedOrbits} orbits, {planet.completedSelfRotations} self rotations\n";
            foreach (var moon in planet.moons)
            {
                if (moon.name == celestialObjectName)
                {
                    logText.text += $"  {moon.name}: {completedRotations} {rotationType}\n";
                }
                else
                {
                    logText.text += $"  {moon.name}: {moon.completedRotations} Rotations around {planet.name}\n";
                }
            }
        }
    }
}