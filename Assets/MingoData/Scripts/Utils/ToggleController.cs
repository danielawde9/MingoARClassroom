using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    public Toggle toggle;
    public RectTransform knobTransform;
    public float speed = 5f;
    public LocalizationManager localizationManager;

    private Vector2 onPosition;
    private Vector2 offPosition;
    public string onColorHex = "#4CD137"; 
    public string offColorHex = "#2F3640";

    private void Start()
    {
        
        // Calculate the on and off positions
        float knobWidth = knobTransform.rect.width;
        float toggleWidth = toggle.GetComponent<RectTransform>().rect.width;
        const float offset = 5f; // Adjust the offset value as per your requirement

        // If language is Arabic, swap the positions
        if (localizationManager.GetCurrentLanguage() == Constants.Lang_AR)
        {
            onPosition = new Vector2((-toggleWidth / 2 + knobWidth / 2) + offset, 0);
            offPosition = new Vector2((toggleWidth / 2 - knobWidth / 2) - offset, 0);
        }
        else
        {
            onPosition = new Vector2((toggleWidth / 2 - knobWidth / 2) - offset, 0);
            offPosition = new Vector2((-toggleWidth / 2 + knobWidth / 2) + offset, 0);
        }

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
        toggle.GetComponent<Image>().color = isOn ? HexToColor(onColorHex) : HexToColor(offColorHex);
    }
    private static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");
        hex = hex.Replace("#", "");
        byte a = 255;
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        // Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

}

