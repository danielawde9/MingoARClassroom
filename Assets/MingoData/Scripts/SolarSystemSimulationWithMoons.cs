using System;
using System.Collections;
using System.Collections.Generic;
using MingoData.Scripts.MainUtil;
using MingoData.Scripts.Utils;
using TMPro;
using Unity.VectorGraphics;
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

            public GameObject arrow;
            public RectTransform arrowRectTransform; // Add this line
            public SVGImage arrowImage;

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

        private Coroutine movePlanetCoroutine; // The reference to the coroutine
        private bool isPlanetSelected;


        public float sizeScale;
        public float timeScale;
        public float distanceScale;
        private const float planetSelectedScale = 0.5f;

        public UIHandler uiHandler;
        public LocalizationManager localizationManager;
        public Canvas canvas;

        private GameObject selectedPlanet;
        private GameObject parentDistanceLinesObject;
        public GameObject arrowPrefab;

        private bool isAfterScanShown;
        private bool isSwipeIconToggled;
        private bool isCoroutineRunning;
        private bool solarSystemPlaced;
        private bool isArrowActive = true;

        private static readonly Dictionary<GameObject, Vector3> OriginalPositions = new Dictionary<GameObject, Vector3>();
        private static readonly Dictionary<GameObject, Vector3> OriginalScales = new Dictionary<GameObject, Vector3>();

        private List<string> loadedPlanets;
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

        protected override void OnSwipeUp()
        {
            base.OnSwipeUp();
            // Only call the function if the panel is not open and a planet is selected
            if (selectedPlanet == null && !uiHandler.isMenuPanelVisible && uiHandler.initialScanFinished)
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
                ReturnPlanetToOriginalState(selectedPlanet);
            }

            selectedPlanet = planet;
            isPlanetSelected = true; // set the flag

            isSwipeIconToggled = false;


            uiHandler.SetPlanetNameTextTitle(selectedPlanet.name, true);
            uiHandler.ToggleSwipeIcon(true);
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

            movePlanetCoroutine = StartCoroutine(MoveSelectedPlanetCoroutine());
        }

        private IEnumerator MoveSelectedPlanetCoroutine()
        {
            while (isPlanetSelected)
            {
                Transform transform1 = mainCamera.transform;
                selectedPlanet.transform.position = Vector3.Lerp(selectedPlanet.transform.position, transform1.position + transform1.forward * 1f, Time.deltaTime);
                yield return null;
            }
        }

        public void ReturnSelectedPlanetToOriginalState()
        {
            if (selectedPlanet == null)
                return;
            
            GameObject tempPlanet = selectedPlanet;

            selectedPlanet = null;
            isPlanetSelected = false; 
            isSwipeIconToggled = false;

            ReturnPlanetToOriginalState(tempPlanet);

            uiHandler.ToggleSwipeIcon(false);
            uiHandler.SetCelestialBodyData(null, selectedFields);
        }

        private void ReturnPlanetToOriginalState(GameObject planet)
        {
            // Stop the coroutine
            if (movePlanetCoroutine != null)
            {
                StopCoroutine(movePlanetCoroutine);
                movePlanetCoroutine = null;
            }
            // Return the planet to its original position and scale
            if (OriginalPositions.TryGetValue(planet, out Vector3 position))
            {
                planet.transform.position = position;
            }
            if (OriginalScales.TryGetValue(planet, out Vector3 scale))
            {
                planet.transform.localScale = scale;
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

        private static void UpdateCelestialBodyScale(CelestialBodyData body, float newSizeScaleFactor)
        {
            if (body.name == Constants.PlanetSun)
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

        private void UpdateToggleArrow(bool isOn)
        {
            isArrowActive = isOn;
            foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
            {
                planet.arrow.SetActive(isOn);
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
            string savedPlanetsString = PlayerPrefs.GetString(Constants.SelectedPlanets, "");
            loadedPlanets = new List<string>(savedPlanetsString.Split(','));
            string selectedLang = PlayerPrefs.GetString(Constants.SelectedLanguage, Constants.LangEn);
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
            uiHandler.onPlanetShowArrowsToggleValueChanged.AddListener(UpdateToggleArrow);

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

            // Instantiate the arrow
            planet.arrow = Instantiate(arrowPrefab, canvas.transform, false);
            planet.arrowRectTransform = planet.arrow.GetComponent<RectTransform>(); // Add this line
            planet.arrow.SetActive(false);
            planet.arrow.name = planet.name + "_Arrow";
            planet.arrowImage = planet.arrow.GetComponent<SVGImage>();
            planet.arrow.transform.SetSiblingIndex(0);

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
                return Constants.InitialSunSizeScale * planet.diameter; // For example, scale it down to 10% of its original size
            }
            return Constants.InitialSizeScale * planet.diameter;
        }

        private void SpawnPlanets(Vector3 placedTouchPosition)
        {
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
                uiHandler.SetPlanetColorLegend(SolarSystemUtility.GetPlanetColorLegend());
                OriginalPositions[planet.celestialBodyInstance] = planet.celestialBodyInstance.transform.position;
                SolarSystemUtility.InitPlanetProgress(planet);
                UtilsFns.CreateOrbitLine(planet.celestialBodyInstance, planet, (body, angle) => SolarSystemUtility.CalculatePlanetPosition((PlanetData)body, angle, distanceScale));
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

        private bool IsObjectVisibleFromCamera(GameObject obj)
        {
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(obj.transform.position);
            return viewportPosition is { z: > 0, x: > 0 and < 1, y: > 0 and < 1 };
        }

        private void UpdateArrowPosition(PlanetData planet)
        {

            Vector3 position = planet.celestialBodyInstance.transform.position;
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);

            if (viewportPosition.z < 0) // Planet is behind the camera
            {
                viewportPosition.x = 1 - viewportPosition.x; // Reflect the position
                viewportPosition.y = 1 - viewportPosition.y; // Reflect the position
                viewportPosition.z = 0; // Ensure the position is not behind the camera
            }

            viewportPosition.x = Mathf.Clamp(viewportPosition.x, 0.1f, 0.9f);
            viewportPosition.y = Mathf.Clamp(viewportPosition.y, 0.1f, 0.9f);
            Vector3 screenPosition = mainCamera.ViewportToScreenPoint(viewportPosition);

            planet.arrowRectTransform.position = screenPosition;

            Vector3 direction = screenPosition - planet.arrowRectTransform.position;
            direction.z = 0;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            planet.arrowRectTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (!SolarSystemUtility.GetPlanetColorLegend().TryGetValue(planet.name, out Color arrowColor))
                return;

            planet.arrowImage.color = arrowColor;
        }

        private void Update()
        {
            if (!solarSystemPlaced) return;

            StartCoroutine(UpdateOffScreenArrowsCoroutine());
            StartCoroutine(UpdateDirectionalLightCoroutine());
            StartCoroutine(UpdatePlanetsCoroutine());
        }

        private IEnumerator UpdateOffScreenArrowsCoroutine()
        {
            while (isArrowActive) // Keep this loop running as long as the script is enabled
            {
                foreach (PlanetData planet in SolarSystemUtility.planetDataDictionary.Values)
                {
                    if (IsObjectVisibleFromCamera(planet.celestialBodyInstance))
                    {
                        planet.arrow.SetActive(false);
                    }
                    else
                    {
                        planet.arrow.SetActive(true);
                        UpdateArrowPosition(planet);
                    }
                }

                yield return new WaitForSeconds(1); // Wait for 1 second before running the loop again
            }
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
            while (isCoroutineRunning)
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
                        SolarSystemUtility.UpdateDistanceFromSunLine(planetData);
                    }

                    planetData.rotationProgress += Mathf.Abs(rotationDelta);

                    int completedSelfRotations = Mathf.FloorToInt(planetData.rotationProgress / 360f);
                    int completedOrbits = Mathf.FloorToInt(planetData.orbitProgress / 360f);

                    if (completedSelfRotations == planetData.completedSelfRotations && completedOrbits == planetData.completedOrbits)
                        continue;
                    planetData.completedSelfRotations = completedSelfRotations;
                    planetData.completedOrbits = completedOrbits;
                }

                yield return new WaitForSeconds(0.2f); // Wait for 1 second before running the loop again
            }
        }
    }

}

// fixes
// todo add tutorial 
// todo check camera permission
// todo fix light range bs yekbar distance
// todo add black background when planet selected
// todo add pinch zoom to increase decrease size 
// todo add swipe up icon
// todo Msh icon monotone
// todo add layout legend divider 
// todo fix color legneds colors

// features
// todo add size prespective
// todo add toggle for normizaling the planets size 
// todo add songs and click sound
// todo add seasons 
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
// todo add point of intresets on planets
// todo add history of the solar system 
// todo add show inner structers




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
