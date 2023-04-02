using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QuestionTypes;

[System.Serializable]
public class QuestionsList
{
    public List<Question> questions;

    [System.Serializable]
    public class Question
    {
        public string type;
        public string question;
        public List<string> answers;
        public int correctAnswerIndex;
    }
}

public class QuizManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private List<Button> answerButtons;
    [SerializeField] private Slider timerSlider;

    public float timerDuration = 30f;

    private float remainingTime;
    private List<QuestionsList.Question> questions;
    private int currentQuestionIndex;
    private int correctAnswerIndex;
    private List<TextMeshProUGUI> answerButtonTexts;

    private void Awake()
    {
        // Cache TextMeshProUGUI components of the answer buttons
        answerButtonTexts = new List<TextMeshProUGUI>();
        foreach (Button button in answerButtons)
        {
            answerButtonTexts.Add(button.GetComponentInChildren<TextMeshProUGUI>());
        }
    }
    private void Start()
    {
        LoadQuestions();
        DisplayQuestion();
        remainingTime = timerDuration;

        timerSlider.maxValue = timerDuration;
        timerSlider.value = timerDuration;
    }

    public void OnButtonClicked(Button button)
    {
        int clickedButtonIndex = answerButtons.IndexOf(button);
        CheckAnswer(clickedButtonIndex);
        MoveToNextQuestion();
    }

    private void LoadQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("questions");
        QuestionsList questionsList = JsonUtility.FromJson<QuestionsList>(jsonFile.text);
        questions = questionsList.questions;
    }

    private void DisplayQuestion()
    {
        QuestionsList.Question currentQuestion = questions[currentQuestionIndex];
        questionText.text = currentQuestion.question;
        correctAnswerIndex = currentQuestion.correctAnswerIndex;
        switch(currentQuestion.type)
        {
            case MULTIPLE_CHOICE:
                DisplayAnswers(currentQuestion.answers, 4);
                break;
            case TRUE_FALSE:
                DisplayAnswers(currentQuestion.answers, 2);
                break;
            default:
                Debug.LogError($"Unsupported question type: {currentQuestion.type}");
                break;
        }
    }

    private void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            timerSlider.value = remainingTime;
        }
        else if (remainingTime <= 0)
        {
            TimerFinished();

        }
    }
    private void TimerFinished()
    {
        Debug.Log("Time's up!");
        // Add your logic here, e.g., move to the next question or end the quiz
    }

    private void DisplayAnswers(List<string> answers, int numberOfButtons)
    {
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (i < numberOfButtons)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtonTexts[i].text = answers[i];
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void MoveToNextQuestion() { 
    
        currentQuestionIndex =(currentQuestionIndex+1)%questions.Count;
        DisplayQuestion();
    }

    private void CheckAnswer(int clickedButtonIndex)
    {
        if(clickedButtonIndex == correctAnswerIndex)
        {
            Debug.Log("Correct answer!");
        }
        else
        {
            Debug.Log("Incorrect answer.");
        }
    }
}
