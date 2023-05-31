using System.Collections.Generic;
using UnityEngine;
using ArabicSupport;

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
    private string missingTextString = "Localized text not found";
    private string currentLanguage = "english";

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

    public string GetLocalizedValue(string key, params object[] formatArgs)
    {
        string result = missingTextString;
        if (localizedText.ContainsKey(key))
        {
            switch (currentLanguage)
            {
                case "english":
                    Debug.Log("English format string: " + localizedText[key].english);
                    Debug.Log("English format args: " + string.Join(", ", formatArgs));
                    result = string.Format(localizedText[key].english, formatArgs);
                    break;
                case "arabic":
                    Debug.Log("Arabic format string before fix: " + localizedText[key].arabic);
                    Debug.Log("Arabic format args: " + string.Join(", ", formatArgs));
                    result = string.Format(localizedText[key].arabic, formatArgs);
                    result = ArabicFixer.Fix(result, true, true);
                    break;
            }
        }

        return result;
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
