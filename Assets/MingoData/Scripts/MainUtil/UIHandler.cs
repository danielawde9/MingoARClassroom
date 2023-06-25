﻿using System;
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
using UnityEngine.UI;
using TextMeshProUGUI = TMPro.TextMeshProUGUI;

namespace MingoData.Scripts.MainUtil
{

    public class UIHandler : MonoBehaviour
    {
        [SerializeField]
        private SolarSystemSimulationWithMoons celestialBodyHandler;
        [SerializeField]
        private LocalizationManager localizationManager;
        public AudioSource audioSource;
        public AudioClip clickSound;
        public AudioClip slidingSound;

        [Header("Panel Menu")]
        public GameObject menuSliderPanel;
        public GameObject sliderPanelToggleButton;
        public ScrollRect sliderPanelScrollRect;
        private Shadow sliderPanelToggleButtonShadow;
        private GameObject darkImageBackgroundSliderPanel;
        public bool initialScanFinished;
        private RectTransform sliderButtonToggleRectTransform;
        public bool isUIOverlayEnabled;
        private GameObject sliderButtonToggleImage;
        private RectTransform sliderPanelRectTransform;
        private const float sliderPanelTransitionDuration = 0.2f;
        private Vector2 initialPosition;
        private Vector2 targetPosition;
        private float startRotation;
        private float endRotation;

        [Header("Top Menu Bar")]
        public TextMeshProUGUI menuPlanetName;
        public GameObject closePlanetButton;
        public GameObject returnToMainMenuButton;
        public GameObject topMenuPlanetLayout;
        public GameObject topMenuLayout;

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
        public static event Action<string, bool> OnPlanetClicked;

        [Header("Solar System Toggle")]
        public TextMeshProUGUI solarSystemToggleTitle;
        public Toggle planetDistanceFromSunToggle;
        public Toggle planetShowGuidanceToggle;
        public Toggle planetNameToggle;
        public Toggle planetInclinationLineToggle;
        public Toggle orbitLineToggle;
        public TextMeshProUGUI planetDistanceFromSunToggleTextMeshPro;
        public TextMeshProUGUI planetShowGuidanceToggleTextMeshPro;
        public TextMeshProUGUI planetNameToggleTextMeshPro;
        public TextMeshProUGUI planetInclinationLineToggleTextMeshPro;
        public TextMeshProUGUI orbitLineToggleTextMeshPro;
        public UnityAction<bool> onPlanetNameToggleValueChanged;
        public UnityAction<bool> onOrbitLineToggleValueChanged;
        public UnityAction<bool> onPlanetInclinationLineToggleValueChanged;
        public UnityAction<bool> onDistanceFromSunToggleValueChanged;
        public UnityAction<bool> onPlanetShowGuidanceToggleValueChanged;


        [Header("Solar System Slider")]
        public TextMeshProUGUI solarSystemSliderTitle;
        public Slider timeScaleSlider;
        public Slider sizeScaleSlider;
        public Slider distanceScaleSlider;
        public TextMeshProUGUI menuTimeText;
        public TextMeshProUGUI menuDistanceText;
        public TextMeshProUGUI menuSizeText;
        public TextMeshProUGUI menuSunSizeText;
        public UnityAction<float> onUpdateTimeScaleSlider;
        public UnityAction<float> onUpdateSizeScaleSlider;
        public UnityAction<float> onUpdateDistanceScaleSlider;

        [Header("Horizontal Buttons")]
        public TextMeshProUGUI horizontalButtonsTitle;
        public GameObject pauseButton;
        public GameObject fastForwardButton;
        public GameObject playButton;

        [Header("Middle Icons Text Helper")]
        public GameObject middleIconHelperPrefab;
        public Sprite scanRoomIcon;
        public Sprite tapIcon;
        public Sprite swipeIcon;
        public Sprite swipeIconRotated;
        private static GameObject _darkImageBackgroundInitialUI;
        private MiddleIconHelper uiHelperInit;
        private MiddleIconHelper uiHelperAfterScan;
        private UnityAction uiHelperShowAfterClickFunction;
        private UnityAction uiHelperSwipeToggleFunction;
        private readonly Dictionary<GameObject, int> siblingIndexes = new Dictionary<GameObject, int>();

        private void OnPauseButtonClicked()
        {
            celestialBodyHandler.timeScale = 0;
            UpdateTimeScale(celestialBodyHandler.timeScale);
            PlayClickSound();

        }

        private void OnFastForwardButtonClicked()
        {
            celestialBodyHandler.timeScale *= 2;
            UpdateTimeScale(celestialBodyHandler.timeScale);
            PlayClickSound();

        }

        private void OnPlayButtonClicked()
        {
            celestialBodyHandler.timeScale = 1;
            UpdateTimeScale(celestialBodyHandler.timeScale);
            PlayClickSound();

        }

        private void OnPlanetInfoPanelToggleOnOffClicked()
        {
            PlayClickSound();
            bool isPlanetInfoActive = planetInfoLayout.activeSelf;
            planetInfoLayout.SetActive(!isPlanetInfoActive);
            darkImageBackgroundPlanetInfo.SetActive(!isPlanetInfoActive);
            menuSliderPanel.SetActive(isPlanetInfoActive);
        }

        private void OnReturnPlanetButtonClicked()
        {
            celestialBodyHandler.ReturnSelectedPlanetToOriginalState();
            PlayClickSound();
        }
        private void OnReturnToMainMenuButtonClicked()
        {
            PlayClickSound();
            PlayerPrefs.SetString(Constants.SelectedPlanets, "");
            PlayerPrefs.SetString(Constants.SelectedLanguage, "");
            PlayerPrefs.Save();
            SolarSystemSimulationWithMoons.ClearDictionary();
            UtilsFns.LoadNewScene("MainMenu");
        }
        public void PlayClickSound()
        {
            audioSource.PlayOneShot(clickSound);
        }
        
        public void PlaySliderSound(float value)
        {
            audioSource.PlayOneShot(slidingSound);
        }

        private void Awake()
        {
            // Note: these are added here due to weird error set initially all the toggles are false then in start set them active 
            orbitLineToggle.transform.gameObject.SetActive(false);
            planetNameToggle.transform.gameObject.SetActive(false);
            planetInclinationLineToggle.transform.gameObject.SetActive(false);
            planetDistanceFromSunToggle.transform.gameObject.SetActive(false);
            planetShowGuidanceToggle.transform.gameObject.SetActive(false);
        }

        private void Start()
        {
            TranslationInit();

            TopMenuInit();

            PlanetInfoInit();

            MenuTransitionInit();

            ClickListenerInit();

            SliderInit();

            ToggleButtonsInit();

            SliderShadowInit();

        }

        private void PlanetInfoInit()
        {

            Button planetInfoButtonComponent = planetInfoButton.GetComponent<Button>();
            planetInfoButtonComponent.onClick.AddListener(OnPlanetInfoPanelToggleOnOffClicked);

            darkImageBackgroundPlanetInfo = UtilsFns.CreateDarkBackground("PlanetInfo");
            darkImageBackgroundPlanetInfo.GetComponent<Button>().onClick.AddListener(OnPlanetInfoPanelToggleOnOffClicked);
            darkImageBackgroundPlanetInfo.SetActive(false);

            planetInfoCloseButton.onClick.AddListener(OnPlanetInfoPanelToggleOnOffClicked);

            HorizontalLayoutGroup layoutGroup = planetInfoListItemParentTitle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(layoutGroup);

        }

        private void ClickListenerInit()
        {
            Button returnToMainMenuButtonComponent = returnToMainMenuButton.GetComponent<Button>();
            returnToMainMenuButtonComponent.onClick.AddListener(OnReturnToMainMenuButtonClicked);

            Button returnButtonComponent = closePlanetButton.GetComponent<Button>();
            returnButtonComponent.onClick.AddListener(OnReturnPlanetButtonClicked);

            Button pauseButtonComponent = pauseButton.GetComponent<Button>();
            pauseButtonComponent.onClick.AddListener(OnPauseButtonClicked);

            Button fastForwardButtonComponent = fastForwardButton.GetComponent<Button>();
            fastForwardButtonComponent.onClick.AddListener(OnFastForwardButtonClicked);

            Button playButtonComponent = playButton.GetComponent<Button>();
            playButtonComponent.onClick.AddListener(OnPlayButtonClicked);

        }



        private void MenuTransitionInit()
        {
            // Create dark backgrounds
            darkImageBackgroundSliderPanel = UtilsFns.CreateDarkBackground("SliderPanel");
            darkImageBackgroundSliderPanel.GetComponent<Button>().onClick.AddListener(ToggleMenuSliderPanel);
            darkImageBackgroundSliderPanel.SetActive(false);

            sliderButtonToggleImage = sliderPanelToggleButton.transform.GetChild(0).gameObject;
            sliderPanelRectTransform = menuSliderPanel.GetComponent<RectTransform>();
            sliderButtonToggleRectTransform = sliderPanelToggleButton.GetComponent<RectTransform>();

            // Set the height of the sliding panel to be half of the screen's height
            float screenHeight = Screen.height;
            float halfScreenHeight = screenHeight / 2;
            float sliderToggleButtonLayoutHeight = sliderPanelToggleButton.transform.gameObject.GetComponent<RectTransform>().rect.height;

            Vector2 sizeDelta = sliderPanelRectTransform.sizeDelta;

            // Set the height
            sizeDelta.y = halfScreenHeight;

            // If the screen's width is more than 1080 pixels, set the width to be 80% of the screen's width
            float screenWidth = Screen.width;

            if (screenWidth > 1080)
            {
                sizeDelta.x = screenWidth * 0.8f;
            }

            sliderPanelRectTransform.sizeDelta = sizeDelta;

            // Set the target position of the panel
            targetPosition = new Vector2(0f, sizeDelta.y / 2);
            initialPosition = new Vector2(0f, -halfScreenHeight / 2 + sliderToggleButtonLayoutHeight);
            sliderPanelRectTransform.anchoredPosition = initialPosition;

            // Add listener to the toggle button
            sliderPanelToggleButton.GetComponent<Button>().onClick.AddListener(ToggleMenuSliderPanel);

            SetToggleButtonSize(sliderButtonToggleRectTransform, 250);
        }
        private void TopMenuInit()
        {
            ReverseOrderIfArabic(topMenuPlanetLayout.GetComponent<HorizontalLayoutGroup>());
            StoreSiblingIndexes();
        }

        public void ToggleMenuSliderPanel()
        {
            PlayClickSound();
            isUIOverlayEnabled = !isUIOverlayEnabled;

            startRotation = sliderButtonToggleImage.transform.eulerAngles.z;
            endRotation = isUIOverlayEnabled ? startRotation + 180 : startRotation - 180;

            if (isUIOverlayEnabled)
            {
                sliderPanelScrollRect.verticalNormalizedPosition = 1f;
                SetToggleButtonSize(sliderButtonToggleRectTransform, 100);
                darkImageBackgroundSliderPanel.SetActive(true);
                StartCoroutine(TransitionPanel(initialPosition, targetPosition));
            }
            else
            {
                SetToggleButtonSize(sliderButtonToggleRectTransform, 250);
                darkImageBackgroundSliderPanel.SetActive(false);
                StartCoroutine(TransitionPanel(targetPosition, initialPosition));
            }
        }

        private static void SetToggleButtonSize(RectTransform rectTransform, float size)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta = new Vector2(sizeDelta.x, size);
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -sizeDelta.y / 2);
        }

        private void TranslationInit()
        {

            menuTimeText.text = localizationManager.GetLocalizedValue("1_second_real_life_equals", menuTimeText, false, Constants.ColorGreen, Constants.InitialTimeScale.ToString(CultureInfo.CurrentCulture));

            menuDistanceText.text = localizationManager.GetLocalizedValue("1_meter_distance_equals", menuDistanceText, false, Constants.ColorGreen, (1 / Constants.InitialDistanceScale).ToString("N0"));

            menuSizeText.text = localizationManager.GetLocalizedValue("1_meter_size_equals", menuSizeText, false, Constants.ColorGreen, (1 / Constants.InitialSizeScale).ToString("N0"));

            menuSunSizeText.text = localizationManager.GetLocalizedValue("sun_size_text", menuSunSizeText, false, Constants.ColorGreen, (1 / Constants.InitialSunSizeScale).ToString("N0"));

            horizontalButtonsTitle.text = localizationManager.GetLocalizedValue("Playback_Time_Settings", horizontalButtonsTitle, false, Constants.ColorWhite);

            solarSystemSliderTitle.text = localizationManager.GetLocalizedValue("Planet_Settings", solarSystemSliderTitle, false, Constants.ColorWhite);

            solarSystemToggleTitle.text = localizationManager.GetLocalizedValue("Orbital_Settings", solarSystemToggleTitle, false, Constants.ColorWhite);

            planetLegendsListTitle.text = localizationManager.GetLocalizedValue("Planets_legend", planetLegendsListTitle, false, Constants.ColorWhite);

            planetInfoListItemParentTitle.text = localizationManager.GetLocalizedValue("Planets_Info", planetInfoListItemParentTitle, false, Constants.ColorWhite);

            planetDistanceFromSunToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Distance_From_Sun", planetDistanceFromSunToggleTextMeshPro, false, Constants.ColorWhite);

            planetShowGuidanceToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Planet_Show_Guidance", planetShowGuidanceToggleTextMeshPro, false, Constants.ColorWhite);

            planetNameToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Planet_Name", planetNameToggleTextMeshPro, false, Constants.ColorWhite);

            planetInclinationLineToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Inclination_Line", planetInclinationLineToggleTextMeshPro, false, Constants.ColorWhite);

            orbitLineToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Planet_Orbit", orbitLineToggleTextMeshPro, false, Constants.ColorWhite);
        }

        private void ToggleButtonsInit()
        {
            orbitLineToggle.isOn = true;
            planetNameToggle.isOn = false;
            planetInclinationLineToggle.isOn = false;
            planetDistanceFromSunToggle.isOn = false;
            planetShowGuidanceToggle.isOn = true;

            planetShowGuidanceToggle.onValueChanged.AddListener(onPlanetShowGuidanceToggleValueChanged);
            planetDistanceFromSunToggle.onValueChanged.AddListener(onDistanceFromSunToggleValueChanged);
            orbitLineToggle.onValueChanged.AddListener(onOrbitLineToggleValueChanged);
            planetNameToggle.onValueChanged.AddListener(onPlanetNameToggleValueChanged);
            planetInclinationLineToggle.onValueChanged.AddListener(onPlanetInclinationLineToggleValueChanged);

            orbitLineToggle.transform.gameObject.SetActive(true);
            planetNameToggle.transform.gameObject.SetActive(true);
            planetInclinationLineToggle.transform.gameObject.SetActive(true);
            planetDistanceFromSunToggle.transform.gameObject.SetActive(true);
            planetShowGuidanceToggle.transform.gameObject.SetActive(true);

            HorizontalLayoutGroup planetDistanceFromSunToggleLayoutGroup = planetDistanceFromSunToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(planetDistanceFromSunToggleLayoutGroup);

            HorizontalLayoutGroup planetShowGuidanceToggleLayoutGroup = planetShowGuidanceToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(planetShowGuidanceToggleLayoutGroup);

            HorizontalLayoutGroup planetNameToggleLayoutGroup = planetNameToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(planetNameToggleLayoutGroup);

            HorizontalLayoutGroup planetInclinationLineToggleLayoutGroup = planetInclinationLineToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(planetInclinationLineToggleLayoutGroup);

            HorizontalLayoutGroup orbitLineToggleLayoutGroup = orbitLineToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(orbitLineToggleLayoutGroup);


        }

        private void ReverseOrderIfArabic(HorizontalOrVerticalLayoutGroup layoutGroup)
        {
            layoutGroup.reverseArrangement = localizationManager.GetCurrentLanguage() == Constants.LangAR;
        }

        private void SliderInit()
        {
            timeScaleSlider.value = Constants.InitialTimeScale;
            sizeScaleSlider.value = Constants.InitialSizeScale;
            distanceScaleSlider.value = Constants.InitialDistanceScale;

            timeScaleSlider.minValue = Constants.InitialTimeScale;
            sizeScaleSlider.minValue = Constants.MinSize;
            distanceScaleSlider.minValue = Constants.MinDistance;

            timeScaleSlider.maxValue = Constants.MaxTime;
            sizeScaleSlider.maxValue = Constants.MaxSize;
            distanceScaleSlider.maxValue = Constants.MaxDistance;

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
            textComponent.text = localizationManager.GetLocalizedValue("no_planet_selected", textComponent, false, Constants.ColorWhite);
            TextMeshProUGUI valueComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[1];
            valueComponent.text = "";
        }

        private void AssignFieldName(MemberInfo field, GameObject newDataItem)
        {
            TextMeshProUGUI textComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[0];
            textComponent.text = localizationManager.GetLocalizedValue(field.Name, textComponent, false, Constants.ColorWhite);
            HorizontalLayoutGroup layoutGroup = textComponent.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(layoutGroup);
        }

        private void AssignFieldValue(FieldInfo field, GameObject newDataItem, CelestialBodyData celestialBodyData)
        {
            TextMeshProUGUI valueComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[1];
            object fieldValue = field.GetValue(celestialBodyData);
            if (fieldValue != null)
            {
                if (localizationManager.GetCurrentLanguage() == Constants.LangAR)
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
                valueComponent.text = localizationManager.GetLocalizedValue("null", valueComponent, false, Constants.ColorWhite);
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
                valueComponent.text = localizationManager.GetLocalizedValue(fieldValue.ToString(), valueComponent, false, Constants.ColorWhite);
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
                valueComponent.text = localizationManager.GetLocalizedValue(fieldValue.ToString(), valueComponent, false, Constants.ColorWhite);
            }
            valueComponent.isRightToLeftText = false;
            valueComponent.alignment = TextAlignmentOptions.MidlineRight;
        }

        public void SetPlanetColorLegend(Dictionary<string, PlanetData> planetColorLegend)
        {
            // Remove all previous legend items
            foreach (Transform child in legendParent)
            {
                Destroy(child.gameObject);
            }

            foreach (PlanetData planetData in planetColorLegend.Values)
            {
                // Instantiate new legend item
                GameObject newLegendItem = Instantiate(legendItemPrefab, legendParent);
                newLegendItem.name = "legendInfo" + planetData.name;
                // Assign planet name to Text component
                TextMeshProUGUI textComponent = newLegendItem.GetComponentInChildren<TextMeshProUGUI>();

                // Use localizationManager to get the localized planet name
                string localizedPlanetName = localizationManager.GetLocalizedValue(planetData.name, textComponent, false, Constants.ColorWhite);

                // If the localized name is not found, fall back to the English name
                if (string.IsNullOrEmpty(localizedPlanetName))
                {
                    localizedPlanetName = planetData.name;
                }

                textComponent.text = localizedPlanetName;

                // Assign color to Image component
                Image imageComponent = newLegendItem.GetComponentInChildren<Image>();
                Color planetLineColor = UtilsFns.CreateHexToColor(planetData.planetColor).ToUnityColor();
                imageComponent.color = planetLineColor;

                Button button = newLegendItem.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {

                    PlayClickSound();
                    OnPlanetClicked?.Invoke(planetData.name, true);
                });

            }
        }

        public void SetPlanetNameTextTitle(string text, bool showGameObjectHolder)
        {

            // Use localizationManager to get the localized planet name
            string localizedPlanetName = localizationManager.GetLocalizedValue(text, menuPlanetName, false, Constants.ColorWhite);

            // If the localized name is not found, fall back to the English name
            if (string.IsNullOrEmpty(localizedPlanetName))
            {
                localizedPlanetName = text;
            }

            menuPlanetName.text = localizedPlanetName;

            topMenuPlanetLayout.SetActive(showGameObjectHolder);
            closePlanetButton.SetActive(showGameObjectHolder);
            returnToMainMenuButton.SetActive(!showGameObjectHolder);
            planetInfoButton.SetActive(showGameObjectHolder);
        }

        private void UpdateSizeScale(float value)
        {
            celestialBodyHandler.UpdateSizeScale(value);
            float realLifeSize = 1f / value;
            menuSizeText.text = localizationManager.GetLocalizedValue("1_meter_size_equals", menuSizeText, false, Constants.ColorGreen, realLifeSize.ToString("N0"));
        }

        private void UpdateDistanceScale(float value)
        {
            celestialBodyHandler.UpdateDistanceScale(value);

            float realLifeDistance = 1f / value;
            menuDistanceText.text = localizationManager.GetLocalizedValue("1_meter_distance_equals", menuDistanceText, false, Constants.ColorGreen, realLifeDistance.ToString("N0"));
        }

        private void UpdateTimeScale(float value)
        {
            celestialBodyHandler.UpdateTimeScale(value);
            string timeText = localizationManager.GetLocalizedTimeValue(value, menuTimeText, Constants.ColorGreen);
            menuTimeText.text = timeText;
        }

        // ReSharper disable Unity.PerformanceAnalysis
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

        private void SliderShadowInit()
        {
            sliderPanelToggleButtonShadow = sliderPanelToggleButton.GetComponent<Shadow>();

            // Ensure the shadow is disabled at the start if the user is at the top of the ScrollView
            sliderPanelToggleButtonShadow.enabled = !(sliderPanelScrollRect.normalizedPosition.y >= 1.0f);

            // Add a listener to the ScrollView's onValueChanged event
            sliderPanelScrollRect.onValueChanged.AddListener(OnUserScroll);
        }


        private UnityAction CreateUiHelperFunction(MiddleIconHelper uiHelper)
        {
            return () =>
            {
                PlayClickSound();
                _darkImageBackgroundInitialUI.SetActive(false);
                uiHelper.Destroy();
                ResetSiblingIndexes();
                isUIOverlayEnabled = false;
            };
        }

        private MiddleIconHelper SpawnMiddleIconHelper(string middleIconsTopHelperTitleKey, string middleIconsTextHelperKey, Sprite bottomIconsImage, bool isMiddleIconsTopHelper)
        {
            // Instantiate the prefab
            GameObject instance = Instantiate(middleIconHelperPrefab, transform);

            instance.name = "MiddleIconHelper";

            // Get the components
            MiddleIconHelper helper = new MiddleIconHelper(instance);
            string middleIconsTopHelperTitleText = localizationManager.GetLocalizedValue(middleIconsTopHelperTitleKey, helper.middleIconsTopHelperTitleText, false, Constants.ColorWhite);

            helper.middleIconsTopHelperTitleText.text = middleIconsTopHelperTitleText;
            string middleIconsTextHelper = localizationManager.GetLocalizedValue(middleIconsTextHelperKey, helper.middleIconsTextHelper, false, Constants.ColorWhite);

            helper.middleIconsTextHelper.text = middleIconsTextHelper;

            helper.bottomIconsImage.sprite = bottomIconsImage;

            // Set the activity of the top helper
            helper.middleIconsTopHelper.SetActive(isMiddleIconsTopHelper);

            if (isMiddleIconsTopHelper)
            {
                helper.middleIconsTopHelperCloseButton.onClick.RemoveAllListeners();

                UnityAction uiHelperFunction = CreateUiHelperFunction(helper);

                helper.middleIconsTopHelperCloseButton.onClick.AddListener(uiHelperFunction);

                SetSiblingIndexes();

                isUIOverlayEnabled = true;
            }

            HorizontalLayoutGroup layoutGroup = helper.middleIconsTopHelper.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(layoutGroup);

            instance.SetActive(true);

            return helper;
        }

        public void UIShowInitial()
        {

            uiHelperInit = SpawnMiddleIconHelper(
                "",
                "Move_your_phone_to_start_scanning_the_room",
                scanRoomIcon,
                false);

            _darkImageBackgroundInitialUI = UtilsFns.CreateDarkBackground("InitialUI");

            planetInfoLayout.SetActive(false);
            topMenuPlanetLayout.SetActive(false);
            menuSliderPanel.SetActive(false);
        }

        public void UIShowAfterScan()
        {
            uiHelperInit.Destroy();

            uiHelperAfterScan = SpawnMiddleIconHelper(
                "",
                "Tap_on_the_scanned_area_to_place_the_solar_system",
                tapIcon,
                false);

            initialScanFinished = true;
        }

        public void UIShowAfterClick()
        {
            _darkImageBackgroundInitialUI.SetActive(true);

            uiHelperAfterScan.Destroy();

            SpawnMiddleIconHelper(
                "Instructions",
                "Click_on_any_planet_or_click_on_the_menu_below_to_display_more_settings",
                swipeIconRotated,
                true);

            menuSliderPanel.SetActive(true);
            closePlanetButton.SetActive(false);
            planetInfoButton.SetActive(false);
        }

        public void ToggleSwipeIcon()
        {
            _darkImageBackgroundInitialUI.SetActive(true);
            SpawnMiddleIconHelper(
                "Instructions",
                "Touch_and_drag_to_move_the_planet_Around",
                swipeIcon,
                true);
        }

        private void StoreSiblingIndexes()
        {
            siblingIndexes[menuSliderPanel] = menuSliderPanel.transform.GetSiblingIndex();
            siblingIndexes[topMenuLayout] = topMenuLayout.transform.GetSiblingIndex();
        }

        private void SetSiblingIndexes()
        {
            menuSliderPanel.transform.SetSiblingIndex(0);
            topMenuLayout.transform.SetSiblingIndex(0);
        }

        private void ResetSiblingIndexes()
        {
            menuSliderPanel.transform.SetSiblingIndex(siblingIndexes[menuSliderPanel]);
            topMenuLayout.transform.SetSiblingIndex(siblingIndexes[topMenuLayout]);
            UtilsFns.BringToFront(menuSliderPanel);
        }
    }

}
