using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SolarSystemSimulationWithMoons : BasePressInputHandler
{
    [Serializable]
    public class PlanetData : CelestialBodyData
    {
        public List<MoonData> moons;
        public float distanceFromSun;
        [NonSerialized] public LineRenderer distanceLineRenderer;
        [NonSerialized] public TextMeshPro distanceText;
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

    [SerializeField]
    ARPlaneManager m_PlaneManager;
    private ARRaycastManager m_RaycastManager;
    private Camera mainCamera;

    public float sizeScale;
    public float timeScale;
    public float distanceScale;


    public UIHandler uiHandler;

    private bool solarSystemPlaced = false;
    private GameObject selectedPlanet;
    private readonly Dictionary<GameObject, Vector3> originalPositions = new();
    private readonly Dictionary<GameObject, Vector3> originalScales = new();
    private GameObject parentDistanceLinesObject;
    private const float planetSelectedScale = 0.5f;
    private bool isAfterScanShown = false;
    public LocalizationManager localizationManager;


    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
        mainCamera = Camera.main;
    }

    protected override void OnDrag(Vector2 delta)
    {
        if (selectedPlanet != null && !uiHandler.isMenuPanelVisible)
        {
            uiHandler.ToggleSwipeIcon(false);

            float rotationSpeed = 0.1f; // Adjust this value to change the rotation speed
            // Rotate around the y-axis based on x delta (for left/right swipes)
            selectedPlanet.transform.Rotate(0f, -delta.x * rotationSpeed, 0f, Space.World);

            // Rotate around the x-axis based on y delta (for up/down swipes)
            selectedPlanet.transform.Rotate(delta.y * rotationSpeed, 0f, 0f, Space.World);
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
                uiHandler.UIShowAfterClick();
                solarSystemPlaced = true;
                m_PlaneManager.enabled = false;
            }

            else
            {
                solarSystemPlaced = false;
                m_PlaneManager.enabled = true;
            }

        }
        else if (solarSystemPlaced && !uiHandler.isMenuPanelVisible)
        {
            // Detect the planet touch if no swipe gesture is detected
            DetectPlanetTouch(touchPosition);
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
                if (selectedPlanet != null)
                {
                    // If the touched object is a different planet
                    if (selectedPlanet != hitObject)
                    {
                        // Return the previously selected planet to its original position and scale
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
        // If another planet is already selected, return it to its original position and scale first
        if (selectedPlanet != null && selectedPlanet != planet)
        {
            ReturnPlanetToOriginalState();
        }

        selectedPlanet = planet;
        uiHandler.SetPlanetNameTextTitle(selectedPlanet.name, true);
        uiHandler.ToggleSwipeIcon(true);

        // Save the original position and scale of the planet
        if (!originalPositions.ContainsKey(planet))
        {
            originalPositions[planet] = planet.transform.position;
        }
        if (!originalScales.ContainsKey(planet))
        {
            originalScales[planet] = planet.transform.localScale;
        }

       uiHandler.SetCelestialBodyData(SolarSystemUtility.planetDataDictionary[planet.name]);

        // Move the selected planet in front of the user by one unit and scale it to 1,1,1
        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward;
        selectedPlanet.transform.position = newPosition;
        selectedPlanet.transform.localScale = new Vector3(planetSelectedScale, planetSelectedScale, planetSelectedScale);
    }

    public void ReturnSelectedPlanetToOriginalState()
    {
        if (selectedPlanet != null)
        {
            ReturnPlanetToOriginalState();
            uiHandler.SetPlanetNameTextTitle("", false);
            selectedPlanet = null;
        }
    }

    private void ReturnPlanetToOriginalState()
    {
        // Return the planet to its original position and scale
        if (originalPositions.ContainsKey(selectedPlanet))
        {
            selectedPlanet.transform.position = originalPositions[selectedPlanet];
        }
        if (originalScales.ContainsKey(selectedPlanet))
        {
            selectedPlanet.transform.localScale = originalScales[selectedPlanet];
        }
    }

    public void UpdateTimeScale(float value)
    {
        timeScale = value;
    }

    public void UpdateSizeScale(float value)

    {
        sizeScale = value;

        foreach (var planetData in SolarSystemUtility.planetDataDictionary.Values)
        {
            UpdateCelestialBodyScale(planetData, sizeScale);
        }
    }

    public void UpdateDistanceScale(float value)
    {
        distanceScale = value;

        foreach (var body in SolarSystemUtility.planetDataDictionary.Values)
        {
            SolarSystemUtility.UpdateOrbitLine(body, (body, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)body, angle, distanceScale));
            if(body.name != "Sun")
            {
                SolarSystemUtility.UpdateDistanceFromSunText(body, localizationManager);

            }
        }

    }

    public void UpdateCelestialBodyScale(CelestialBodyData body, float newSizeScaleFactor)
    {
        // todo scale the sun less than the planets 
        if (body.name != "Sun")
        {
            float scale = body.diameter * newSizeScaleFactor;
            body.celestialBodyInstance.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
        
    private void UpdatePlanetNameVisibility(bool isOn)
    {
        foreach (var planet in SolarSystemUtility.planetDataDictionary.Values)
        {
            GameObject planetInstance = planet.celestialBodyInstance;
            GameObject planetName = planetInstance.transform.Find($"{planet.name}_PlanetNameParent").gameObject;
            planetName.SetActive(isOn);
        }
    }
    private void UpdateInclinationLineVisibility(bool isOn)
    {
        foreach (var planet in SolarSystemUtility.planetDataDictionary.Values)
        {
            GameObject planetInstance = planet.celestialBodyInstance;
            GameObject parentObject = planetInstance.transform.Find($"{planet.name}_InclinationLinesParent").gameObject;
            GameObject inclinationLineText = parentObject.transform.Find($"{planet.name}_InclinationLineText").gameObject;
            GameObject yAxis = parentObject.transform.Find($"{planet.name}_YAxis").gameObject;

            GameObject inclinationLine = planetInstance.transform.Find($"{planet.name}_InclinationLine").gameObject;
            parentObject.SetActive(isOn);
            inclinationLine.SetActive(isOn);
            inclinationLineText.SetActive(isOn);
            yAxis.SetActive(isOn);
        }
    }

    private void UpdateOrbitLineVisibility(bool isOn)
    {
        foreach (var planet in SolarSystemUtility.planetDataDictionary.Values)
        {
            GameObject planetInstance = planet.celestialBodyInstance;
            GameObject orbitLine = planetInstance.transform.Find($"{planet.name}_Orbit_Line").gameObject;
            orbitLine.SetActive(isOn);
        }
    }
    private void UpdateDistanceFromSunVisibility(bool isOn)
    {
        foreach (Transform child in parentDistanceLinesObject.transform)
        {
            child.gameObject.SetActive(isOn);
        }
    }


    private void Start()
    {
        uiHandler.UIShowInitial();
        uiHandler.OnUpdateTimeScaleSlider += UpdateTimeScale;
        uiHandler.OnUpdateSizeScaleSlider += UpdateSizeScale;
        uiHandler.OnUpdateDistanceScaleSlider += UpdateDistanceScale;

        uiHandler.onPlanetNameToggleValueChanged.AddListener(UpdatePlanetNameVisibility);
        uiHandler.onPlanetInclinationLineToggleValueChanged.AddListener(UpdateInclinationLineVisibility);
        uiHandler.onOrbitLineToggleValueChanged.AddListener(UpdateOrbitLineVisibility);
        uiHandler.onDistanceFromSunToggleValueChanged.AddListener(UpdateDistanceFromSunVisibility);

        SolarSystemUtility.LoadPlanetData();
    }

    private void SpawnPlanets(Vector3 placedTouchPosition)
    {
        parentDistanceLinesObject = new GameObject("ParentDistanceLines");

        Quaternion rotationCorrection = Quaternion.Euler(0, 0, 0);

        foreach (var planet in SolarSystemUtility.planetDataDictionary.Values)
        {
            GameObject planetPrefab = Resources.Load<GameObject>(planet.prefabName);

            planet.rotationAxis = Quaternion.Euler(0, 0, planet.obliquityToOrbit) * Vector3.up;

            distanceScale = Constants.initialDistanceScale;
            Vector3 planetPositionRelativeToSun = SolarSystemUtility.CalculatePlanetPosition(planet, 0f, distanceScale);
            Vector3 newPosition = placedTouchPosition + planetPositionRelativeToSun;
            planet.celestialBodyInstance = Instantiate(planetPrefab, newPosition, rotationCorrection * planetPrefab.transform.rotation * Quaternion.Euler(planet.rotationAxis));
            planet.celestialBodyInstance.name = planet.name;

            float newScale = Constants.initialSizeScale * planet.diameter;
            planet.celestialBodyInstance.transform.localScale = new(newScale, newScale, newScale);

            SolarSystemUtility.CreateInclinationLine(planet, planet.celestialBodyInstance , localizationManager);

            SolarSystemUtility.CreatePlanetName(planet, planet.celestialBodyInstance , localizationManager);


            if (planet.name != "Sun")
            {
                SolarSystemUtility.CreateDistanceLineAndTextFromSun(parentDistanceLinesObject, planet);
                SolarSystemUtility.UpdateDistanceFromSunText(planet, localizationManager);
            }
            else
            {
                SolarSystemUtility.CreateDirectionalLight(planet.celestialBodyInstance.transform, distanceScale, SolarSystemUtility.planetDataDictionary);

                GameObject directionalLight = GameObject.Find("Directional Light");
                if (directionalLight != null)
                {
                    directionalLight.transform.SetParent(planet.celestialBodyInstance.transform);
                    directionalLight.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
                }
            }

            uiHandler.SetPlanetColorLegend(SolarSystemUtility.GetPlanetColorLegend());

            originalPositions[planet.celestialBodyInstance] = planet.celestialBodyInstance.transform.position;

            planet.rotationSpeed = 360f / planet.rotationPeriod;

            planet.orbitProgress = 0f;
            planet.rotationProgress = 0f;

            SolarSystemUtility.CreateOrbitLine(planet.celestialBodyInstance, planet, (body, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)body, angle, distanceScale));

            if (planetPrefab == null)
            {
                Debug.LogError($"Prefab not found for {planet.name}");
                continue;
            }
        }
    }

    private void Update()
    {
        if (!isAfterScanShown && m_PlaneManager.trackables.count >= 1)
        {
            uiHandler.UIShowAfterScan();
            isAfterScanShown = true;
        }

        if (!solarSystemPlaced) return;

        if (selectedPlanet != null)
        {
            // Move the selected planet to a position in front of the camera
            selectedPlanet.transform.position = Vector3.Lerp(selectedPlanet.transform.position, mainCamera.transform.position + mainCamera.transform.forward * 1f, Time.deltaTime);
        }

        if (SolarSystemUtility.directionalLight != null && SolarSystemUtility.planetDataDictionary.TryGetValue("Sun", out var sunData))
        {
            Vector3 sunDirection = -sunData.celestialBodyInstance.transform.position.normalized;
            if (sunDirection != Vector3.zero)
            {
                SolarSystemUtility.directionalLight.transform.SetPositionAndRotation(sunData.celestialBodyInstance.transform.position, Quaternion.LookRotation(sunDirection));
            }
        }

        // Use a coroutine for calculations that don't need to be made every frame
        StartCoroutine(UpdatePlanets());

    }

    private IEnumerator UpdatePlanets()
    {
        float deltaTime = Time.deltaTime * timeScale;

        foreach (var planetData in SolarSystemUtility.planetDataDictionary.Values)
        {
            // Skip the selected planet for orbital motion calculation
            if (planetData.celestialBodyInstance == selectedPlanet) continue;

            float rotationDelta = -deltaTime / planetData.rotationPeriod * 360f;
            float orbitDelta = deltaTime / planetData.orbitalPeriod * 360f;

            planetData.celestialBodyInstance.transform.Rotate(planetData.rotationAxis, rotationDelta, Space.World);

            if (planetData.name != "Sun")
            {
                planetData.orbitProgress += orbitDelta;
                planetData.celestialBodyInstance.transform.position = SolarSystemUtility.CalculatePlanetPosition(planetData, planetData.orbitProgress, distanceScale);
                SolarSystemUtility.UpdateDistanceFromSunLine(planetData);
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
}

// fixes
// todo default text for the planet info incase no planet is seletected

// features
// todo add songs and click sound
// todo add seasons 
// todo add pov planets
// todo add solar eclipse
// todo day night texture
// todo add the cockpit
// todo add moons 
// TODO add meteor 
// todo add login screen mtl ios planet background 
// todo add smooth transition for selecting planets
// TODO add SATURN RINGS 
// todo add point of intresets on planets
// todo add history of the solar system 
// todo add show inner structer

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
