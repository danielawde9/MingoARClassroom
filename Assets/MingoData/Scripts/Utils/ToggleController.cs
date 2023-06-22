using UnityEngine;
using UnityEngine.UI;
namespace MingoData.Scripts.Utils
{
    public class ToggleController : MonoBehaviour
    {
        public Toggle toggle;
        public RectTransform knobTransform;
        public float speed = 5f;
        [SerializeField]
        private LocalizationManager localizationManager;

        private Vector2 onPosition;
        private Vector2 offPosition;

        private void Start()
        {
        
            // Calculate the on and off positions
            float knobWidth = knobTransform.rect.width;
            float toggleWidth = toggle.GetComponent<RectTransform>().rect.width;
            const float offset = 5f; // Adjust the offset value as per your requirement

            // If language is Arabic, swap the positions
            if (localizationManager.GetCurrentLanguage() == Constants.LangAR)
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
            toggle.GetComponent<Image>().color = isOn ? Constants.ColorGreen : Constants.ColorDarkGrey;
        }
        

    }

}

