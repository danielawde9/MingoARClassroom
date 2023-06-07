using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI userScoreText;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject playerScoreList;
    [SerializeField] private GameObject playerScorePrefab;

    private Dictionary<int, TextMeshProUGUI> playerScoreTexts = new Dictionary<int, TextMeshProUGUI>();


    public void UpdateTextInstructions(string newText)
    {
        instructionsText.text = newText;
    }

    public void UpdateUserScore(int newScore)
    {
        userScoreText.text = $"Score: {newScore}";
    }

    public void UpdateProgress(float progress)
    {
        progressBar.fillAmount = progress;
    }

    public void UpdatePlayerScore(int playerId, int newScore)
    {
        if(playerScoreTexts.ContainsKey(playerId))
        {
            playerScoreTexts[playerId].text = $"Player {playerId}: {newScore}";
        } else
        {
            GameObject newPlayerScore = Instantiate(playerScorePrefab, playerScoreList.transform);
            TextMeshProUGUI newPlayerScoreText = newPlayerScore.GetComponent<TextMeshProUGUI>();
            newPlayerScoreText.text = $"Player {playerId}: {newScore}";
            playerScoreTexts.Add(playerId, newPlayerScoreText);

        }

        SortPlayerScoreList();
    }

    private void SortPlayerScoreList()
    {
        List<KeyValuePair<int, TextMeshProUGUI>> sortedList = new List<KeyValuePair<int, TextMeshProUGUI>>(playerScoreTexts);
        sortedList.Sort((x,y)=> GetScoreFromText(y.Value.text).CompareTo(GetScoreFromText(x.Value.text)));
        for(int i=0;i<sortedList.Count;i++)
        {
            sortedList[i].Value.transform.SetSiblingIndex(i);
        }
    }

    private int GetScoreFromText(string text) {
        string[] parts = text.Split(' ');
        return int.Parse(parts[parts.Length -1 ]);
    }
}
