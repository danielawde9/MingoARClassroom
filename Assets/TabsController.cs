using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabsController : MonoBehaviour
{
    public List<Button> tabButtons;
    public List<GameObject> tabPanels;

    public Color selectedButtonColor;
    public Color deselectedButtonColor;
    public Color selectedTextColor;
    public Color deselectedTextColor;

    private List<TextMeshProUGUI> tabTexts = new List<TextMeshProUGUI>();
    private int currentlySelectedTab = -1;

    private void Start()
    {
        selectedButtonColor = HexToColor("#7F8FA6");
        deselectedButtonColor = HexToColor("#DCDDE1");
        selectedTextColor = HexToColor("#F5F6FA");
        deselectedTextColor = HexToColor("#2F3640");

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(index));
        }

        TextMeshProUGUI textComponent = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            tabTexts.Add(textComponent);
        }
        else
        {
            Debug.LogError("No TextMeshProUGUI component found for button at index " + i);
        }
        if (tabPanels.Count > 0)
            ShowTab(0);
    }

    private void ShowTab(int index)
    {
        if (currentlySelectedTab != -1)
        {
            tabButtons[currentlySelectedTab].GetComponent<Image>().color = deselectedButtonColor;
            tabTexts[currentlySelectedTab].color = deselectedTextColor;
            tabPanels[currentlySelectedTab].SetActive(false);
        }

        tabButtons[index].GetComponent<Image>().color = selectedButtonColor;
        tabTexts[index].color = selectedTextColor;
        tabPanels[index].SetActive(true);

        currentlySelectedTab = index;
    }

    private Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}
