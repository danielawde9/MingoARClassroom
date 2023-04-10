using UnityEngine;
using UnityEngine.UI;

public class GravitationalConstantSlider : MonoBehaviour
{
    public SolarSystemPhysics solarSystemPhysics;
    public Slider gravitationalConstantSlider;

    private void Start()
    {
        gravitationalConstantSlider.minValue= 6.674f * Mathf.Pow(10, -7); 
        gravitationalConstantSlider.maxValue= 6.674f * Mathf.Pow(10, -6);
        gravitationalConstantSlider.onValueChanged.AddListener(OnGravitationalConstantChanged);
    }

    private void OnGravitationalConstantChanged(float value)
    {
        solarSystemPhysics.UpdateGravitationalConstant(value);
    }
}
