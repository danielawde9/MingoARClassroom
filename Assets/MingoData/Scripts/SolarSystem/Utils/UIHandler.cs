using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public SolarSystemSimulationWithMoons celestialBodyHandler;
    public LocalizationManager localizationManager;

    [Header("Tabs Toggle Layout")]
    public List<Button> tabButtons;
    public List<GameObject> tabPanels;

    private TextMeshProUGUI settingsTabTextMeshPro;
    private TextMeshProUGUI planetInfoTabTextMeshPro;

    private Color selectedButtonColor;
    private Color deselectedButtonColor;
    private Color selectedTextColor;
    private Color deselectedTextColor;

    private readonly List<TextMeshProUGUI> tabTexts = new();
    private int currentlySelectedTab = -1;


    [Header("Panel Menu")]
    public GameObject menuSliderPanel;
    public GameObject sliderPanelToggleButton;
    [HideInInspector]
    public bool isMenuPanelVisible = false;
    private GameObject sliderButtonToggleImage;
    private RectTransform sliderPanelRectTransform;
    private readonly float sliderPanelTransitionDuration = 0.2f;
    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private float startRotation;
    private float endRotation;

    [Header("Top Menu Bar")]
    public TextMeshProUGUI menuPlanetName;
    public GameObject returnButton;

    [Header("Planet Legends List")]
    public TextMeshProUGUI planetLegendsListTitle;
    public GameObject legendItemPrefab;
    public Transform legendParent;

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
    public TextMeshProUGUI menuSizeText;
    public TextMeshProUGUI menuDistanceText;

    [Header("Middle Icons Text Helper")]
    public TextMeshProUGUI middleIconsHelperText;
    public GameObject scanRoomIconObject;
    public GameObject tapIconObject;
    public GameObject swipeIconObject;

    public UnityAction<float> OnUpdateTimeScaleSlider;
    public UnityAction<float> OnUpdateSizeScaleSlider;
    public UnityAction<float> OnUpdateDistanceScaleSlider;
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

    [Header("Planet Info 2nd tab List")]
    public GameObject planetInfoItemPrefab;
    public Transform planetInfoItemParent;

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
        celestialBodyHandler.ReturnSelectedPlanetToOriginalState();
    }

    private void Awake()
    {
        // NOTE: These are added here due to weird bug, set initally all the toggles are false then in start set them active 
        orbitLineToggle.transform.gameObject.SetActive(false);
        planetNameToggle.transform.gameObject.SetActive(false);
        planetInclinationLineToggle.transform.gameObject.SetActive(false);
        planetDistanceFromSunToggle.transform.gameObject.SetActive(false);

    }

    private void Start()
    {
        MenuTransistionInit();

        ClickListenerInit();

        SliderInit();

        ToggleButtonsInit();

        localizationManager.SetLanguage(Constants.AR);
        localizationManager.LoadLocalizedText();

        menuTimeText.text = localizationManager.GetLocalizedValue("1_second_real_life_equals", menuTimeText, Constants.initialTimeScale.ToString());
        menuSizeText.text = localizationManager.GetLocalizedValue("1_meter_size_equals", menuSizeText, (1 / Constants.initialSizeScale).ToString());
        menuDistanceText.text = localizationManager.GetLocalizedValue("1_meter_distance_equals", menuDistanceText, (1 / Constants.initialDistanceScale).ToString());


        pauseButtonTextMeshPro = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
        fastForwardButtonTextMeshPro = fastForwardButton.GetComponentInChildren<TextMeshProUGUI>();
        playButtonTextMeshPro = playButton.GetComponentInChildren<TextMeshProUGUI>();

        pauseButtonTextMeshPro.text = localizationManager.GetLocalizedValue("Pause", pauseButtonTextMeshPro);
        fastForwardButtonTextMeshPro.text = localizationManager.GetLocalizedValue("Fast_Forward", fastForwardButtonTextMeshPro);
        playButtonTextMeshPro.text = localizationManager.GetLocalizedValue("Play", playButtonTextMeshPro);
        horizontalButtonsTitle.text = localizationManager.GetLocalizedValue("Playback_Time_Settings", horizontalButtonsTitle);
        solarSystemSliderTitle.text = localizationManager.GetLocalizedValue("Planet_Settings", solarSystemSliderTitle);
        solarSystemToggleTitle.text = localizationManager.GetLocalizedValue("Orbital_Settings", solarSystemToggleTitle);
        planetLegendsListTitle.text = localizationManager.GetLocalizedValue("Planets_legend", planetLegendsListTitle);

        planetDistanceFromSunToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Distance_From_Sun", planetDistanceFromSunToggleTextMeshPro);
        planetNameToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Planet_Name", planetNameToggleTextMeshPro);
        planetInclinationLineToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Inclination_Line", planetInclinationLineToggleTextMeshPro);
        orbitLineToggleTextMeshPro.text = localizationManager.GetLocalizedValue("Display_Planet_Orbit", orbitLineToggleTextMeshPro);


        settingsTabTextMeshPro = tabButtons[0].GetComponentInChildren<TextMeshProUGUI>();
        planetInfoTabTextMeshPro = tabButtons[1].GetComponentInChildren<TextMeshProUGUI>();


        settingsTabTextMeshPro.text = localizationManager.GetLocalizedValue("Settings", settingsTabTextMeshPro);
        planetInfoTabTextMeshPro.text = localizationManager.GetLocalizedValue("The_Solar_System", planetInfoTabTextMeshPro);

        TabSwitchLayoutInit();
    }

    private void TabSwitchLayoutInit()
    {
        selectedButtonColor = HexToColor("#7F8FA6");
        deselectedButtonColor = HexToColor("#DCDDE1");
        selectedTextColor = HexToColor("#F5F6FA");
        deselectedTextColor = HexToColor("#2F3640");

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(index));

            TextMeshProUGUI textComponent = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                tabTexts.Add(textComponent);
            }
            else
            {
                Debug.LogError("No TextMeshProUGUI component found for button at index " + i);
            }
        }

        if (tabPanels.Count > 0)
            ShowTab(0);
    }

    private void ShowTab(int index)
    {
        if (currentlySelectedTab != -1)
        {
            tabButtons[currentlySelectedTab].GetComponent<Image>().color = deselectedButtonColor;
            tabTexts[currentlySelectedTab].color = deselectedTextColor;
            tabPanels[currentlySelectedTab].SetActive(false);
        }

        tabButtons[index].GetComponent<Image>().color = selectedButtonColor;
        tabTexts[index].color = selectedTextColor;
        tabPanels[index].SetActive(true);

        currentlySelectedTab = index;
    }

    private Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    private void ClickListenerInit()
    {
        Button returnButtonComponent = returnButton.GetComponent<Button>();
        returnButtonComponent.onClick.AddListener(OnReturnButtonClick);

        Button pauseButtonComponent = pauseButton.GetComponent<Button>();
        pauseButtonComponent.onClick.AddListener(OnPauseButtonClicked);

        Button fastForwardButtonComponent = fastForwardButton.GetComponent<Button>();
        fastForwardButtonComponent.onClick.AddListener(OnFastForwardButtonClicked);

        Button playButtonComponent = playButton.GetComponent<Button>();
        playButtonComponent.onClick.AddListener(OnPlayButtonClicked);

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
        planetDistanceFromSunToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.AR);
        Debug.Log(planetDistanceFromSunToggleLayoutGroup.ToString());
        Debug.Log((localizationManager.GetCurrentLanguage() == Constants.AR));
        HorizontalLayoutGroup planetNameToggleLayoutGroup = planetNameToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
        planetNameToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.AR);

        HorizontalLayoutGroup planetInclinationLineToggleLayoutGroup = planetInclinationLineToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
        planetInclinationLineToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.AR);

        HorizontalLayoutGroup orbitLineToggleLayoutGroup = orbitLineToggle.transform.parent.GetComponent<HorizontalLayoutGroup>();
        orbitLineToggleLayoutGroup.reverseArrangement = (localizationManager.GetCurrentLanguage() == Constants.AR);

    }

    public void ReverseOrderIfArabic(HorizontalLayoutGroup layoutGroup)
    {
        if (localizationManager.GetCurrentLanguage() == Constants.AR)
        {
            layoutGroup.reverseArrangement = true;
        }
        else
        {
            layoutGroup.reverseArrangement = false;
        }
    }

    private void MenuTransistionInit()
    {
        sliderButtonToggleImage = sliderPanelToggleButton.transform.GetChild(0).gameObject;
        sliderPanelRectTransform = menuSliderPanel.GetComponent<RectTransform>();
        float halfHeight = menuSliderPanel.GetComponent<RectTransform>().rect.height / 2;

        // Set the height of the sliding panel to be half of the screen's height
        float screenHeight = Screen.height;
        float halfScreenHeight = screenHeight / 2;

        sliderPanelRectTransform.sizeDelta = new Vector2(sliderPanelRectTransform.sizeDelta.x, halfScreenHeight);
        sliderPanelRectTransform.anchoredPosition = new Vector2(sliderPanelRectTransform.anchoredPosition.x, -halfScreenHeight / 2);

        // Set the target position of the panel
        targetPosition = sliderPanelRectTransform.anchoredPosition + new Vector2(0f, halfScreenHeight);
        initialPosition = new Vector2(sliderPanelRectTransform.anchoredPosition.x, -halfScreenHeight / 2);

        // Add listener to the toggle button
        sliderPanelToggleButton.GetComponent<Button>().onClick.AddListener(ToggleMenuSliderPanel);
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

        OnUpdateTimeScaleSlider = new UnityAction<float>(UpdateTimeScale);
        OnUpdateSizeScaleSlider = new UnityAction<float>(UpdateSizeScale);
        OnUpdateDistanceScaleSlider = new UnityAction<float>(UpdateDistanceScale);

        timeScaleSlider.onValueChanged.AddListener(OnUpdateTimeScaleSlider);
        sizeScaleSlider.onValueChanged.AddListener(OnUpdateSizeScaleSlider);
        distanceScaleSlider.onValueChanged.AddListener(OnUpdateDistanceScaleSlider);

    }

    public void SetCelestialBodyData(CelestialBodyData celestialBodyData)
    {
        // Remove all previous items
        foreach (Transform child in planetInfoItemParent)
        {
            Destroy(child.gameObject);
        }

        // Using reflection to get all fields in the CelestialBodyData class
        FieldInfo[] fields = typeof(CelestialBodyData).GetFields();

        foreach (FieldInfo field in fields)
        {
            // Instantiate new data item
            GameObject newDataItem = Instantiate(planetInfoItemPrefab, planetInfoItemParent);

            // Assign field name to Text component
            TextMeshProUGUI textComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[0];
            textComponent.text = field.Name;

            // Assign field value to another Text component
            TextMeshProUGUI valueComponent = newDataItem.GetComponentsInChildren<TextMeshProUGUI>()[1];

            // Check for null value before trying to convert to string
            object fieldValue = field.GetValue(celestialBodyData);
            if (fieldValue != null)
            {
                valueComponent.text = fieldValue.ToString();
            }
            else
            {
                valueComponent.text = "null";
            }
        }
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

            // Assign planet name to Text component
            TextMeshProUGUI textComponent = newLegendItem.GetComponentInChildren<TextMeshProUGUI>();

            // Use localizationManager to get the localized planet name
            string localizedPlanetName = localizationManager.GetLocalizedValue(planet.Key, textComponent);

            // If the localized name is not found, fall back to the English name
            if (string.IsNullOrEmpty(localizedPlanetName))
            {
                localizedPlanetName = planet.Key;
            }

            textComponent.text = localizedPlanetName;

            // Assign color to Image component
            Image imageComponent = newLegendItem.GetComponentInChildren<Image>();
            imageComponent.color = planet.Value;

            HorizontalLayoutGroup layoutGroup = imageComponent.transform.parent.GetComponent<HorizontalLayoutGroup>();
            ReverseOrderIfArabic(layoutGroup);

        }
    }

    public void SetPlanetNameTextTitle(string text, bool showGameObjectHolder)
    {
        menuPlanetName.transform.parent.gameObject.SetActive(showGameObjectHolder);
        menuPlanetName.text = text;
        returnButton.SetActive(showGameObjectHolder);
    }

    public void SetMiddleIconsHelperText(string text)
    {
        middleIconsHelperText.text = text;
    }

    public void UpdateTimeScale(float value)
    {
        celestialBodyHandler.UpdateTimeScale(value); // Notify SolarSystemSimulationWithMoons

        // Similar conversion logic you have
        string timeText = TimeScaleConversion(value);

        menuTimeText.text = localizationManager.GetLocalizedValue("1_second_real_life_equals", menuTimeText, timeText.ToString());

    }

    public void UpdateSizeScale(float value)
    {
        celestialBodyHandler.UpdateSizeScale(value); // Notify SolarSystemSimulationWithMoons

        float realLifeSize = 1f / value;
        menuSizeText.text = localizationManager.GetLocalizedValue("1_meter_size_equals", menuSizeText, realLifeSize.ToString());

    }

    public void UpdateDistanceScale(float value)
    {
        celestialBodyHandler.UpdateDistanceScale(value); // Notify SolarSystemSimulationWithMoons

        float realLifeDistance = 1f / value;
        menuDistanceText.text = localizationManager.GetLocalizedValue("1_meter_distance_equals", menuDistanceText, realLifeDistance.ToString());
    }

    private string TimeScaleConversion(float timeScale)
    {
        float simulatedSeconds = timeScale;
        float simulatedMinutes = simulatedSeconds / 60;
        float simulatedHours = simulatedMinutes / 60;
        float simulatedDays = simulatedHours / 24;
        float simulatedWeeks = simulatedDays / 7;
        float simulatedMonths = simulatedDays / 30.44f; // A month is approximately 30.44 days on average
        float simulatedYears = simulatedDays / 365.25f; // A year is approximately 365.25 days considering leap years

        string timeText;

        if (simulatedYears >= 1)
        {
            timeText = $"{simulatedYears} {localizationManager.GetLocalizedValue("Years", menuTimeText)}";
        }
        else if (simulatedMonths >= 1)
        {
            timeText = $"{simulatedYears} {localizationManager.GetLocalizedValue("Months", menuTimeText)}";
        }
        else if (simulatedWeeks >= 1)
        {
            timeText = $"{simulatedYears} {localizationManager.GetLocalizedValue("Weeks", menuTimeText)}";
        }
        else if (simulatedDays >= 1)
        {
            timeText = $"{simulatedYears} {localizationManager.GetLocalizedValue("Days", menuTimeText)}";
        }
        else if (simulatedHours >= 1)
        {
            timeText = $"{simulatedYears} {localizationManager.GetLocalizedValue("Hours", menuTimeText)}";
        }
        else if (simulatedMinutes >= 1)
        {
            timeText = $"{simulatedYears} {localizationManager.GetLocalizedValue("Minutes", menuTimeText)}";
        }
        else
        {
            timeText = $"{simulatedYears} {localizationManager.GetLocalizedValue("Seconds", menuTimeText)}";
        }

        return timeText;
    }

    public void ToggleMenuSliderPanel()
    {
        isMenuPanelVisible = !isMenuPanelVisible;

        startRotation = sliderButtonToggleImage.transform.eulerAngles.z;
        endRotation = isMenuPanelVisible ? startRotation + 180 : startRotation - 180;

        if (isMenuPanelVisible)
        {
            StartCoroutine(TransitionPanel(initialPosition, targetPosition));
            if (middleIconsHelperText.transform.parent.gameObject.activeInHierarchy)
                middleIconsHelperText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(TransitionPanel(targetPosition, initialPosition));
        }
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


    public void UIShowInitial()
    {
        menuPlanetName.transform.parent.gameObject.SetActive(false);
        scanRoomIconObject.SetActive(true);
        SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Move_your_phone_to_start_scanning_the_room", middleIconsHelperText));
        tapIconObject.SetActive(false);
        returnButton.SetActive(false);
        menuSliderPanel.SetActive(false);
    }

    public void UIShowAfterScan()
    {
        scanRoomIconObject.SetActive(false);
        SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Tap_on_the_scanned_area_to_place_the_solar_system", middleIconsHelperText));
        tapIconObject.SetActive(true);
    }

    public void UIShowAfterClick()
    {
        scanRoomIconObject.SetActive(false);
        tapIconObject.SetActive(false);
        menuSliderPanel.SetActive(true);
        returnButton.SetActive(false);
        SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Click_on_any_planet_or_click_on_the_menu_below_to_display_more_settings", middleIconsHelperText));
    }

    public void ToggleSwipeIcon(bool toggleState)
    {
        SetMiddleIconsHelperText(localizationManager.GetLocalizedValue("Touch_and_drag_to_move_the_planet_Around", middleIconsHelperText));
        swipeIconObject.SetActive(toggleState);
        ToggleMiddleIconHelper(toggleState);
    }

    public void ToggleMiddleIconHelper(bool toggleState)
    {
        middleIconsHelperText.transform.parent.gameObject.SetActive(toggleState);
    }
}
