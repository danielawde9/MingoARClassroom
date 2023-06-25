using System.Collections;
using System.Collections.Generic;
using MingoData.Scripts.MainUtil;
using MingoData.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MingoData.Scripts
{

    public class SolarSystemSimulationWithMoons : BasePressInputHandler
    {
        [SerializeField]
        private ARPlaneManager mPlaneManager;
        [SerializeField]
        private UIHandler uiHandler;
        [SerializeField]
        private LocalizationManager localizationManager;
        [SerializeField]
        private GameObject planetGuidancePrefab;
        
        [HideInInspector]
        public float sizeScale;
        [HideInInspector]
        public float timeScale;
        [HideInInspector]
        public float distanceScale;

        private ARRaycastManager mRaycastManager;
        private Camera mainCamera;
        private Canvas canvas;
        private Coroutine movePlanetCoroutine; 

        private GameObject parentDistanceLinesObject;
        private GameObject selectedPlanet;

        private bool isAfterScanShown;
        private bool isCoroutineRunning;
        private bool isSolarSystemPlaced;
        private bool isPlanetGuidanceActive = true;
        private bool isPlanetSelected;
        private bool isDistanceFromSunVisible;

        private const float planetSelectedScale = 0.5f;

        private static readonly Dictionary<GameObject, Vector3> OriginalPositions = new Dictionary<GameObject, Vector3>();
        private static readonly Dictionary<GameObject, Vector3> OriginalScales = new Dictionary<GameObject, Vector3>();

        private List<string> loadedPlanets;
        private readonly List<string> selectedFields = new List<string>
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
        

        protected override void OnSwipeUp()
        {
            base.OnSwipeUp();
            // Only call the function if the panel is not open and a planet is selected
            if (!isPlanetSelected && !uiHandler.isUIOverlayEnabled && uiHandler.initialScanFinished)
            {
                uiHandler.ToggleMenuSliderPanel();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            mRaycastManager = GetComponent<ARRaycastManager>();
            mainCamera = Camera.main;
            DontDestroyOnLoad(mainCamera);
        }

        protected override void OnDrag(Vector2 delta)
        {
            if (!isPlanetSelected || uiHandler.isUIOverlayEnabled)
                return;

            const float rotationSpeed = 0.1f; // Adjust this value to change the rotation speed
            // Rotate around the y-axis based on x delta (for left/right swipes)
            selectedPlanet.transform.Rotate(0f, -delta.x * rotationSpeed, 0f, Space.World);

            // Rotate around the x-axis based on y delta (for up/down swipes)
            selectedPlanet.transform.Rotate(delta.y * rotationSpeed, 0f, 0f, Space.World);
        }

        protected override void OnPress(Vector3 touchPosition)
        {
            switch (isSolarSystemPlaced)
            {
                case false:
                {
                    List<ARRaycastHit> sHits = new List<ARRaycastHit>();

                    // to detect if the user clicked on the scanned area
                    if (mRaycastManager.Raycast(touchPosition, sHits, TrackableType.PlaneWithinPolygon))
                    {
                        Pose hitPose = sHits[0].pose;
                        Vector3 placementPosition = hitPose.position;
                        SpawnPlanets(placementPosition);
                        uiHandler.UIShowAfterClick();
                        isSolarSystemPlaced = true;
                        mPlaneManager.enabled = false;
                    }
                    else
                    {
                        isSolarSystemPlaced = false;
                        mPlaneManager.enabled = true;
                    }
                    break;
                }
                case true when !uiHandler.isUIOverlayEnabled:
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
            // NOTE: make sure to add CelestialBody tag 
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

        private void SelectPlanet(GameObject planet)
        {
            // If another planet is already selected, return it to its original position and scale first
            if (selectedPlanet != null && selectedPlanet != planet)
            {
                ReturnSelectedPlanetToOriginalState();
            }

            selectedPlanet = planet;
            isPlanetSelected = true;
            
            uiHandler.PlayClickSound();
            uiHandler.SetPlanetNameTextTitle(selectedPlanet.name, true);
            
            uiHandler.ToggleSwipeIcon();
            uiHandler.SetCelestialBodyData(SolarSystemUtility.planetDataDictionary[planet.name], selectedFields);

            // Save the original position and scale of the planet
            OriginalPositions.TryAdd(planet, planet.transform.position);
            OriginalScales.TryAdd(planet, planet.transform.localScale);

            // Move the selected planet in front of the user by one unit and scale it to 1,1,1
            if (mainCamera != null)
            {
                Transform transform1 = mainCamera.transform;
                Vector3 newPosition = transform1.position + transform1.forward;
                selectedPlanet.transform.position = newPosition;
            }

            selectedPlanet.transform.localScale = new Vector3(planetSelectedScale, planetSelectedScale, planetSelectedScale);

            if (movePlanetCoroutine != null)
            {
                StopCoroutine(movePlanetCoroutine);
            }

            movePlanetCoroutine = StartCoroutine(MoveSelectedPlanetWithUserCoroutine(selectedPlanet));
        }

        private IEnumerator MoveSelectedPlanetWithUserCoroutine(GameObject planet)
        {
            while (isPlanetSelected)
            {
                Transform transform1 = mainCamera.transform;
                planet.transform.position = Vector3.Lerp(planet.transform.position, transform1.position + transform1.forward * 1f, Time.deltaTime);
                yield return null;
            }
        }

        public void ReturnSelectedPlanetToOriginalState()
        {

            // Stop the coroutine
            if (movePlanetCoroutine != null)
            {
                StopCoroutine(movePlanetCoroutine);
                movePlanetCoroutine = null;
            }
            // Return the planet to its original position and scale
            if (OriginalPositions.TryGetValue(selectedPlanet, out Vector3 position))
            {
                selectedPlanet.transform.position = position;
            }
            if (OriginalScales.TryGetValue(selectedPlanet, out Vector3 scale))
            {
                selectedPlanet.transform.localScale = scale;
            }
            uiHandler.SetPlanetNameTextTitle("", false);
            uiHandler.SetCelestialBodyData(null, selectedFields);
            
            isPlanetSelected = false;
            selectedPlanet = null;

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
            distanceScale = Mathf.Clamp(value, Constants.MinDistance, Constants.MaxDistance);

            foreach (PlanetData body in SolarSystemUtility.planetDataDictionary.Values)
            {
                SolarSystemUtility.UpdateOrbitLine(body, (celestialBodyData, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)celestialBodyData, angle, distanceScale));
                if (body.name != Constants.PlanetSun)
                {
                    SolarSystemUtility.UpdateDistanceFromSunText(body, localizationManager);
                }
            }
        }

        private  void UpdateCelestialBodyScale(CelestialBodyData body, float newSizeScaleFactor)
        {
            if (body.name == Constants.PlanetSun)
                return;
            float scale = body.diameter * newSizeScaleFactor;
            body.celestialBodyInstance.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void UpdatePlanetNameVisibilityToggle(bool isOn)
        {
            uiHandler.PlayClickSound();

            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
            {
                GameObject planetInstance = planet.celestialBodyInstance;
                GameObject planetName = planetInstance.transform.Find($"{planet.name}_PlanetNameParent").gameObject;
                planetName.SetActive(isOn);
            }
        }

        private void UpdateInclinationLineVisibilityToggle(bool isOn)
        {
            uiHandler.PlayClickSound();

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

        private void UpdateOrbitLineVisibilityToggle(bool isOn)
        {
            uiHandler.PlayClickSound();

            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
            {
                GameObject planetInstance = planet.celestialBodyInstance;
                GameObject orbitLine = planetInstance.transform.Find($"{planet.name}_Orbit_Line").gameObject;
                orbitLine.SetActive(isOn);
            }
        }
        
        private void UpdateDistanceFromSunVisibilityToggle(bool isOn)
        {
            uiHandler.PlayClickSound();

            isDistanceFromSunVisible = isOn;
            foreach (Transform child in parentDistanceLinesObject.transform)
            {
                child.gameObject.SetActive(isOn);
            }
        }

        private void UpdateTogglePlanetGuidanceVisibilityToggle(bool isOn)
        {
            uiHandler.PlayClickSound();
            isPlanetGuidanceActive = isOn;
            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
            {
                planet.planetGuidance.SetActive(isOn);
            }
        }

        public static void ClearDictionary()
        {
            OriginalScales.Clear();
            OriginalPositions.Clear();
            SolarSystemUtility.ClearDictionary();
        }


        private void Start()
        {
            canvas = uiHandler.GetComponent<Canvas>();
         
            string savedPlanetsString = PlayerPrefs.GetString(Constants.SelectedPlanets, "");
            string selectedLang = PlayerPrefs.GetString(Constants.SelectedLanguage, Constants.LangEn);

            loadedPlanets = new List<string>(savedPlanetsString.Split(','));
            localizationManager.SetLanguage(selectedLang);
            localizationManager.LoadLocalizedText();

            uiHandler.UIShowInitial();
            uiHandler.onUpdateTimeScaleSlider += UpdateTimeScale;
            uiHandler.onUpdateSizeScaleSlider += UpdateSizeScale;
            uiHandler.onUpdateDistanceScaleSlider += UpdateDistanceScale;

            uiHandler.onPlanetNameToggleValueChanged = UpdatePlanetNameVisibilityToggle;
            uiHandler.onOrbitLineToggleValueChanged = UpdateOrbitLineVisibilityToggle;
            uiHandler.onPlanetInclinationLineToggleValueChanged = UpdateInclinationLineVisibilityToggle;
            uiHandler.onDistanceFromSunToggleValueChanged = UpdateDistanceFromSunVisibilityToggle;
            uiHandler.onPlanetShowGuidanceToggleValueChanged = UpdateTogglePlanetGuidanceVisibilityToggle;
            

            uiHandler.SetCelestialBodyData(null, selectedFields);

            SolarSystemUtility.LoadPlanetData(loadedPlanets);
        }
        
        private void SpawnPlanets(Vector3 placedTouchPosition)
        {
            uiHandler.PlayClickSound();
            parentDistanceLinesObject = new GameObject("ParentDistanceLines");
            Quaternion rotationCorrection = Quaternion.Euler(0, 0, 0);

            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
            {
                InstantiatePlanet(planet, placedTouchPosition, rotationCorrection);
                if (planet.name != Constants.PlanetSun)
                {
                    SolarSystemUtility.CreateDistanceLineAndTextFromSun(parentDistanceLinesObject, planet);
                    SolarSystemUtility.UpdateDistanceFromSunText(planet, localizationManager);
                }
                else
                {
                    SolarSystemUtility.AssignDirectionalLight(planet.celestialBodyInstance.transform, distanceScale, loadedPlanets);
                }
                uiHandler.SetPlanetColorLegend(SolarSystemUtility.planetDataDictionary);
                OriginalPositions[planet.celestialBodyInstance] = planet.celestialBodyInstance.transform.position;
                SolarSystemUtility.InitPlanetProgress(planet);
                UtilsFns.CreateOrbitLine(planet.celestialBodyInstance, planet, (body, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)body, angle, distanceScale));
            }
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
            distanceScale = Constants.InitialDistanceScale;

            Vector3 newPosition;
            if (planet.name == Constants.PlanetSun)
            {
                newPosition = Vector3.zero;
            }
            else
            {
                Vector3 planetPositionRelativeToSun = SolarSystemUtility.CalculatePlanetPosition(planet, 0f, distanceScale);
                newPosition = placedTouchPosition + planetPositionRelativeToSun;
            }

            // Instantiate the planetGuidance
            planet.planetGuidance = Instantiate(planetGuidancePrefab, canvas.transform, false);
            planet.planetGuidanceRectTransform = planet.planetGuidance.GetComponent<RectTransform>();
            planet.planetGuidance.SetActive(false);
            planet.planetGuidance.name = planet.name + "_Guidance";
            planet.planetGuidanceImage = planet.planetGuidance.transform.Find("PlanetImage").GetComponent<Image>();
            Sprite planetSprite = Resources.Load<Sprite>("SolarSystemWithMoon/PlanetImages/" + planet.name);
            planet.planetGuidanceImage.sprite = planetSprite;
            planet.planetGuidance.GetComponent<Button>().onClick.AddListener(() => SelectPlanetByName(planet.name, false));
            planet.planetGuidance.transform.SetSiblingIndex(0);


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
            if (planet.name == Constants.PlanetSun)
            {
                // Set scale specific for the Sun
                return Constants.InitialSunSizeScale * planet.diameter;
            }
            return Constants.InitialSizeScale * planet.diameter;
        }


        private void SelectPlanetByName(string planetName, bool toggleMenuOn)
        {
            GameObject planet = GameObject.Find(planetName);
            if (planet == null)
                return;
            SelectPlanet(planet);
            if (toggleMenuOn)
            {
                uiHandler.ToggleMenuSliderPanel();
            }
        }

        private new void OnEnable()
        {
            isCoroutineRunning = true;
            UIHandler.OnPlanetClicked += SelectPlanetByName;
            mPlaneManager.planesChanged += OnPlanesChanged;
            base.OnEnable();
        }

        private new void OnDisable()
        {
            isCoroutineRunning = false;
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
            if (!isSolarSystemPlaced) return;

            StartCoroutine(UpdateOffScreenGuidanceCoroutine());
            StartCoroutine(UpdateDirectionalLightCoroutine());
            StartCoroutine(UpdatePlanetsCoroutine());
        }

        private bool IsObjectVisibleFromCamera(GameObject obj)
        {
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(obj.transform.position);
            return viewportPosition is { z: > 0, x: > 0 and < 1, y: > 0 and < 1 };
        }

        private IEnumerator UpdateOffScreenGuidanceCoroutine()
        {
            if (!isPlanetGuidanceActive)
                yield break;
            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
            {
                if (IsObjectVisibleFromCamera(planet.celestialBodyInstance))
                {
                    planet.planetGuidance.SetActive(false);
                }
                else
                {
                    planet.planetGuidance.SetActive(true);
                    SolarSystemUtility.UpdatePlanetGuidancePosition(planet, mainCamera);
                }
            }
            yield return null;
        }

        private IEnumerator UpdateDirectionalLightCoroutine()
        {
            while (isCoroutineRunning)
            {
                if (!SolarSystemUtility.planetDataDictionary.TryGetValue(Constants.PlanetSun, out PlanetData sunData)) yield break;
                Vector3 sunDirection = -sunData.celestialBodyInstance.transform.position.normalized;
                if (sunDirection != Vector3.zero)
                {
                    UtilsFns.directionalLight.transform.SetPositionAndRotation(sunData.celestialBodyInstance.transform.position, Quaternion.LookRotation(sunDirection));
                }
                yield return new WaitForSeconds(1); // Wait for 1 second before running the loop again
            }
        }

        private IEnumerator UpdatePlanetsCoroutine()
        {
            float deltaTime = Time.deltaTime * timeScale;

            foreach (PlanetData planetData in SolarSystemUtility.planetDataDictionary.Values)
            {
                // Skip the selected planet for orbital motion calculation
                if (planetData.celestialBodyInstance == selectedPlanet) continue;

                float rotationDelta = -deltaTime / planetData.rotationPeriod * 360f;
                float orbitDelta = deltaTime / planetData.orbitalPeriod * 360f;
                planetData.celestialBodyInstance.transform.Rotate(planetData.rotationAxis, rotationDelta, Space.World);

                if (planetData.name != Constants.PlanetSun)
                {
                    planetData.orbitProgress += orbitDelta;
                    planetData.celestialBodyInstance.transform.position = SolarSystemUtility.CalculatePlanetPosition(planetData, planetData.orbitProgress, distanceScale);

                    if (isDistanceFromSunVisible)
                    {
                        SolarSystemUtility.UpdateDistanceFromSunLine(planetData);
                    }
                }

                planetData.rotationProgress += Mathf.Abs(rotationDelta);

                int completedSelfRotations = Mathf.FloorToInt(planetData.rotationProgress / 360f);
                int completedOrbits = Mathf.FloorToInt(planetData.orbitProgress / 360f);

                if (completedSelfRotations == planetData.completedSelfRotations && completedOrbits == planetData.completedOrbits)
                    continue;
                planetData.completedSelfRotations = completedSelfRotations;
                planetData.completedOrbits = completedOrbits;
            }
            yield return null;
        }
    }
}

// fixes
// todo planets too spread out 
// todo planets too big
// todo fix translation
// todo check camera permission
// todo fix light range on distance
// todo add black background when planet selected
// todo swipe icon not working 
// todo fix solar dust 
// todo add audio 
// todo add animation for middle helper 

// features
// todo add toggle for sound  
// todo populate language from the translation json 
// todo add swipe up icon
// todo add layout legend divider 
// todo add tutorial 
// todo add pinch zoom to increase decrease size 
// todo add size perspective
// todo add toggle for normalizing the planets size 
// todo add songs and click sound
// todo add seasons 
// todo add solar dust toggle
// todo add pov planets
// todo document your code
// todo add solar eclipse
// todo day night texture
// todo add the cockpit
// todo add moons 
// TODO add meteor 
// todo add login screen mtl ios planet background 
// todo add smooth transition for selecting planets
// TODO add SATURN RINGS 
// todo add point of interests on planets
// todo add history of the solar system 
// todo add show inner structures




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
