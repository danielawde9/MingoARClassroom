using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIHandler : BasePressInputHandler
{
    public SolarSystemSimulationWithMoons celestialBodyHandler;

    public GameObject returnButton;
    public GameObject pauseButton;
    public GameObject fastForwardButton;
    public GameObject playButton;

    public UnityAction<float> OnUpdateTimeScaleSlider;
    public UnityAction<float> OnUpdateSizeScaleSlider;
    public UnityAction<float> OnUpdateDistanceScaleSlider;

    public GameObject legendItemPrefab;
    public Transform legendParent;

    public GameObject planetInfoItemPrefab;
    public Transform planetInfoItemParent;
        
    public Slider timeScaleSlider;
    public Slider sizeScaleSlider;
    public Slider distanceScaleSlider;

    public TextMeshProUGUI menuTimeText;
    public TextMeshProUGUI menuSizeText;
    public TextMeshProUGUI menuDistanceText;
    public TextMeshProUGUI menuPlanetName;
    public TextMeshProUGUI middleIconsHelperText;

    public GameObject menuSliderPanel;
    public GameObject scanRoomIconObject;
    public GameObject tapIconObject;
    public GameObject swipeIconObject;

    [HideInInspector]
    public UnityEvent<bool> onPlanetNameToggleValueChanged;
    [HideInInspector]
    public UnityEvent<bool> onOrbitLineToggleValueChanged;
    [HideInInspector]
    public UnityEvent<bool> onPlanetInclinationLineToggleValueChanged;
    [HideInInspector]
    public UnityEvent<bool> onDistanceFromSunToggleValueChanged;
    public Toggle planetNameToggle;
    public Toggle orbitLineToggle;
    public Toggle planetInclinationLineToggle;
    public Toggle planetDistanceFromSunToggle;

    public GameObject sliderButtonToggleImage;

    public RectTransform sliderPanelRectTransform;
    public Button sliderPanelToggleButton;
    public float sliderPanelTransitionDuration = 1f;
    private Vector2 initialPosition;
    private Vector2 targetPosition;
    public bool isMenuPanelVisible = false;
    float startRotation;
    float endRotation;

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

    private void Start()
    {
        timeScaleSlider.value = Constants.initialTimeScale;
        sizeScaleSlider.value = Constants.initialSizeScale;
        distanceScaleSlider.value = Constants.initialDistanceScale;

        timeScaleSlider.minValue = Constants.initialTimeScale;
        sizeScaleSlider.minValue = Constants.initialSizeScale;
        distanceScaleSlider.minValue = Constants.minDistance;

        menuTimeText.text = $"1 second in real life equals {Constants.initialTimeScale} second in the simulated solar system.";
        menuSizeText.text = $"1 meter size in the simulated solar system equals {1 / Constants.initialSizeScale} kilometer in real life.";
        menuDistanceText.text = $"1 meter distance in the simulated solar system equals {1 / Constants.initialDistanceScale} kilometer in real life.";

        timeScaleSlider.maxValue = Constants.maxTime;
        sizeScaleSlider.maxValue = Constants.maxSize;
        distanceScaleSlider.maxValue = Constants.maxDistance;

        OnUpdateTimeScaleSlider = new UnityAction<float>(UpdateTimeScale);
        OnUpdateSizeScaleSlider = new UnityAction<float>(UpdateSizeScale);
        OnUpdateDistanceScaleSlider = new UnityAction<float>(UpdateDistanceScale);

        timeScaleSlider.onValueChanged.AddListener(OnUpdateTimeScaleSlider);
        sizeScaleSlider.onValueChanged.AddListener(OnUpdateSizeScaleSlider);
        distanceScaleSlider.onValueChanged.AddListener(OnUpdateDistanceScaleSlider);


        float halfHeight = sliderPanelRectTransform.rect.height / 2;

        // Set the height of the sliding panel to be half of the screen's height
        float screenHeight = Screen.height;
        float halfScreenHeight = screenHeight / 2;

        sliderPanelRectTransform.sizeDelta = new Vector2(sliderPanelRectTransform.sizeDelta.x, halfScreenHeight);
        sliderPanelRectTransform.anchoredPosition = new Vector2(sliderPanelRectTransform.anchoredPosition.x, -halfScreenHeight);

        // Set the target position of the panel
        targetPosition = sliderPanelRectTransform.anchoredPosition + new Vector2(0f, halfScreenHeight);

        // Add listener to the toggle button
        sliderPanelToggleButton.onClick.AddListener(ToggleMenuSliderPanel);

        Button returnButtonComponent = returnButton.GetComponent<Button>();
        returnButtonComponent.onClick.AddListener(OnReturnButtonClick);

        Button pauseButtonComponent = pauseButton.GetComponent<Button>();
        pauseButtonComponent.onClick.AddListener(OnPauseButtonClicked);

        Button fastForwardButtonComponent = fastForwardButton.GetComponent<Button>();
        fastForwardButtonComponent.onClick.AddListener(OnFastForwardButtonClicked);

        Button playButtonComponent = playButton.GetComponent<Button>();
        playButtonComponent.onClick.AddListener(OnPlayButtonClicked);

        // NOTE: Make sure to turn of the parent also from inspector weird bug 
        orbitLineToggle.transform.parent.gameObject.SetActive(false);
        planetNameToggle.transform.parent.gameObject.SetActive(false);
        planetInclinationLineToggle.transform.parent.gameObject.SetActive(false);
        planetDistanceFromSunToggle.transform.parent.gameObject.SetActive(false);

        orbitLineToggle.isOn = true;
        planetNameToggle.isOn = false;
        planetInclinationLineToggle.isOn = false;  
        planetDistanceFromSunToggle.isOn = false;

        planetDistanceFromSunToggle.onValueChanged.AddListener((isOn) => { onDistanceFromSunToggleValueChanged?.Invoke(isOn); });
        orbitLineToggle.onValueChanged.AddListener((isOn) => { onOrbitLineToggleValueChanged?.Invoke(isOn); });
        planetNameToggle.onValueChanged.AddListener((isOn) => { onPlanetNameToggleValueChanged?.Invoke(isOn); });
        planetInclinationLineToggle.onValueChanged.AddListener((isOn) => { onPlanetInclinationLineToggleValueChanged?.Invoke(isOn); });


        orbitLineToggle.transform.parent.gameObject.SetActive(true);
        planetNameToggle.transform.parent.gameObject.SetActive(true);
        planetInclinationLineToggle.transform.parent.gameObject.SetActive(true);
        planetDistanceFromSunToggle.transform.parent.gameObject.SetActive(true);


    }

    public void DisplayCelestialBodyData(CelestialBodyData celestialBodyData)
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
    // todo set the font and aligment and incase no planet is selected default text

    public void DisplayPlanetColorLegend(Dictionary<string, Color> planetColorLegend)
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
            textComponent.text = planet.Key;

            // Assign color to Image component
            Image imageComponent = newLegendItem.GetComponentInChildren<Image>();
            imageComponent.color = planet.Value;
        }
    }



    public void UpdateTimeScale(float value)
    {
        celestialBodyHandler.UpdateTimeScale(value); // Notify SolarSystemSimulationWithMoons

        // Similar conversion logic you have
        string timeText = TimeScaleConversion(value);
        menuTimeText.text = $"1 second in real life equals {timeText} in the simulated solar system.";
    }

    public void UpdateSizeScale(float value)
    {
        celestialBodyHandler.UpdateSizeScale(value); // Notify SolarSystemSimulationWithMoons

        float realLifeSize = 1f / value;
        menuSizeText.text = $"1 meter size in the simulated solar system equals {realLifeSize} kilometer in real life.";
    }

    public void UpdateDistanceScale(float value)
    {
        celestialBodyHandler.UpdateDistanceScale(value); // Notify SolarSystemSimulationWithMoons

        float realLifeDistance = 1f / value;
        menuDistanceText.text = $"1 meter distance in the simulated solar system equals {realLifeDistance} kilometer in real life.";
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
            timeText = $"{simulatedYears} years";
        }
        else if (simulatedMonths >= 1)
        {
            timeText = $"{simulatedMonths} months";
        }
        else if (simulatedWeeks >= 1)
        {
            timeText = $"{simulatedWeeks} weeks";
        }
        else if (simulatedDays >= 1)
        {
            timeText = $"{simulatedDays} days";
        }
        else if (simulatedHours >= 1)
        {
            timeText = $"{simulatedHours} hours";
        }
        else if (simulatedMinutes >= 1)
        {
            timeText = $"{simulatedMinutes} minutes";
        }
        else
        {
            timeText = $"{simulatedSeconds} seconds";
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

    public void SetPlanetNameTextTitle(string text)
    {
        menuPlanetName.transform.parent.gameObject.SetActive(true);
        menuPlanetName.text = text;
    }

    public void SetPlanetInfo(string text)
    {
        menuPlanetName.transform.parent.gameObject.SetActive(true);
        menuPlanetName.text = text;
    }

    public void SetMiddleIconsHelperText(string text)
    {
        middleIconsHelperText.text = text;
    }


    public void UIShowInitial()
    {
        menuPlanetName.transform.parent.gameObject.SetActive(false);
        scanRoomIconObject.SetActive(true);
        SetMiddleIconsHelperText("Move your phone to start scanning the room");
        tapIconObject.SetActive(false);
        returnButton.SetActive(false);
        menuSliderPanel.SetActive(false);
    }

    public void UIShowAfterScan()
    {
        scanRoomIconObject.SetActive(false);
        SetMiddleIconsHelperText("Tap on the scanned area to place the solar system ");
        tapIconObject.SetActive(true);
    }

    public void UIShowAfterClick()
    {
        scanRoomIconObject.SetActive(false);
        tapIconObject.SetActive(false);
        menuSliderPanel.SetActive(true);
        returnButton.SetActive(true);
        SetMiddleIconsHelperText("Click on any planet or click on the menu below to display more settings");
    }
    public void ToggleSwipeIcon(bool toggleState)
    {
        SetMiddleIconsHelperText("Touch and drag to move the planet Around");
        swipeIconObject.SetActive(toggleState);
        ToggleMiddleIconHelper(toggleState);
    }

    public void ToggleMiddleIconHelper(bool toggleState)
    {
        middleIconsHelperText.transform.parent.gameObject.SetActive(toggleState);
    }
}
