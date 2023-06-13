using System;
using System.Collections;
using System.Collections.Generic;
using MingoData.Scripts.MainUtil;
using MingoData.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
namespace MingoData.Scripts
{

    public class SolarSystemSimulationWithMoons : BasePressInputHandler
    {
        [Serializable]
        public class PlanetData : CelestialBodyData
        {
            public List<MoonData> moons;
            public float distanceFromSun;
            
            public string image;
            
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
        private ARPlaneManager mPlaneManager;
        private ARRaycastManager mRaycastManager;
        private Camera mainCamera;

        public float sizeScale;
        public float timeScale;
        public float distanceScale;
        public UIHandler uiHandler;

        private bool solarSystemPlaced;
        private GameObject selectedPlanet;
        private readonly Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
        private readonly Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
        private GameObject parentDistanceLinesObject;
        private const float planetSelectedScale = 0.5f;
        private bool isAfterScanShown;
        public LocalizationManager localizationManager;
        private bool isSwipeIconToggled;


        private readonly List<string> selectedFields = new List<string>()
        {
            "name", 
            "diameter", 
            "rotationSpeed",
            "rotationPeriod",
            "lengthOfDay",
            "perihelion",
            "aphelion",
            "orbitalPeriod",
            "orbitalVelocity",
            "orbitalInclination",
            "orbitalEccentricity",
            "obliquityToOrbit",
            "orbitProgress",
            "rotationProgress",
            "completedOrbits",
            "completedSelfRotations"
        };
        private List<string> loadedPlanets;

        // private readonly List<string> desiredPlanets = new List<string> {"Sun",  "Venus", "Earth" };

        protected override void OnSwipeUp()
        {
            base.OnSwipeUp();
            if (selectedPlanet == null && !uiHandler.isMenuPanelVisible && uiHandler.initialScanFinished) // Only call the function if the panel is not open and a planet is selected
            {
                Debug.LogError("3k3k");
                uiHandler.ToggleMenuSliderPanel();
            }
        }

        
        protected override void Awake()
        {
            base.Awake();
            mRaycastManager = GetComponent<ARRaycastManager>();
            mainCamera = Camera.main;
        }

        protected override void OnDrag(Vector2 delta)
        {
            if (selectedPlanet == null || uiHandler.isMenuPanelVisible)
                return;

            const float rotationSpeed = 0.1f; // Adjust this value to change the rotation speed
            // Rotate around the y-axis based on x delta (for left/right swipes)
            selectedPlanet.transform.Rotate(0f, -delta.x * rotationSpeed, 0f, Space.World);

            // Rotate around the x-axis based on y delta (for up/down swipes)
            selectedPlanet.transform.Rotate(delta.y * rotationSpeed, 0f, 0f, Space.World);
        }

        protected override void OnPress(Vector3 touchPosition)
        {
            switch (solarSystemPlaced)
            {
                case false:
                {
                    List<ARRaycastHit> sHits = new List<ARRaycastHit>();

                    if (mRaycastManager.Raycast(touchPosition, sHits, TrackableType.PlaneWithinPolygon))
                    {
                        Pose hitPose = sHits[0].pose;

                        Vector3 placementPosition = hitPose.position;
                        SpawnPlanets(placementPosition);
                        uiHandler.UIShowAfterClick();
                        solarSystemPlaced = true;
                        mPlaneManager.enabled = false;
                    }
                    else
                    {
                        solarSystemPlaced = false;
                        mPlaneManager.enabled = true;
                    }
                    break;
                }
                case true when !uiHandler.isMenuPanelVisible:
                    // Detect the planet touch if no swipe gesture is detected
                    DetectPlanetTouch(touchPosition);
                    break;
            }
        }

        private void DetectPlanetTouch(Vector2 touchPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(touchPosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                return;
            GameObject hitObject = hit.collider.gameObject;
            if (!hitObject.CompareTag($"CelestialBody"))
                return;
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

        protected override void OnPressBegan(Vector3 position)
        {
            if (selectedPlanet == null || isSwipeIconToggled)
                return;
            uiHandler.ToggleSwipeIcon(false);
            isSwipeIconToggled = true;
        }

    
        private void SelectPlanet(GameObject planet)
        {
            // If another planet is already selected, return it to its original position and scale first
            if (selectedPlanet != null && selectedPlanet != planet)
            {
                ReturnPlanetToOriginalState();
            }

            selectedPlanet = planet;
            uiHandler.SetPlanetNameTextTitle(selectedPlanet.name, true);
            uiHandler.ToggleSwipeIcon(true);
            isSwipeIconToggled = false;

            // Save the original position and scale of the planet
            originalPositions.TryAdd(planet, planet.transform.position);
            originalScales.TryAdd(planet, planet.transform.localScale);

            uiHandler.SetCelestialBodyData(SolarSystemUtility.planetDataDictionary[planet.name], selectedFields);

            // Move the selected planet in front of the user by one unit and scale it to 1,1,1
            if (Camera.main != null)
            {
                Transform transform1 = Camera.main.transform;
                Vector3 newPosition = transform1.position + transform1.forward;
                selectedPlanet.transform.position = newPosition;
            }
            selectedPlanet.transform.localScale = new Vector3(planetSelectedScale, planetSelectedScale, planetSelectedScale);
        }

        public void ReturnSelectedPlanetToOriginalState()
        {
            if (selectedPlanet == null)
                return;
            ReturnPlanetToOriginalState();
            uiHandler.SetPlanetNameTextTitle("", false);
            selectedPlanet = null;
            uiHandler.ToggleSwipeIcon(false);
            isSwipeIconToggled = false;
            uiHandler.SetCelestialBodyData(null, selectedFields);
        }

        private void ReturnPlanetToOriginalState()
        {
            // Return the planet to its original position and scale
            if (originalPositions.TryGetValue(selectedPlanet, out Vector3 position))
            {
                selectedPlanet.transform.position = position;
            }
            if (originalScales.TryGetValue(selectedPlanet, out Vector3 scale))
            {
                selectedPlanet.transform.localScale = scale;
            }
        }

        public void UpdateTimeScale(float value)
        {
            timeScale = value;
        }

        public void UpdateSizeScale(float value)

        {
            sizeScale = value;

            foreach (PlanetData planetData in SolarSystemUtility.planetDataDictionary.Values)
            {
                UpdateCelestialBodyScale(planetData, sizeScale);
            }
        }

        public void UpdateDistanceScale(float value)
        {
            distanceScale = Mathf.Clamp(value, Constants.minDistance, Constants.maxDistance);

            foreach (PlanetData body in SolarSystemUtility.planetDataDictionary.Values)
            {
                SolarSystemUtility.UpdateOrbitLine(body, (celestialBodyData, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)celestialBodyData, angle, distanceScale));
                if (body.name != "Sun")
                {
                    SolarSystemUtility.UpdateDistanceFromSunText(body, localizationManager);
                }
            }
        }

        private static void UpdateCelestialBodyScale(CelestialBodyData body, float newSizeScaleFactor)
        {
            if (body.name == "Sun")
                return;
            float scale = body.diameter * newSizeScaleFactor;
            body.celestialBodyInstance.transform.localScale = new Vector3(scale, scale, scale);
        }

        private static void UpdatePlanetNameVisibility(bool isOn)
        {
            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
            {
                GameObject planetInstance = planet.celestialBodyInstance;
                GameObject planetName = planetInstance.transform.Find($"{planet.name}_PlanetNameParent").gameObject;
                planetName.SetActive(isOn);
            }
        }

        private static void UpdateInclinationLineVisibility(bool isOn)
        {
            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
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

        private static void UpdateOrbitLineVisibility(bool isOn)
        {
            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
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
            
            string savedPlanetsString = PlayerPrefs.GetString("SelectedPlanets", "");
            loadedPlanets = new List<string>(savedPlanetsString.Split(','));
            string selectedLang = PlayerPrefs.GetString("SelectedPlanetFromGroup", Constants.Lang_EN);
            localizationManager.SetLanguage(selectedLang);
            localizationManager.LoadLocalizedText();

            uiHandler.UIShowInitial();
            uiHandler.onUpdateTimeScaleSlider += UpdateTimeScale;
            uiHandler.onUpdateSizeScaleSlider += UpdateSizeScale;
            uiHandler.onUpdateDistanceScaleSlider += UpdateDistanceScale;

            uiHandler.onPlanetNameToggleValueChanged.AddListener(UpdatePlanetNameVisibility);
            uiHandler.onPlanetInclinationLineToggleValueChanged.AddListener(UpdateInclinationLineVisibility);
            uiHandler.onOrbitLineToggleValueChanged.AddListener(UpdateOrbitLineVisibility);
            uiHandler.onDistanceFromSunToggleValueChanged.AddListener(UpdateDistanceFromSunVisibility);

            uiHandler.SetCelestialBodyData(null, selectedFields);

            SolarSystemUtility.LoadPlanetData(loadedPlanets);
        }
    
        private void InstantiatePlanet(PlanetData planet, Vector3 placedTouchPosition, Quaternion rotationCorrection) 
        { 
            GameObject planetPrefab = Resources.Load<GameObject>(planet.prefabName);
            if (planetPrefab == null) 
            { 
                Debug.LogError($"Prefab not found for {planet.name}"); 
                return; 
            } 
            planet.rotationAxis = Quaternion.Euler(0, 0, planet.obliquityToOrbit) * Vector3.up; 
            distanceScale = Constants.initialDistanceScale;

            Vector3 newPosition;
            if (planet.name == "Sun") 
            {
                newPosition = Vector3.zero;
            }
            else
            {
                Vector3 planetPositionRelativeToSun = SolarSystemUtility.CalculatePlanetPosition(planet, 0f, distanceScale);
                newPosition = placedTouchPosition + planetPositionRelativeToSun;
            }

            planet.celestialBodyInstance = Instantiate(planetPrefab, newPosition, rotationCorrection * planetPrefab.transform.rotation * Quaternion.Euler(planet.rotationAxis));
            planet.celestialBodyInstance.name = planet.name;

            float newScale = GetPlanetScale(planet);
            planet.celestialBodyInstance.transform.localScale = new Vector3(newScale, newScale, newScale);
        
        
            UtilsFns.CreateInclinationLine(planet, planet.celestialBodyInstance, localizationManager);
            UtilsFns.CreatePlanetName(planet, planet.celestialBodyInstance, localizationManager);

        
        }

        private static float GetPlanetScale(CelestialBodyData planet)
        {
            // Adjust the scale for the Sun
            if (planet.name == "Sun")
            {
                // Set scale specific for the Sun
                return Constants.initialSunSizeScale * planet.diameter; // For example, scale it down to 10% of its original size
            }
            return Constants.initialSizeScale * planet.diameter;
        }
    
        private void SpawnPlanets(Vector3 placedTouchPosition) {
            parentDistanceLinesObject = new GameObject("ParentDistanceLines");
            Quaternion rotationCorrection = Quaternion.Euler(0, 0, 0);

            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values) {
                // Check if the current planet's name is in the desiredPlanets list
                // if (desiredPlanets.Contains(planet.name)) {
                InstantiatePlanet(planet, placedTouchPosition, rotationCorrection);
                if (planet.name != "Sun") {
                    UtilsFns.CreateDistanceLineAndTextFromSun(parentDistanceLinesObject, planet);
                    SolarSystemUtility.UpdateDistanceFromSunText(planet, localizationManager);
                } else {
                    SolarSystemUtility.AssignDirectionalLight(planet.celestialBodyInstance.transform, distanceScale, loadedPlanets);
                }
                uiHandler.SetPlanetColorLegend(UtilsFns.GetPlanetColorLegend());
                originalPositions[planet.celestialBodyInstance] = planet.celestialBodyInstance.transform.position;
                SolarSystemUtility.InitPlanetProgress(planet);
                UtilsFns.CreateOrbitLine(planet.celestialBodyInstance, planet, (body, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)body, angle, distanceScale));
                // }
            }
        }

    
        private void SelectPlanetByName(string planetName)
        {
            GameObject planet = GameObject.Find(planetName);
            if (planet != null)
            {
                SelectPlanet(planet);
            }
        }

        private new void OnEnable()
        {
            UIHandler.OnPlanetClicked += SelectPlanetByName;
            mPlaneManager.planesChanged += OnPlanesChanged;
            base.OnEnable();
        }

        private new void OnDisable()
        {
            UIHandler.OnPlanetClicked -= SelectPlanetByName;
            mPlaneManager.planesChanged -= OnPlanesChanged;
            base.OnDisable();
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            if (isAfterScanShown || mPlaneManager.trackables.count < 1)
                return;
            uiHandler.UIShowAfterScan();
            isAfterScanShown = true;
        }


        private void Update()
        {
        
            if (!solarSystemPlaced) return;

            MoveSelectedPlanet();
            UpdateDirectionalLight();
            StartCoroutine(UpdatePlanets());
        }

        private void MoveSelectedPlanet()
        {
            if (selectedPlanet == null) return;
            Transform transform1 = mainCamera.transform;
            selectedPlanet.transform.position = Vector3.Lerp(selectedPlanet.transform.position, transform1.position + transform1.forward * 1f, Time.deltaTime);
        }

        private static void UpdateDirectionalLight()
        {
            if (UtilsFns.directionalLight == null || !SolarSystemUtility.planetDataDictionary.TryGetValue("Sun", out PlanetData sunData)) return;
            Vector3 sunDirection = -sunData.celestialBodyInstance.transform.position.normalized;
            if (sunDirection != Vector3.zero)
            {
                UtilsFns.directionalLight.transform.SetPositionAndRotation(sunData.celestialBodyInstance.transform.position, Quaternion.LookRotation(sunDirection));
            }
        }


        private IEnumerator UpdatePlanets()
        {
            float deltaTime = Time.deltaTime * timeScale;

            foreach (PlanetData planetData in SolarSystemUtility.planetDataDictionary.Values)
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

}

// fixes
// todo fix light range  bs yekbar distance
// todo add black bacground when planet selected
// todo add pinch zoom to increase decrease size 
// todo add toggle buttons to add planets 
// todo check landscape kif bdk tzbta w ipad
// Todo tafe how buttons wara bs planet info 
// todo add color 3l text 
// todo add swipe up icon

// features
// todo add toggle for normizaling the planets size 
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
// todo add show inner structers
// todo move the static funtion 3a janab


/*
 *
 * untiy iphone TLS Allocator ALLOC_TEMP_TLS, underlying allocator ALLOC_TEMP_MAIN has unfreed allocations, size 366
UnityEngine.GUIUtility:ProcessEvent (int,intptr,bool&)

Internal: Stack allocator ALLOC_TEMP_MAIN has unfreed allocations, size 366

To Debug, run app with -diag-temp-memory-leak-validation cmd line argument. This will output the callstacks of the leaked allocations.

TLS Allocator ALLOC_TEMP_TLS, underlying allocator ALLOC_TEMP_MAIN has unfreed allocations, size 366
UnityEngine.GUIUtility:ProcessEvent (int,intptr,bool&)

ChatGPT
The error message you're receiving, "TLS Allocator ALLOC_TEMP_TLS, underlying allocator ALLOC_TEMP_MAIN has unfreed allocations, size 366", is indicating that some temporary memory that was allocated was not freed. This is often caused by a memory leak in the game's code.

The debugging advice given in the error message ("To Debug, run app with -diag-temp-memory-leak-validation cmd line argument. This will output the callstacks of the leaked allocations.") can be quite helpful. It's suggesting that you run your application with the -diag-temp-memory-leak-validation command line argument. This will output callstacks of the leaked allocations, which can give you a good starting point for tracking down where the leak is happening in your code.

Unfortunately, without the specific code causing this issue, I can't give you a precise solution. But, in general, this type of error occurs when a piece of allocated memory isn't properly released. This can occur if you're allocating memory for an object, array, or other data structure but not properly freeing it when it's no longer needed.

You'll want to check your code to ensure you're correctly managing memory. For example, if you're using new to create objects or arrays, make sure you're calling Destroy when they're no longer needed. Similarly, if you're using methods that allocate memory (like some of the Texture2D methods), ensure you're calling the corresponding Release method when done.

Also, check that you're not holding references to objects that you're done with, as this can prevent Unity's garbage collector from freeing up that memory.







 */
// in update function 
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
