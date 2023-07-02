using System.Collections.Generic;
using System.Linq;
using ArabicSupport;
using TMPro;
using UnityEngine;

namespace MingoData.Scripts.Utils
{

    [System.Serializable]
    public class LocalizationItem
    {
        public string key;
        public string english;
        public string arabic;
    }
    [System.Serializable]
    public class LocalizationData
    {
        public LocalizationItem[] items;
    }
    public class LocalizationManager : MonoBehaviour
    {
        public TextAsset localizationFile; // Reference to the JSON file

        private Dictionary<string, LocalizationItem> localizedText;
        private const string MissingTextString = "Localized text not found";
        private string currentLanguage;

        public void LoadLocalizedText()
        {
            if (localizationFile != null)
            {
                string dataAsJson = localizationFile.ToString();
                LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

                localizedText = new Dictionary<string, LocalizationItem>();

                foreach (LocalizationItem text in loadedData.items)
                {
                    localizedText.Add(text.key, text);
                }

                Debug.Log("Data loaded, dictionary contains: " + localizedText.Count + " entries");
            }
            else
            {
                Debug.LogError("Localization file not assigned!");
            }
        }
        
        public string GetLocalizedValue(string key, TMP_Text textComponent, bool centerText, Color valueColor, params object[] formatArgs)
        {
            string result = MissingTextString;

            // Convert the valueColor to hex
            string hexValueColor = ColorUtility.ToHtmlStringRGB(valueColor);

            if (localizedText.ContainsKey(key))
            {
                switch (currentLanguage)
                {
                    case Constants.LangEn:
                        // Add color to formatArgs
                        for (int i = 0; i < formatArgs.Length; i++)
                        {
                            formatArgs[i] = $"<color=#{hexValueColor}>{formatArgs[i]}</color>";
                        }
                        result = string.Format(localizedText[key].english, formatArgs);
                        if (textComponent != null)
                        {
                            textComponent.isRightToLeftText = false;
                            textComponent.alignment = centerText ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineLeft;
                        }
                        break;
                    case Constants.LangAR:
                        result = string.Format(localizedText[key].arabic, formatArgs);
                        //  NOTE: arabic coloring is not supported yet
                        result = ArabicFixer.Fix(result, true, true);
                        if (textComponent != null)
                        {
                            textComponent.isRightToLeftText = true;
                            textComponent.alignment = centerText ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineRight;
                        }
                        result = new string(result.ToCharArray().Reverse().ToArray());
                        break;
                }
            }
            else
            {
                Debug.Log("GetLocalizedValue: key not found in localizedText");
            }

            // If there are no formatArgs, colorize the entire string
            if (formatArgs.Length == 0)
            {
                result = $"<color=#{ColorUtility.ToHtmlStringRGB(Constants.ColorWhite)}>{result}</color>";
            }

            return result;
        }

        public string GetLocalizedTimeValue(float timeScale, TextMeshProUGUI textComponent, Color valueColor)
        {
            // Get time unit and time text
            (string timeUnitKey, string timeValue) = UtilsFns.TimeScaleConversion(timeScale);

            // Convert the valueColor to hex
            string hexValueColor = ColorUtility.ToHtmlStringRGB(valueColor);

            // Add color to timeValue

            string result = MissingTextString;

            // Check if there is a localized string for the given time unit
            if (!localizedText.ContainsKey(timeUnitKey))
                return result;

            switch (currentLanguage)
            {
                case Constants.LangEn:
                    timeValue = $"<color=#{hexValueColor}>{timeValue}</color>";
                    result = string.Format(localizedText[timeUnitKey].english, timeValue);
                    textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case Constants.LangAR:
                    // NOTE: arabic coloring is not supported yet 
                    result = string.Format(localizedText[timeUnitKey].arabic, timeValue);
                    result = ArabicFixer.Fix(result, true, true);
                    textComponent.isRightToLeftText = true;
                    textComponent.alignment = TextAlignmentOptions.MidlineRight;
                    result = new string(result.ToCharArray().Reverse().ToArray());
                    break;
            }

            return result;
        }

        public static string GetCurrentLanguage()
        {
            return currentLanguage;
        }

        public void SetLanguage(string language)
        {
            currentLanguage = language;
            PlayerPrefs.SetString(Constants.SelectedLanguage, currentLanguage);
        }
    }
}
