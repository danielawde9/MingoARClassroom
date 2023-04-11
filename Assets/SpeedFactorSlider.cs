using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpeedFactorSlider : MonoBehaviour
{
    //public List<CelestialBodyInitialVelocity> celestialBodyInitialVelocities;
    public Slider speedFactorSlider;

    private void Start()
    {
        speedFactorSlider.minValue= 9000;
        speedFactorSlider.maxValue= 10000;

        speedFactorSlider.onValueChanged.AddListener(OnSpeedFactorChanged);
    }

    private void OnSpeedFactorChanged(float value)
    {
        //foreach (var celestialBodyInitialVelocity in celestialBodyInitialVelocities)
            //celestialBodyInitialVelocity.speedFactor = value;
    }
}
