using System.Collections.Generic;
using UnityEngine;
using ArabicSupport;
using TMPro;

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
    private string currentLanguage = Constants.AR;

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

    public string GetLocalizedValue(string key, TextMeshProUGUI textComponent , params object[] formatArgs)
    {
        string result = missingTextString;
        if (localizedText.ContainsKey(key))
        {
            switch (currentLanguage)
            {
                case "english":
                    result = string.Format(localizedText[key].english, formatArgs);
                    textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case "arabic":
                    result = string.Format(localizedText[key].arabic, formatArgs);
                    result = ArabicFixer.Fix(result, true, true);
                    textComponent.alignment = TextAlignmentOptions.MidlineRight;

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
