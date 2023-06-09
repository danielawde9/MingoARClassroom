using ArabicSupport;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
    private bool isReady = false;
    private const string missingTextString = "Localized text not found";
    private string currentLanguage = Constants.Lang_AR;
    //private string currentLanguage = Constants.Lang_EN;

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
            isReady = true;
        }
        else
        {
            Debug.LogError("Localization file not assigned!");
        }
    }
    public string GetLocalizedValue(string key, TMP_Text textComponent, bool centerText, params object[] formatArgs)
    {
        string result = missingTextString;

        // Debug statement
        Debug.Log("GetLocalizedValue: key = " + key);

        if (localizedText.ContainsKey(key))
        {
            switch (currentLanguage)
            {
                case Constants.Lang_EN:
                    result = string.Format(localizedText[key].english, formatArgs);
                    if (textComponent != null)
                    {
                        textComponent.isRightToLeftText = false;
                        textComponent.alignment = centerText ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineLeft;
                    }
                    break;
                case Constants.Lang_AR:
                    result = string.Format(localizedText[key].arabic, formatArgs);
                    result = ArabicFixer.Fix(result, true, true);
                    if (textComponent != null)
                    {
                        textComponent.isRightToLeftText = true;
                        textComponent.alignment = centerText ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineRight;
                    }
                    result = new string(result.ToCharArray().Reverse().ToArray());
                    break;
            }

            // Debug statement
            Debug.Log("GetLocalizedValue: result = " + result);
        }
        else
        {
            // Debug statement
            Debug.Log("GetLocalizedValue: key not found in localizedText");
        }

        return result;
    }
    private (string timeUnitKey, string timeValue) TimeScaleConversion(float timeScale)
    {
        return timeScale switch
        {
            <= 1 => ("1_second_real_life_equals_seconds", (timeScale).ToString("F2")),
            < 60 => ("1_second_real_life_equals_minutes", (timeScale).ToString("F2")),
            < 3600 => ("1_second_real_life_equals_hours", (timeScale / 60).ToString("F2")),
            < 86400 => ("1_second_real_life_equals_days", (timeScale / 3600).ToString("F2")),
            < 604800 => ("1_second_real_life_equals_weeks", (timeScale / 86400).ToString("F2")),
            < 2629800 => ("1_second_real_life_equals_months", (timeScale / 604800).ToString("F2")),
            _ => ("1_second_real_life_equals_years", (timeScale / 2629800).ToString("F2"))
        };
    }

    public string GetLocalizedTimeValue(float timeScale, TextMeshProUGUI textComponent, bool centerText)
    {
        // Get time unit and time text
        (string timeUnitKey, string timeValue) = TimeScaleConversion(timeScale);

        string result = missingTextString;

        // Check if there is a localized string for the given time unit
        if (!localizedText.ContainsKey(timeUnitKey))
            return result;
        switch (currentLanguage)
        {
            case "english":
                result = string.Format(localizedText[timeUnitKey].english, timeValue);
                if (textComponent != null)
                {
                    textComponent.alignment = centerText ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineLeft;
                }
                break;
            case "arabic":
                result = string.Format(localizedText[timeUnitKey].arabic, timeValue);
                result = ArabicFixer.Fix(result, true, true);
                if (textComponent != null)
                {
                    textComponent.isRightToLeftText = true;
                    textComponent.alignment = centerText ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineRight;
                }
                result = new string(result.ToCharArray().Reverse().ToArray());
                break;
        }

        return result;
    }



    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
    }
    public bool GetIsReady()
    {
        return isReady;
    }
}
