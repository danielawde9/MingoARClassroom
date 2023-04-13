using UnityEngine;
using UnityEngine.UI;

public class ScaleController : MonoBehaviour
{
    public Slider scaleSlider;

    private PlanetController[] planetControllers;
    private Vector3[] initialScales;

    private void Start()
    {
        planetControllers = FindObjectsOfType<PlanetController>();

        // Save the initial scale of each planet
        initialScales = new Vector3[planetControllers.Length];
        for (int i = 0; i < planetControllers.Length; i++)
        {
            initialScales[i] = planetControllers[i].transform.localScale;
        }

        scaleSlider.onValueChanged.AddListener(OnScaleValueChanged);
    }

    private void OnScaleValueChanged(float value)
    {
        for (int i = 0; i < planetControllers.Length; i++)
        {
            Transform planetTransform = planetControllers[i].transform;
            planetTransform.localScale = initialScales[i] * value;
        }
    }
}
