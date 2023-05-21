using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    public Toggle toggle;
    public RectTransform knobTransform;
    public float speed = 5f;

    private Vector2 onPosition;
    private Vector2 offPosition;

    private void Start()
    {
        // Calculate the on and off positions
        float knobWidth = knobTransform.rect.width;
        float toggleWidth = toggle.GetComponent<RectTransform>().rect.width;
        onPosition = new Vector2(toggleWidth / 2 - knobWidth / 2, 0);
        offPosition = new Vector2(-toggleWidth / 2 + knobWidth / 2, 0);

        // Add listener for when the value changes
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void Update()
    {
        // Smoothly move the knob to the target position
        Vector2 targetPosition = toggle.isOn ? onPosition : offPosition;
        knobTransform.anchoredPosition = Vector2.Lerp(knobTransform.anchoredPosition, targetPosition, speed * Time.deltaTime);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        // Change the color of the background based on the new value
        toggle.GetComponent<Image>().color = isOn ? new Color(76, 209, 55) : new Color(220, 221, 225);
    }
}
