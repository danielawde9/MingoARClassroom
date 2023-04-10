using UnityEngine;
using UnityEngine.UI;

public class SpeedFactorSlider : MonoBehaviour
{
    public CelestialBodyInitialVelocity celestialBodyInitialVelocity;
    public Slider speedFactorSlider;

    private void Start()
    {
        speedFactorSlider.onValueChanged.AddListener(OnSpeedFactorChanged);
    }

    private void OnSpeedFactorChanged(float value)
    {
        celestialBodyInitialVelocity.speedFactor = value;
    }
}
