using System;
using System.Collections;
using System.Collections.Generic;
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

    private bool isAfterScanShown = false;


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
                // todo check if they didnt click on the scanned area

                SpawnPlanets(placementPosition);

                uiHandler.UIShowAfterClick();

            }

            solarSystemPlaced = true;
            m_PlaneManager.enabled = false;

        }
        else if (solarSystemPlaced && !uiHandler.isMenuPanelVisible)
        {
            Debug.Log(uiHandler.isMenuPanelVisible + "debug menuy");
            // Detect the planet touch if no swipe gesture is detected
            DetectPlanetTouch(touchPosition);

            // Rotate the selected planet if a swipe gesture is detected
            if (selectedPlanet != null && m_DragAction.phase == InputActionPhase.Performed)
            {
                Vector2 swipeDelta = m_DragAction.ReadValue<Vector2>();

                float rotationSpeed = 0.1f; // Adjust this value to change the rotation speed
                selectedPlanet.transform.Rotate(0f, -swipeDelta.x * rotationSpeed, 0f, Space.World);
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
        uiHandler.SetMenuTextTitle(selectedPlanet.name);

        // Save the original position and scale of the planet
        if (!originalPositions.ContainsKey(planet))
        {
            originalPositions[planet] = planet.transform.position;
        }
        if (!originalScales.ContainsKey(planet))
        {
            originalScales[planet] = planet.transform.localScale;
        }

        uiHandler.ToggleSwipeIcon(true);

        // Move the selected planet in front of the user by one unit and scale it to 1,1,1
        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward;
        selectedPlanet.transform.position = newPosition;
        selectedPlanet.transform.localScale = new Vector3(1, 1, 1);
    }

    public void ReturnSelectedPlanetToOriginalState()
    {
        if (selectedPlanet != null)
        {
            ReturnPlanetToOriginalState();
            uiHandler.SetMenuTextTitle("");
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

    private void Start()
    {
        uiHandler.UIShowInitial();
        uiHandler.OnUpdateTimeScaleSlider += UpdateTimeScale;
        uiHandler.OnUpdateSizeScaleSlider += UpdateSizeScale;
        uiHandler.OnUpdateDistanceScaleSlider += UpdateDistanceScale;
        SolarSystemUtility.LoadPlanetData();
    }

    private void SpawnPlanets(Vector3 placedTouchPosition)
    {
        Quaternion rotationCorrection = Quaternion.Euler(0, 0, 0);

        foreach (var planet in SolarSystemUtility.planetDataDictionary.Values)
        {
            GameObject prefab = Resources.Load<GameObject>(planet.prefabName);

            planet.rotationAxis = Quaternion.Euler(0, 0, planet.obliquityToOrbit) * Vector3.up;

            distanceScale = Constants.initialDistanceScale;
            Vector3 planetPositionRelativeToSun = SolarSystemUtility.CalculatePlanetPosition(planet, 0f, distanceScale);
            Vector3 newPosition = placedTouchPosition + planetPositionRelativeToSun;
            planet.celestialBodyInstance = Instantiate(prefab, newPosition, rotationCorrection * prefab.transform.rotation * Quaternion.Euler(planet.rotationAxis));
            planet.celestialBodyInstance.name = planet.name;

            float newScale = Constants.initialSizeScale * planet.diameter;
            planet.celestialBodyInstance.transform.localScale = new(newScale, newScale, newScale);


            SolarSystemUtility.CreateInclinationLine(planet, planet.celestialBodyInstance);

            originalPositions[planet.celestialBodyInstance] = planet.celestialBodyInstance.transform.position;

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found for {planet.name}");
                continue;
            }

            if (planet.name == "Sun")
            {
                SolarSystemUtility.CreateDirectionalLight(planet.celestialBodyInstance.transform, distanceScale, SolarSystemUtility.planetDataDictionary);

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

            SolarSystemUtility.CreateOrbitLine(planet.celestialBodyInstance, planet, (body, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)body, angle, distanceScale));

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
// todo day night texture
// todo add the cockpit
// todo fix moon orbit line 
// todo add solar eclipse
// todo history of the solar system 
// todo scale planet with size to include distance 
// todo add poimt of intresets
// todo add seasons 
// todo add pov planets
// todo show inner structer
// todo add toggles for inlination planeets w hek 
// todo add moons 
// todo add arabic 
// todo optimize anitmation icons 

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
