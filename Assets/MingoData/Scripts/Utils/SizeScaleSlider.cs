using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeScaleSlider : MonoBehaviour
{
    public SolarSystemSimulation solarSystemSimulation;
    public Slider sizeScaleSlider;

    private void Start()
    {
        sizeScaleSlider.onValueChanged.AddListener(UpdateSimulationSizeScale);
    }

    private void UpdateSimulationSizeScale(float value)
    {
        solarSystemSimulation.sizeScale = value;
        foreach (var planet in solarSystemSimulation.planetDataList.planets)
        {
            solarSystemSimulation.UpdatePlanetScale(planet);
        }
    }
}
