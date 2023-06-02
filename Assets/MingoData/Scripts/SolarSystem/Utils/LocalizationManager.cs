using System.Collections.Generic;
using UnityEngine;
using ArabicSupport;
using TMPro;
using System.Linq;

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
    public string GetLocalizedValue(string key, TextMeshProUGUI textComponent, bool centerText, params object[] formatArgs)
    {
        string result = missingTextString;

        // Debug statement
        Debug.Log("GetLocalizedValue: key = " + key);

        if (localizedText.ContainsKey(key))
        {
            switch (currentLanguage)
            {
                case "english":
                    result = string.Format(localizedText[key].english, formatArgs);
                    if (centerText) textComponent.alignment = TextAlignmentOptions.Midline;
                    else textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case "arabic":
                    result = string.Format(localizedText[key].arabic, formatArgs); 
                    result = ArabicFixer.Fix(result, true, true);
                    textComponent.isRightToLeftText = true;
                    result = new string(result.ToCharArray().Reverse().ToArray());
                    if (centerText) textComponent.alignment = TextAlignmentOptions.Midline;
                    else textComponent.alignment = TextAlignmentOptions.MidlineRight;
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
