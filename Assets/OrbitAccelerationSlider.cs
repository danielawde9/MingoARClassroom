using UnityEngine;
using UnityEngine.UI;

public class OrbitAccelerationSlider : MonoBehaviour
{
    public Slider orbitAccelerationSlider;
    private PlanetController[] planetControllers;

    private void Start()
    {
        planetControllers = FindObjectsOfType<PlanetController>();
        orbitAccelerationSlider.onValueChanged.AddListener(UpdateOrbitAcceleration);
    }

    private void UpdateOrbitAcceleration(float value)
    {
        foreach (PlanetController planetController in planetControllers)
        {
            planetController.OrbitAccelerationMultiplier(value);
        }
    }
}
