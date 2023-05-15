using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.Rendering.DebugUI;

public class SolarSystemSimulationWithMoons : BasePressInputHandler
{
    [Serializable]
    public class PlanetData : CelestialBodyData
    {
        public List<MoonData> moons;
        public float distanceFromSun;
    }

    [Serializable]
    public class MoonData : CelestialBodyData
    {
        public float distanceFromPlanet;
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

    public Slider timeScaleSlider;
    public Slider sizeScaleSlider;
    public Slider distanceScaleSlider;

    public PlanetDataList planetDataList;

    private GameObject directionalLight;

    [SerializeField]
    ARPlaneManager m_PlaneManager;

    private ARRaycastManager m_RaycastManager;
    private Camera mainCamera;
    private bool solarSystemPlaced = false;
    private GameObject selectedPlanet;
    // Member variable for Sun data
    private PlanetData sunData = null;

    private readonly Dictionary<GameObject, Vector3> originalPositions = new();
    readonly Dictionary<GameObject, Vector3> originalScales = new();
    public Dictionary<string, PlanetData> planetDataDictionary; 

    public GameObject returnButton;
    public TextMeshProUGUI planetNameText;

    private void CreateDirectionalLight(Transform sunTransform)
    {
        directionalLight = new GameObject("Directional Light");
        Light lightComponent;
        if (!directionalLight.TryGetComponent<Light>(out lightComponent))
        {
            lightComponent = directionalLight.AddComponent<Light>();
        }
        lightComponent.type = LightType.Point;
        lightComponent.color = Color.white;
        lightComponent.intensity = 1.0f;
        if (planetDataDictionary.TryGetValue("Pluto", out var plutoData))
        {
            float distanceToPluto = Vector3.Distance(Vector3.zero, plutoData.celestialBodyInstance.transform.position);
            Debug.Log("Distance from Sun to Pluto: " + distanceToPluto);

            // Adjust the range of the directional light
            lightComponent.range = distanceToPluto;
        } else
        {
            lightComponent.range = distanceScale * 100;
        }

        directionalLight.transform.SetParent(sunTransform);
        directionalLight.transform.localPosition = Vector3.zero;
    }

    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
        mainCamera = Camera.main;
    }

    protected override void OnDrag(Vector2 delta)
    {
        if (selectedPlanet != null)
        {
            Debug.Log("OnDrag: " + delta);

            float rotationSpeed = 0.1f; // Adjust this value to change the rotation speed
            selectedPlanet.transform.Rotate(0f, -delta.x * rotationSpeed, 0f, Space.World);
        }
    }

    protected override void OnPress(Vector3 touchPosition)
    {
        if (!solarSystemPlaced)
        {
            List<ARRaycastHit> s_Hits = new();

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = s_Hits[0].pose;

                Vector3 placementPosition = hitPose.position;

                SpawnPlanets(placementPosition);
            }

            solarSystemPlaced = true;

            m_PlaneManager.enabled = false;
        }
        else
        {
            // Rotate the selected planet if a swipe gesture is detected
            if (selectedPlanet != null && m_DragAction.phase == InputActionPhase.Performed)
            {
                Vector2 swipeDelta = m_DragAction.ReadValue<Vector2>();

                float rotationSpeed = 0.1f; // Adjust this value to change the rotation speed
                selectedPlanet.transform.Rotate(0f, -swipeDelta.x * rotationSpeed, 0f, Space.World);
            }

            else
            {
                // Detect the planet touch if no swipe gesture is detected
                DetectPlanetTouch(touchPosition);
            }
        }
    }

    void DetectPlanetTouch(Vector2 touchPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("CelestialBody"))
            {
                Debug.Log("CelestialBody touched: " + hitObject.name);

                if (selectedPlanet != null)
                {
                    // If the touched object is a different planet
                    if (selectedPlanet != hitObject)
                    {
                        // Return the previously selected planet to its original position and scale
                        StartCoroutine(MoveToOriginalPositionAndScale(selectedPlanet));
                        SelectPlanet(hitObject);
                    }
                }
                else
                {
                    SelectPlanet(hitObject);
                }

            }
        }
    }

    void SelectPlanet(GameObject planet)
    {
        selectedPlanet = planet;
        planetNameText.text = selectedPlanet.name;

        // When a planet is selected, save its original position and scale
        if (!originalPositions.ContainsKey(planet))
        {
            originalPositions[planet] = planet.transform.position;
        }
        if (!originalScales.ContainsKey(planet))
        {
            originalScales[planet] = planet.transform.localScale;
        }
    }


    public void OnReturnButtonClick()
    {
        if (selectedPlanet != null)
        {
            StartCoroutine(MoveToOriginalPositionAndScale(selectedPlanet));
            planetNameText.text = "";
            selectedPlanet = null;
        }
    }

    private IEnumerator MoveToOriginalPositionAndScale(GameObject planet)
    {
        float transitionDuration = 1.0f; // Adjust this value to control the transition speed
        float elapsed = 0;

        Vector3 originalPosition = originalPositions[planet];
        Vector3 currentPosition = planet.transform.position;
        Vector3 originalScale = originalScales[planet];
        Vector3 currentScale = planet.transform.localScale;

        while (Vector3.Distance(planet.transform.position, originalPosition) > 0.01f || Vector3.Distance(planet.transform.localScale, originalScale) > 0.01f)
        {
            planet.transform.position = Vector3.Lerp(planet.transform.position, originalPosition, elapsed / transitionDuration);
            planet.transform.localScale = Vector3.Lerp(planet.transform.localScale, originalScale, elapsed / transitionDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }


        planet.transform.position = originalPosition;
        planet.transform.localScale = originalScale;
    }

    private Vector3 CalculatePlanetPosition(PlanetData planet, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        float semiMajorAxis = planet.distanceFromSun;
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - planet.orbitalEccentricitySquared);

        float eccentricAnomaly = 2 * Mathf.Atan(Mathf.Tan(radians / 2) * Mathf.Sqrt((1 - planet.orbitalEccentricitySquared) / (1 + planet.orbitalEccentricity)));
        float distance = semiMajorAxis * (1 - planet.orbitalEccentricitySquared) / (1 + planet.orbitalEccentricity * Mathf.Cos(eccentricAnomaly));

        float x = distance * Mathf.Cos(eccentricAnomaly);
        float z = distance * Mathf.Sin(eccentricAnomaly);

        // Apply the orbital inclination
        float y = Mathf.Sin(eccentricAnomaly) * semiMinorAxis * Mathf.Tan(planet.orbitalInclination * Mathf.Deg2Rad);

        return new Vector3(x * distanceScale, y * distanceScale, z * distanceScale);
    }

    private Vector3 CalculateMoonPosition(MoonData moon, PlanetData planet, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float distanceFromPlanet = moon.distanceFromPlanet * distanceScale;

        float x = distanceFromPlanet * Mathf.Cos(radians);
        float y = 0f;
        float z = distanceFromPlanet * Mathf.Sin(radians);

        Vector3 positionRelativeToPlanet = new Vector3(x, y, z);
        Vector3 planetPosition = CalculatePlanetPosition(planet, planet.orbitProgress);

        return planetPosition + positionRelativeToPlanet;
    }

    private void CreateOrbitLine(CelestialBodyData body, float diameterScaleFactor, Func<CelestialBodyData, float, Vector3> calculatePosition)
    {
        GameObject orbitLine = new ($"{body.name} Orbit Line");
        LineRenderer lineRenderer = orbitLine.AddComponent<LineRenderer>();
        lineRenderer.material = orbitLineMaterial;
       // lineRenderer.widthMultiplier = body.diameter * sizeScale * diameterScaleFactor;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 360;

        float angleStep = 360f / lineRenderer.positionCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float angle = i * angleStep;
            Vector3 position = calculatePosition(body, angle);
            lineRenderer.SetPosition(i, position);
        }
    }

    public void UpdateOrbitLine(CelestialBodyData body, string lineObjectName, Func<CelestialBodyData, float, Vector3> calculatePosition)
    {
        LineRenderer lineRenderer = GameObject.Find($"{lineObjectName} Orbit Line").GetComponent<LineRenderer>();

        float angleStep = 360f / lineRenderer.positionCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float angle = i * angleStep;
            Vector3 position = calculatePosition(body, angle);
            lineRenderer.SetPosition(i, position);
        }
    }

    private void UpdateTimeScaleSlider(float value)
    {
        timeScale = value;
    }

    private void UpdateSizeScaleSlider(float value)
    {
        sizeScale = value;
        foreach (var planet in planetDataList.planets)
        {
            UpdateCelestialBodyScale(planet, sizeScale);
            foreach (var moon in planet.moons)
            {
                UpdateCelestialBodyScale(moon, sizeScale);
            }
        }
    }

    private void UpdateDistanceScaleSlider(float value)
    {
        distanceScale = value;

        //foreach (var planet in planetDataList.planets)
        //{
        //    UpdateOrbitLine(planet, $"{planet.name}", (body, angle) => CalculatePlanetPosition((PlanetData)body, angle));
        //    foreach (var moon in planet.moons)
        //    {
        //        UpdateOrbitLine(moon, $"{moon.name}", (body, angle) => CalculateMoonPosition((MoonData)body, planet, angle));
        //    }
        //}
    }

    public void UpdateCelestialBodyScale(CelestialBodyData body, float newSizeScaleFactor)
    {
        if (body.name != "Sun")
        {
            //body.celestialBodyInstance.transform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);
            float scale = body.diameter * newSizeScaleFactor;
            body.celestialBodyInstance.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private void Start()
    {
        timeScaleSlider.onValueChanged.AddListener(UpdateTimeScaleSlider);
        sizeScaleSlider.onValueChanged.AddListener(UpdateSizeScaleSlider);
        distanceScaleSlider.onValueChanged.AddListener(UpdateDistanceScaleSlider);
        LoadPlanetData();
    }

    private void LoadPlanetData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystemWithMoon/planet_data_with_moon");
        planetDataList = JsonUtility.FromJson<PlanetDataList>(jsonFile.text);
        foreach (var planetData in planetDataList.planets)
        {
            planetData.rotationPeriod *= 3600; // convert hours to seconds
            planetData.orbitalPeriod *= 86400; // convert days to seconds
            planetData.perihelion *= 1E6f; // convert 10^6 km to km
            planetData.aphelion *= 1E6f; // convert 10^6 km to km
            planetData.distanceFromSun *= 1E6f; // convert 10^6 km to km
            planetData.orbitalEccentricitySquared = Mathf.Pow(planetData.orbitalEccentricity, 2);

        }
        planetDataDictionary = planetDataList.planets.ToDictionary(p => p.name, p => p);
    }


    private void SpawnPlanets(Vector3 placedTouchPosition)
    {
        Quaternion rotationCorrection = Quaternion.Euler(0, 0, 0);

        foreach (var planet in planetDataList.planets)
        {
            GameObject prefab = Resources.Load<GameObject>(planet.prefabName);

            planet.rotationAxis = Quaternion.Euler(0, 0, planet.obliquityToOrbit) * Vector3.up;

            float initialDistanceScale = 1f / 10000000f; // initial scale of 1:10,000,000
            float newDistanceScaleFactor =  initialDistanceScale;
            distanceScale = initialDistanceScale;
            Debug.Log("New distance scale factor: " + newDistanceScaleFactor);

            Vector3 planetPositionRelativeToSun = CalculatePlanetPosition(planet, 0f);
            Vector3 newPosition = placedTouchPosition + planetPositionRelativeToSun * newDistanceScaleFactor ;
            planet.celestialBodyInstance = Instantiate(prefab, newPosition, rotationCorrection * prefab.transform.rotation * Quaternion.Euler(planet.rotationAxis));

            // Set the name of the instantiated planet
            planet.celestialBodyInstance.name = planet.name;

            float initialSizeScale = 1f / 1000000f;
            float newScale = initialSizeScale * planet.diameter;
            planet.celestialBodyInstance.transform.localScale = new(newScale, newScale, newScale);

            float intialTimeScale = 1f;
            originalPositions[planet.celestialBodyInstance] = planet.celestialBodyInstance.transform.position;

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found for {planet.name}");
                continue;
            }

            if (planet.name == "Sun")
            {
                CreateDirectionalLight(planet.celestialBodyInstance.transform);

                GameObject directionalLight = GameObject.Find("Directional Light");
                if (directionalLight != null)
                {
                    directionalLight.transform.SetParent(planet.celestialBodyInstance.transform);
                    directionalLight.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
                }
            }

            planet.rotationSpeed = 360f / planet.rotationPeriod;

            planet.orbitProgress = 0f;
            planet.rotationProgress = 0f;

            CreateOrbitLine(planet, 1f, (body, angle) => CalculatePlanetPosition((PlanetData)body, angle));
            // For planets
            //CreateLabel(planet.planetInstance, planet.name, "Planet", planet.diameter, planet.rotationSpeed, planet.orbitalPeriod);


        }
    }

    private void Update()
    {
        if (!solarSystemPlaced) return;

        if (selectedPlanet != null)
        {
            // Move the selected planet to a position in front of the camera
            selectedPlanet.transform.position = Vector3.Lerp(selectedPlanet.transform.position, mainCamera.transform.position + mainCamera.transform.forward * 1f, Time.deltaTime);

            // Scale the planet up relative to its original scale
            Vector3 originalScale = originalScales[selectedPlanet];
            // Vector3 targetScale = originalScale * 25f;  // Adjust this value to change the scaling factor
            //Vector3 targetScale = new (sizeScale,sizeScale,sizeScale); 
            Vector3 targetScale = originalScale;
            selectedPlanet.transform.localScale = Vector3.Lerp(selectedPlanet.transform.localScale, targetScale, Time.deltaTime);
        }


        if (directionalLight != null && planetDataDictionary.TryGetValue("Sun", out var sunData))
        {
            Vector3 sunDirection = -sunData.celestialBodyInstance.transform.position.normalized;
            if (sunDirection != Vector3.zero)
            {
                directionalLight.transform.SetPositionAndRotation(sunData.celestialBodyInstance.transform.position, Quaternion.LookRotation(sunDirection));
            }
        }

        // Use a coroutine for calculations that don't need to be made every frame
        StartCoroutine(UpdatePlanets());

    }

    private IEnumerator UpdatePlanets()
    {
        float deltaTime = Time.deltaTime * timeScale;

        foreach (var planetData in planetDataDictionary.Values)
        {
            // Skip the selected planet for orbital motion calculation
            if (planetData.celestialBodyInstance == selectedPlanet) continue;

            float rotationDelta = deltaTime / planetData.rotationPeriod * 360f;
            float orbitDelta = deltaTime / planetData.orbitalPeriod * 360f;

            planetData.celestialBodyInstance.transform.Rotate(planetData.rotationAxis, rotationDelta, Space.World);

            if (planetData.name != "Sun")
            {
                planetData.orbitProgress += orbitDelta;
                planetData.celestialBodyInstance.transform.position = CalculatePlanetPosition(planetData, planetData.orbitProgress);
            }

            planetData.rotationProgress += Mathf.Abs(rotationDelta);

            int completedSelfRotations = Mathf.FloorToInt(planetData.rotationProgress / 360f);
            int completedOrbits = Mathf.FloorToInt(planetData.orbitProgress / 360f);

            if (completedSelfRotations != planetData.completedSelfRotations || completedOrbits != planetData.completedOrbits)
            {
                planetData.completedSelfRotations = completedSelfRotations;
                planetData.completedOrbits = completedOrbits;
            }

            yield return null;
        }
    }
    // todo fix lighting 
    //private void CreateLabel(GameObject celestialObject, string celestialObjectName, string objectType, float jsonSize, float selfRotationSpeed, float orbitSpeed)
    //{
    //    GameObject label = new GameObject($"{celestialObjectName}_Label");
    //    label.transform.SetParent(celestialObject.transform);
    //    label.transform.localPosition = new Vector3(0, (celestialObject.transform.localScale.y / 2) + 0.5f, 0);
    //    label.transform.localRotation = Quaternion.identity;

    //    TextMeshPro tmp = label.AddComponent<TextMeshPro>();
    //    tmp.name = $"{celestialObjectName}_TextMeshPro";
    //    tmp.alignment = TextAlignmentOptions.Center;
    //    tmp.fontSize = 0.3f;
    //    tmp.color = Color.white;

    //    //UpdateLabel(tmp, celestialObjectName, objectType, jsonSize, selfRotationSpeed, orbitSpeed, 0, 0);
    //}

    //private void UpdateLabel(TextMeshPro tmp, string celestialObjectName, string objectType, float jsonSize, float selfRotationSpeed, float orbitSpeed, int completedRotations, int completedOrbits)
    //{
    //    float unityWorldSize = tmp.transform.parent.parent.localScale.x * sizeScale * 1000;

    //    tmp.text = $"{celestialObjectName} ({objectType})\nSize (JSON): {jsonSize}\nUnity world size: {unityWorldSize} meters\nSelf Rotation Speed: {selfRotationSpeed}\nOrbit Speed: {orbitSpeed}\nCompleted Self Rotations: {completedRotations}\nCompleted Orbits: {completedOrbits}";
    //}


    //private void UpdateLogText(string celestialObjectName, int completedRotations, string rotationType)
    //{
    //    logText.text = "";
    //    foreach (var planet in planetDataList.planets)
    //    {
    //        logText.text += $"{planet.name}: {planet.completedOrbits} orbits, {planet.completedSelfRotations} self rotations, Size: {planet.planetInstance.transform.localScale.x * sizeScale * 1000} meters, Local Scale: {planet.planetInstance.transform.localScale}\n";
    //        foreach (var moon in planet.moons)
    //        {
    //            if (moon.name == celestialObjectName)
    //            {
    //                logText.text += $"  {moon.name}: {completedRotations} {rotationType}, Size: {moon.moonInstance.transform.localScale.x * sizeScale * 1000} meters, Local Scale: {moon.moonInstance.transform.localScale}\n";
    //            }
    //            else
    //            {
    //                logText.text += $"  {moon.name}: {moon.completedRotations} Rotations around {planet.name}, Size: {moon.moonInstance.transform.localScale.x * sizeScale * 1000} meters, Local Scale: {moon.moonInstance.transform.localScale}\n";
    //            }
    //        }
    //    }
    //}

}
// todo day night texture
// add the cockpit
// todo fix planets and moons text mesh pro 
// todo fix moon orbit line 
// sun rotatin clockwise
// add solar eclipse
// scale planet with size to include distance 

// in update fuction 
/* foreach (var moon in planet.moons)
{
    float moonRotationDelta = deltaTime / moon.rotationPeriod * 360f;
    float moonOrbitDelta = deltaTime / moon.orbitalPeriod * 360f;

    moon.celestialBodyInstance.transform.Rotate(planet.rotationAxis, moonRotationDelta, Space.World);
    moon.rotationProgress += Mathf.Abs(moonRotationDelta);

    moon.orbitProgress += moonOrbitDelta;
    moon.celestialBodyInstance.transform.position = CalculateMoonPosition(moon, planet, moon.orbitProgress);

    int completedMoonRotations = Mathf.FloorToInt(moon.rotationProgress / 360f);

    if (completedMoonRotations != moon.completedRotations)
    {
        moon.completedRotations = completedMoonRotations;
    }
}*/

// in spawn function
/* foreach (var moon in planet.moons)
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

     moon.perihelionDistance = moon.distanceFromPlanet * (1 - moon.orbitalEccentricity);
     moon.aphelionDistance = moon.distanceFromPlanet * (1 + moon.orbitalEccentricity);

     moon.rotationAxis = Quaternion.Euler(0, 0, moon.obliquityToOrbit) * Vector3.up;

     moon.celestialBodyInstance = Instantiate(moonPrefab, CalculateMoonPosition(moon, planet, 0f), rotationCorrection * moonPrefab.transform.rotation * Quaternion.Euler(moon.rotationAxis));
     moon.celestialBodyInstance.name = moon.name;
     // Set the planet instance as the parent of the moon instance
     moon.celestialBodyInstance.transform.parent = planet.celestialBodyInstance.transform;

     UpdateCelestialBodyScale(moon);

     CreateOrbitLine(moon, 10f, (body, angle) => CalculateMoonPosition((MoonData)body, planet, angle));

     originalPositions[moon.celestialBodyInstance] = moon.celestialBodyInstance.transform.position; // Store original position

     // For moons (inside the moon spawning loop)
     // CreateLabel(moon.moonInstance, moon.name, "Moon", moon.diameter, moon.rotationSpeed, moon.orbitalPeriod);

 }*/
