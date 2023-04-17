using UnityEngine;
using UnityEngine.UI;

public class TimeScaleSlider : MonoBehaviour
{
    public SolarSystemSimulation solarSystemSimulation;
    public Slider timeScaleSlider;

    private void Start()
    {
        timeScaleSlider.onValueChanged.AddListener(UpdateSimulationTimeScale);
    }

    private void UpdateSimulationTimeScale(float value)
    {
        solarSystemSimulation.timeScale = value;
    }
}
