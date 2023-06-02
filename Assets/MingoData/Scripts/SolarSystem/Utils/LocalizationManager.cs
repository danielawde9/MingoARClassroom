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
    private readonly string missingTextString = "Localized text not found";
     private string currentLanguage = Constants.Lang_AR;
    //private string currentLanguage = Constants.Lang_EN;

    public void LoadLocalizedText()
    {
        if (localizationFile != null)
        {
            string dataAsJson = localizationFile.ToString();
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

            localizedText = new Dictionary<string, LocalizationItem>();

            for (int i = 0; i < loadedData.items.Length; i++)
            {
                localizedText.Add(loadedData.items[i].key, loadedData.items[i]);
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
                        if (centerText) textComponent.alignment = TextAlignmentOptions.Midline;
                        else textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    }
                    break;
                case Constants.Lang_AR:
                    result = string.Format(localizedText[key].arabic, formatArgs);
                    result = ArabicFixer.Fix(result, true, true);
                    if (textComponent != null)
                    {
                        textComponent.isRightToLeftText = true;
                        if (centerText) textComponent.alignment = TextAlignmentOptions.Midline;
                        else textComponent.alignment = TextAlignmentOptions.MidlineRight;
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
        if (timeScale <= 1)
        {
            return ("1_second_real_life_equals_seconds", (timeScale).ToString("F2"));
        }
        else if (timeScale < 60)
        {
            return ("1_second_real_life_equals_minutes", (timeScale).ToString("F2"));
        }
        else if (timeScale < 3600)
        {
            return ("1_second_real_life_equals_hours", (timeScale / 60).ToString("F2"));
        }
        else if (timeScale < 86400)
        {
            return ("1_second_real_life_equals_days", (timeScale / 3600).ToString("F2"));
        }
        else if (timeScale < 604800)
        {
            return ("1_second_real_life_equals_weeks", (timeScale / 86400).ToString("F2"));
        }
        else if (timeScale < 2629800)
        {
            return ("1_second_real_life_equals_months", (timeScale / 604800).ToString("F2"));
        }
        else
        {
            return ("1_second_real_life_equals_years", (timeScale / 2629800).ToString("F2"));
        }
    }

    public string GetLocalizedTimeValue(float timeScale, TextMeshProUGUI textComponent, bool centerText)
    {
        // Get time unit and time text
        var (timeUnitKey, timeValue) = TimeScaleConversion(timeScale);

        string result = missingTextString;

        // Check if there is a localized string for the given time unit
        if (localizedText.ContainsKey(timeUnitKey))
        {
            switch (currentLanguage)
            {
                case "english":
                    result = string.Format(localizedText[timeUnitKey].english, timeValue);
                    if (textComponent != null)
                    {
                        if (centerText) textComponent.alignment = TextAlignmentOptions.Midline;
                        else textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    }
                    break;
                case "arabic":
                    result = string.Format(localizedText[timeUnitKey].arabic, timeValue);
                    result = ArabicFixer.Fix(result, true, true);
                    if (textComponent != null)
                    {
                        textComponent.isRightToLeftText = true;
                        if (centerText) textComponent.alignment = TextAlignmentOptions.Midline;
                        else textComponent.alignment = TextAlignmentOptions.MidlineRight;
                    }
                    result = new string(result.ToCharArray().Reverse().ToArray());
                    break;
            }
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
