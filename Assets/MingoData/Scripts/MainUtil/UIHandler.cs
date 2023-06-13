using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ArabicSupport;
using MingoData.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MingoData.Scripts.MainUtil
{
    public class UIHandler : MonoBehaviour
    {
        [SerializeField]
        public SolarSystemSimulationWithMoons celestialBodyHandler;
        [SerializeField]
        public LocalizationManager localizationManager;

        
        [Header("Panel Menu")]
        public GameObject menuSliderPanel;
        public GameObject sliderPanelToggleButton;
        public ScrollRect sliderPanelScrollRect;
        private Shadow sliderPanelToggleButtonShadow;
        private GameObject darkImageBackgroundSliderPanel;
        public bool initialScanFinished;
        
        [HideInInspector]
        public bool isMenuPanelVisible;
        private GameObject sliderButtonToggleImage;
        private RectTransform sliderPanelRectTransform;
        private const float sliderPanelTransitionDuration = 0.2f;
        private Vector2 initialPosition;
        private Vector2 targetPosition;
        private float startRotation;
        private float endRotation;

        [Header("Top Menu Bar")]
        public TextMeshProUGUI menuPlanetName;
        public GameObject returnPlanetButton;
        public GameObject returnToMainMenuButton;

        [Header("Planet Info Center List")]
        public GameObject planetInfoButton;
        public GameObject planetInfoItemPrefab;
        public Transform planetInfoItemListParent;
        public TextMeshProUGUI planetInfoListItemParentTitle;
        public GameObject planetInfoLayout;
        private GameObject darkImageBackgroundPlanetInfo;
        public Button planetInfoCloseButton;

        [Header("Planet Legends List")]
        public TextMeshProUGUI planetLegendsListTitle;
        public GameObject legendItemPrefab;
        public Transform legendParent;
        public static event Action<string> OnPlanetClicked;

        [Header("Solar System Toggle")]
        public TextMeshProUGUI solarSystemToggleTitle;
        public Toggle planetDistanceFromSunToggle;
        public Toggle planetNameToggle;
        public Toggle planetInclinationLineToggle;
        public Toggle orbitLineToggle;
        public TextMeshProUGUI planetDistanceFromSunToggleTextMeshPro;
        public TextMeshProUGUI planetNameToggleTextMeshPro;
        public TextMeshProUGUI planetInclinationLineToggleTextMeshPro;
        public TextMeshProUGUI orbitLineToggleTextMeshPro;

        [Header("Solar System Slider")]
        public TextMeshProUGUI solarSystemSliderTitle;
        public Slider timeScaleSlider;
        public Slider sizeScaleSlider;
        public Slider distanceScaleSlider;
        public TextMeshProUGUI menuTimeText;
        public TextMeshProUGUI menuDistanceText;
        public TextMeshProUGUI menuSizeText;
        public TextMeshProUGUI menuSunSizeText;

        [Header("Middle Icons Text Helper")]
        public TextMeshProUGUI middleIconsHelperText;
        public GameObject scanRoomIconObject;
        public GameObject tapIconObject;
        public GameObject swipeIconObject;
        public UnityAction<float> onUpdateTimeScaleSlider;
        public UnityAction<float> onUpdateSizeScaleSlider;
        public UnityAction<float> onUpdateDistanceScaleSlider;
        [HideInInspector]
        public UnityEvent<bool> onPlanetNameToggleValueChanged;
        [HideInInspector]
        public UnityEvent<bool> onOrbitLineToggleValueChanged;
        [HideInInspector]
        public UnityEvent<bool> onPlanetInclinationLineToggleValueChanged;
        [HideInInspector]
        public UnityEvent<bool> onDistanceFromSunToggleValueChanged;


        [Header("Horizontal Buttons")]
        public TextMeshProUGUI horizontalButtonsTitle;
        public GameObject pauseButton;
        public GameObject fastForwardButton;
        public GameObject playButton;
        private TextMeshProUGUI pauseButtonTextMeshPro;
        private TextMeshProUGUI fastForwardButtonTextMeshPro;
        private TextMeshProUGUI playButtonTextMeshPro;



        private void OnPauseButtonClicked()
        {
            celestialBodyHandler.timeScale = 0;
            UpdateTimeScale(celestialBodyHandler.timeScale);
        }

        private void OnFastForwardButtonClicked()
        {
            celestialBodyHandler.timeScale *= 2; // double the speed
            UpdateTimeScale(celestialBodyHandler.timeScale);
        }

        private void OnPlayButtonClicked()
        {
            celestialBodyHandler.timeScale = 1; // reset to real-time
            UpdateTimeScale(celestialBodyHandler.timeScale);
        }

        private void OnReturnButtonClick()
        {
            PlayerPrefs.SetString(Constants.SelectedPlanets, "");
            PlayerPrefs.Save();
            SolarSystemSimulationWithMoons.clearDicionary();
            celestialBodyHandler.ReturnSelectedPlanetToOriginalState();
        }

        private void Awake()
        {
            // NOTE: these are added here due to weird error set initially all the toggles are false then in start set them active 
            orbitLineToggle.transform.gameObject.SetActive(false);
            planetNameToggle.transform.gameObject.SetActive(false);
            planetInclinationLineToggle.transform.gameObject.SetActive(false);
            planetDistanceFromSunToggle.transform.gameObject.SetActive(false);
        }

        private void TogglePlanetInfoPanel()
        {
            bool isPlanetInfoActive = planetInfoLayout.activeSelf;
            planetInfoLayout.SetActive(!isPlanetInfoActive);
            darkImageBackgroundPlanetInfo.SetActive(!isPlanetInfoActive);
            menuSliderPanel.SetActive(isPlanetInfoActive);
        }

        private void Start()
        {
            TranslationInit();

            PlanetInfoInit();

            MenuTransitionInit();

            ClickListenerInit();

            SliderInit();

            ToggleButtonsInit();

            InitSliderShadow();

        }
        
        private void PlanetInfoInit()
        {

            Button planetInfoButtonComponent = planetInfoButton.GetComponent<Button>();
            planetInfoButtonComponent.onClick.AddListener(TogglePlanetInfoPanel);

            darkImageBackgroundPlanetInfo = UtilsFns.CreateDarkBackground();
            darkImageBackgroundPlanetInfo.GetComponent<Button>().onClick.AddListener(TogglePlanetInfoPanel);
            darkImageBackgroundPlanetInfo.SetActive(false);
            planetInfoCloseButton.onClick.AddListener(TogglePlanetInfoPanel);

            HorizontalLayoutGroup layoutGroup = planetInfoListItemParentTitle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(layoutGroup);

        }


        private void ClickListenerInit()
        {
            Button returnToMainMenuButtonComponent = returnToMainMenuButton.GetComponent<Button>();
            returnToMainMenuButtonComponent.onClick.AddListener(ReturnToMainMenu);
            
            Button returnButtonComponent = returnPlanetButton.GetComponent<Button>();
            returnButtonComponent.onClick.AddListener(OnReturnButtonClick);

            Button pauseButtonComponent = pauseButton.GetComponent<Button>();
            pauseButtonComponent.onClick.AddListener(OnPauseButtonClicked);

            Button fastForwardButtonComponent = fastForwardButton.GetComponent<Button>();
            fastForwardButtonComponent.onClick.AddListener(OnFastForwardButtonClicked);

            Button playButtonComponent = playButton.GetComponent<Button>();
            playButtonComponent.onClick.AddListener(OnPlayButtonClicked);

        }
        private static void ReturnToMainMenu()
        {
            UtilsFns.LoadNewScene("MainMenu");
        }


        private void MenuTransitionInit()
        {
           
            
            
            // Create dark backgrounds
            darkImageBackgroundSliderPanel = UtilsFns.CreateDarkBackground();
            darkImageBackgroundSliderPanel.GetComponent<Button>().onClick.AddListener(ToggleMenuSliderPanel);
            darkImageBackgroundSliderPanel.SetActive(false);


            sliderButtonToggleImage = sliderPanelToggleButton.transform.GetChild(0).gameObject;
            sliderPanelRectTransform = menuSliderPanel.GetComponent<RectTransform>();

            // Set the height of the sliding panel to be half of the screen's height
            float screenHeight = Screen.height;
            float halfScreenHeight = screenHeight / 2;
            float sliderToggleButtonLayoutHeight = sliderPanelToggleButton.transform.gameObject.GetComponent<RectTransform>().rect.height;

            Vector2 sizeDelta = sliderPanelRectTransform.sizeDelta;
            sizeDelta = new Vector2(sizeDelta.x, halfScreenHeight);
            sliderPanelRectTransform.sizeDelta = sizeDelta;

            // Set the target position of the panel
            targetPosition = new Vector2(0f, sizeDelta.y / 2);
            initialPosition = new Vector2(0f, -halfScreenHeight / 2 + sliderToggleButtonLayoutHeight);
            sliderPanelRectTransform.anchoredPosition = initialPosition;
            // Add listener to the toggle button
            sliderPanelToggleButton.GetComponent<Button>().onClick.AddListener(ToggleMenuSliderPanel);
        }


        public void ToggleMenuSliderPanel()
        {
            isMenuPanelVisible = !isMenuPanelVisible;

            startRotation = sliderButtonToggleImage.transform.eulerAngles.z;
            endRotation = isMenuPanelVisible ? startRotation + 180 : startRotation - 180;

            if (isMenuPanelVisible)
            {
                darkImageBackgroundSliderPanel.SetActive(true);
                StartCoroutine(TransitionPanel(initialPosition, targetPosition));
                if (middleIconsHelperText.transform.parent.gameObject.activeInHierarchy)
                    middleIconsHelperText.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                darkImageBackgroundSliderPanel.SetActive(false);
                StartCoroutine(TransitionPanel(targetPosition, initialPosition));
            }
        }
        

        private void TranslationInit()
        {

            menuTimeText.text = localizationManager.GetLocalizedValue("1_second_real_life_equals", menuTimeText, false, Constants.initialTimeScale.ToString(CultureInfo.CurrentCulture));
            menuDistanceText.text = localizationManager.GetLocalizedValue("1_meter_distance_equals", menuDistanceText, false, (1 / Constants.initialDistanceScale).ToString("N0"));
            menuSizeText.text = localizationManager.GetLocalizedValue("1_meter_size_equals", menuSizeText, false, (1 / Constants.initialSizeScale).ToString("N0"));
            menuSunSizeText.text = localizationManager.GetLocalizedValue("sun_size_text", menuSunSizeText, false, (1 / Constants.initialSunSizeScale).ToString("N0"));

            pauseButtonTextMeshPro = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
            fastForwardButtonTextMeshPro = fastForwardButton.GetComponentInChildren<TextMeshProUGUI>();
            playButtonTextMeshPro = playButton.GetComponentInChildren<TextMeshProUGUI>();

            pauseButtonTextMeshPro.text = localizationManager.GetLocalizedValue("Pause", pauseButtonTextMeshPro, true);
            fastForwardButtonTextMeshPro.text = localizationManager.GetLocalizedValue("Fast_Forward", fastForwardButtonTextMeshPro, true);
            playButtonTextMeshPro.text = localizationManager.GetLocalizedValue("Play", playButtonTextMeshPro, true);
            horizontalButtonsTitle.text = localizationManager.GetLocalizedValue("Playback_Time_Settings", horizontalButtonsTitle, false);
            solarSystemSliderTitle.text = localizationManager.GetLocalizedValue("Planet_Settings", solarSystemSliderTitle, false);
            solarSystemToggleTitle.text = localizationManager.GetLocalizedValue("Orbital_Settings", solarSystemToggleTitle, false);
            planetLegendsListTitle.text = localizationManager.GetLocalizedValue("Planets_legend", planetLegendsListTitle, false);
            planetInfoListItemParentTitle.text = localizationManager.GetLocalizedValue("Planets_Info", planetInfoListItemParentTitle, false);

            planetDistanceFromSunToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Distance_From_Sun", planetDistanceFromSunToggleTextMeshPro, false);
            planetNameToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Planet_Name", planetNameToggleTextMeshPro, false);
            planetInclinationLineToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Inclination_Line", planetInclinationLineToggleTextMeshPro, false);
            orbitLineToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Planet_Orbit", orbitLineToggleTextMeshPro, false);
        }

        private void ToggleButtonsInit()
        {
            orbitLineToggle.isOn = true;
            planetNameToggle.isOn = false;
            planetInclinationLineToggle.isOn = false;
            planetDistanceFromSunToggle.isOn = false;

            planetDistanceFromSunToggle.onValueChanged.AddListener((isOn) => { onDistanceFromSunToggleValueChanged?.Invoke(isOn); });
            orbitLineToggle.onValueChanged.AddListener((isOn) => { onOrbitLineToggleValueChanged?.Invoke(isOn); });
            planetNameToggle.onValueChanged.AddListener((isOn) => { onPlanetNameToggleValueChanged?.Invoke(isOn); });
            planetInclinationLineToggle.onValueChanged.AddListener((isOn) => { onPlanetInclinationLineToggleValueChanged?.Invoke(isOn); });

            orbitLineToggle.transform.gameObject.SetActive(true);
            planetNameToggle.transform.gameObject.SetActive(true);
            planetInclinationLineToggle.transform.gameObject.SetActive(true);
            planetDistanceFromSunToggle.transform.gameObject.SetActive(true);


            HorizontalLayoutGroup planetDistanceFromSunToggleLayoutGroup = planetDistanceFromSunToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            planetDistanceFromSunToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.Lang_AR);

            HorizontalLayoutGroup planetNameToggleLayoutGroup = planetNameToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            planetNameToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.Lang_AR);

            HorizontalLayoutGroup planetInclinationLineToggleLayoutGroup = planetInclinationLineToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            planetInclinationLineToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.Lang_AR);

            HorizontalLayoutGroup orbitLineToggleLayoutGroup = orbitLineToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            orbitLineToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.Lang_AR);

        }

        private void ReverseOrderIfArabic(HorizontalOrVerticalLayoutGroup layoutGroup)
        {
            layoutGroup.reverseArrangement = localizationManager.GetCurrentLanguage() == Constants.Lang_AR;
        }

        private void SliderInit()
        {
            timeScaleSlider.value = Constants.initialTimeScale;
            sizeScaleSlider.value = Constants.initialSizeScale;
            distanceScaleSlider.value = Constants.initialDistanceScale;

            timeScaleSlider.minValue = Constants.initialTimeScale;
            sizeScaleSlider.minValue = Constants.minSize;
            distanceScaleSlider.minValue = Constants.minDistance;

            timeScaleSlider.maxValue = Constants.maxTime;
            sizeScaleSlider.maxValue = Constants.maxSize;
            distanceScaleSlider.maxValue = Constants.maxDistance;

            onUpdateTimeScaleSlider = UpdateTimeScale;
            onUpdateSizeScaleSlider = UpdateSizeScale;
            onUpdateDistanceScaleSlider = UpdateDistanceScale;

            timeScaleSlider.onValueChanged.AddListener(onUpdateTimeScaleSlider);
            sizeScaleSlider.onValueChanged.AddListener(onUpdateSizeScaleSlider);
            distanceScaleSlider.onValueChanged.AddListener(onUpdateDistanceScaleSlider);

        }

        public void SetCelestialBodyData(CelestialBodyData celestialBodyData, List<string> selectedFields)
        {
            // Remove all previous items
            foreach (Transform child in planetInfoItemListParent)
            {
                Destroy(child.gameObject);
            }

            // Check if celestialBodyData is null (no planet is selected)
            if (celestialBodyData == null)
            {
                CreateNoPlanetDataItem();
                return;
            }

            foreach (string fieldName in selectedFields)
            {
                FieldInfo field = typeof(CelestialBodyData).GetField(fieldName);

                if (field == null)
                    continue;

                GameObject newDataItem = Instantiate(planetInfoItemPrefab, planetInfoItemListParent);
                newDataItem.name = "CelestialBodyData";
                AssignFieldValue(field, newDataItem, celestialBodyData);
                AssignFieldName(field, newDataItem);
            }
        }

        private void CreateNoPlanetDataItem()
        {
            GameObject newDataItem = Instantiate(planetInfoItemPrefab, planetInfoItemListParent);
            TextMeshProUGUI textComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[0];
            textComponent.text = localizationManager.GetLocalizedValue("no_planet_selected", textComponent, false);
            TextMeshProUGUI valueComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[1];
            valueComponent.text = "";
        }

        private void AssignFieldName(MemberInfo field, GameObject newDataItem)
        {
            TextMeshProUGUI textComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[0];
            textComponent.text = localizationManager.GetLocalizedValue(field.Name, textComponent, false);
            HorizontalLayoutGroup layoutGroup = textComponent.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(layoutGroup);
        }

        private void AssignFieldValue(FieldInfo field, GameObject newDataItem, CelestialBodyData celestialBodyData)
        {
            TextMeshProUGUI valueComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[1];
            object fieldValue = field.GetValue(celestialBodyData);
            if (fieldValue != null)
            {
                if (localizationManager.GetCurrentLanguage() == Constants.Lang_AR)
                {
                    AssignArabicFieldValue(fieldValue, valueComponent);
                }
                else
                {
                    AssignNonArabicFieldValue(fieldValue, valueComponent);
                }
            }
            else
            {
                valueComponent.text = localizationManager.GetLocalizedValue("null", valueComponent, false);
            }
        }

        private void AssignArabicFieldValue(object fieldValue, TMP_Text valueComponent)
        {
            // Check if fieldValue is a number
            if (float.TryParse(fieldValue.ToString(), out float number))
            {
                string formattedNumber = number.ToString("N0");
                valueComponent.text = $"{new string(ArabicFixer.Fix(formattedNumber, true, true).ToCharArray().Reverse().ToArray())}";
            }
            else
            {
                valueComponent.text = localizationManager.GetLocalizedValue(fieldValue.ToString(), valueComponent, false);
            }
            valueComponent.isRightToLeftText = true;
            valueComponent.alignment = TextAlignmentOptions.MidlineLeft;
        }

        private void AssignNonArabicFieldValue(object fieldValue, TMP_Text valueComponent)
        {
            // Check if fieldValue is a number
            if (float.TryParse(fieldValue.ToString(), out float number))
            {
                string formattedNumber = number.ToString("N0");
                valueComponent.text = $"{formattedNumber}";
            }
            else
            {
                valueComponent.text = localizationManager.GetLocalizedValue(fieldValue.ToString(), valueComponent, false);
            }
            valueComponent.isRightToLeftText = false;
            valueComponent.alignment = TextAlignmentOptions.MidlineRight;
        }


        public void SetPlanetColorLegend(Dictionary<string, Color> planetColorLegend)
        {
            // Remove all previous legend items
            foreach (Transform child in legendParent)
            {
                Destroy(child.gameObject);
            }

            foreach (KeyValuePair<string, Color> planet in planetColorLegend)
            {
                // Instantiate new legend item
                GameObject newLegendItem = Instantiate(legendItemPrefab, legendParent);
                newLegendItem.name = "legendInfo" + planet.Key;
                // Assign planet name to Text component
                TextMeshProUGUI textComponent = newLegendItem.GetComponentInChildren<TextMeshProUGUI>();

                // Use localizationManager to get the localized planet name
                string localizedPlanetName = localizationManager.GetLocalizedValue(planet.Key, textComponent, false);

                // If the localized name is not found, fall back to the English name
                if (string.IsNullOrEmpty(localizedPlanetName))
                {
                    localizedPlanetName = planet.Key;
                }

                textComponent.text = localizedPlanetName;

                // Assign color to Image component
                Image imageComponent = newLegendItem.GetComponentInChildren<Image>();
                imageComponent.color = planet.Value;

                Button button = newLegendItem.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    OnPlanetClicked?.Invoke(planet.Key);
                });

                HorizontalLayoutGroup layoutGroup = imageComponent.transform.parent.GetComponent<HorizontalLayoutGroup>();
                ReverseOrderIfArabic(layoutGroup);

            }
        }

        public void SetPlanetNameTextTitle(string text, bool showGameObjectHolder)
        {
            menuPlanetName.transform.parent.gameObject.SetActive(showGameObjectHolder);

            // Use localizationManager to get the localized planet name
            string localizedPlanetName = localizationManager.GetLocalizedValue(text, menuPlanetName, false);

            // If the localized name is not found, fall back to the English name
            if (string.IsNullOrEmpty(localizedPlanetName))
            {
                localizedPlanetName = text;
            }

            menuPlanetName.text = localizedPlanetName;

            returnPlanetButton.SetActive(showGameObjectHolder);
            returnToMainMenuButton.SetActive(!showGameObjectHolder);
            planetInfoButton.SetActive(showGameObjectHolder);
        }

        private void SetMiddleIconsHelperText(string text)
        {
            middleIconsHelperText.text = text;
        }

        private void UpdateSizeScale(float value)
        {
            celestialBodyHandler.UpdateSizeScale(value); // Notify SolarSystemSimulationWithMoons
            float realLifeSize = 1f / value;
            menuSizeText.text = localizationManager.GetLocalizedValue("1_meter_size_equals", menuSizeText, false, realLifeSize.ToString("N0"));
        }

        private void UpdateDistanceScale(float value)
        {
            celestialBodyHandler.UpdateDistanceScale(value); // Notify SolarSystemSimulationWithMoons

            float realLifeDistance = 1f / value;
            menuDistanceText.text = localizationManager.GetLocalizedValue("1_meter_distance_equals", menuDistanceText, false, realLifeDistance.ToString("N0"));
        }

        private void UpdateTimeScale(float value)
        {
            celestialBodyHandler.UpdateTimeScale(value); 
            string timeText = localizationManager.GetLocalizedTimeValue(value, menuTimeText);
            menuTimeText.text = timeText;
        }

        private IEnumerator TransitionPanel(Vector2 startPosition, Vector2 endPosition)
        {
            float elapsedTime = 0f;
            while (elapsedTime < sliderPanelTransitionDuration)
            {
                float t = Mathf.Clamp01(elapsedTime / sliderPanelTransitionDuration);
                sliderPanelRectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

                // Rotation
                float currentRotation = Mathf.Lerp(startRotation, endRotation, t);
                sliderButtonToggleImage.transform.eulerAngles = new Vector3(0, 0, currentRotation);


                elapsedTime += Time.deltaTime;
                yield return null;
            }

            sliderPanelRectTransform.anchoredPosition = endPosition;

            // Ensure the rotation finishes exactly at the end rotation
            sliderButtonToggleImage.transform.eulerAngles = new Vector3(0, 0, endRotation);

        }

        // This method will be called whenever the ScrollView's position changes
        private void OnUserScroll(Vector2 scrollPosition)
        {
            // If the scroll position is at the top, disable the shadow
            // otherwise, enable the shadow
            sliderPanelToggleButtonShadow.enabled = !(scrollPosition.y >= 1.0f);
        }

        private void InitSliderShadow()
        {
            sliderPanelToggleButtonShadow = sliderPanelToggleButton.GetComponent<Shadow>();

            // Ensure the shadow is disabled at the start if the user is at the top of the ScrollView
            sliderPanelToggleButtonShadow.enabled = !(sliderPanelScrollRect.normalizedPosition.y >= 1.0f);

            // Add a listener to the ScrollView's onValueChanged event
            sliderPanelScrollRect.onValueChanged.AddListener(OnUserScroll);
        }


        public void UIShowInitial()
        {
            planetInfoLayout.SetActive(false);
            ToggleMiddleIconHelper(true);
            scanRoomIconObject.SetActive(true);
            SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Move_your_phone_to_start_scanning_the_room", middleIconsHelperText, false));
            tapIconObject.SetActive(false);
            returnPlanetButton.SetActive(false);
            menuSliderPanel.SetActive(false);
            planetInfoButton.SetActive(false);
        }

        public void UIShowAfterScan()
        {
            scanRoomIconObject.SetActive(false);
            initialScanFinished = true;
            SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Tap_on_the_scanned_area_to_place_the_solar_system", middleIconsHelperText, false));
            tapIconObject.SetActive(true);
        }

        public void UIShowAfterClick()
        {
            scanRoomIconObject.SetActive(false);
            tapIconObject.SetActive(false);
            menuSliderPanel.SetActive(true);
            returnPlanetButton.SetActive(false);
            planetInfoButton.SetActive(false);
            SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Click_on_any_planet_or_click_on_the_menu_below_to_display_more_settings", middleIconsHelperText, false));
        }

        public void ToggleSwipeIcon(bool toggleState)
        {
            SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Touch_and_drag_to_move_the_planet_Around", middleIconsHelperText, false));
            swipeIconObject.SetActive(toggleState);
            ToggleMiddleIconHelper(toggleState);
        }

        private void ToggleMiddleIconHelper(bool toggleState)
        {
            middleIconsHelperText.transform.parent.gameObject.SetActive(toggleState);
        }
    }

}
