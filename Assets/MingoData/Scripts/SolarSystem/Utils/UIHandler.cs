using System.Collections;
using TMPro;
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


    public Slider timeScaleSlider;
    public Slider sizeScaleSlider;
    public Slider distanceScaleSlider;

    public TextMeshProUGUI menuTimeText;
    public TextMeshProUGUI menuSizeText;
    public TextMeshProUGUI menuDistanceText;
    public TextMeshProUGUI menuTitleText;

    public RectTransform sliderPanelRectTransform;
    public Button sliderPanelToggleButton;
    public float sliderPanelTransitionDuration = 1f;

    private Vector2 initialPosition;
    private Vector2 targetPosition;
    public bool isMenuPanelVisible = false;

    float startRotation;
    float endRotation;
    public GameObject buttonImage;

    public void OnPauseButtonClicked()
    {
        celestialBodyHandler.timeScale = 0;
        UpdateTimeScale(celestialBodyHandler.timeScale);
    }

    public void OnFastForwardButtonClicked()
    {
        celestialBodyHandler.timeScale *= 2; // double the speed
        UpdateTimeScale(celestialBodyHandler.timeScale);
    }

    public void OnPlayButtonClicked()
    {
        celestialBodyHandler.timeScale = 1; // reset to real-time
        UpdateTimeScale(celestialBodyHandler.timeScale);
    }

    public void OnReturnButtonClick()
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
        sliderPanelRectTransform.anchoredPosition = new Vector2(sliderPanelRectTransform.anchoredPosition.x, -halfHeight);

        initialPosition = sliderPanelRectTransform.anchoredPosition;
        sliderPanelToggleButton.onClick.AddListener(TogglePanel);
        targetPosition = initialPosition + new Vector2(0f, sliderPanelRectTransform.rect.height);
    }

    public void UpdateTimeScale(float value)
    {
        celestialBodyHandler.UpdateTimeScaleSlider(value); // Notify SolarSystemSimulationWithMoons

        // Similar conversion logic you have
        string timeText = TimeScaleConversion(value);
        menuTimeText.text = $"1 second in real life equals {timeText} in the simulated solar system.";
    }

    public void UpdateSizeScale(float value)
    {
        celestialBodyHandler.UpdateSizeScaleSlider(value); // Notify SolarSystemSimulationWithMoons

        float realLifeSize = 1f / value;
        menuSizeText.text = $"1 meter size in the simulated solar system equals {realLifeSize} kilometer in real life.";
    }

    public void UpdateDistanceScale(float value)
    {
        celestialBodyHandler.UpdateDistanceScaleSlider(value); // Notify SolarSystemSimulationWithMoons

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

    public void TogglePanel()
    {
        isMenuPanelVisible = !isMenuPanelVisible;

        startRotation = buttonImage.transform.eulerAngles.z;
        endRotation = isMenuPanelVisible ? startRotation + 180 : startRotation - 180;

        if (isMenuPanelVisible)
        {
            StartCoroutine(TransitionPanel(initialPosition, targetPosition));
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
            buttonImage.transform.eulerAngles = new Vector3(0, 0, currentRotation);


            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sliderPanelRectTransform.anchoredPosition = endPosition;

        // Ensure the rotation finishes exactly at the end rotation
        buttonImage.transform.eulerAngles = new Vector3(0, 0, endRotation);

    }

    public void SetMenuTextTitle(string text)
    {
        menuTitleText.text= text;
    }

}
