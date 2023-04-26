using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question
{
    //public string Id { get; set; }
    public string QuestionText { get; set; }

    public string[] Answers { get; set; }

    public int UserScore { get; set; }
    public Question() { }
    public Question( string questionText, string[] answers, int userScore)
    {
        this.QuestionText = questionText;
        Shuffle(answers);
        this.Answers = answers;
        this.UserScore = userScore;
    }

    private void Shuffle(string[] answers)
    {
        System.Random random = new();
        for (int i = answers.Length - 1; i >= 0; i--)
        {
            int j = random.Next(i + 1);
            /*string temp = answers[j];
            answers[j] = answers[i];
            answers[i] = temp;*/
            (answers[i], answers[j]) = (answers[j], answers[i]);
        }
    }
}

[System.Serializable]
public class QuestionData
{
    [SerializeField]
    public string questionText;
    [SerializeField]
    public string answers;
    [SerializeField]
    public int userScore;
}
