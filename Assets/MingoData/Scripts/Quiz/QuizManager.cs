using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QuestionTypes;



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
        // If the submit button is clicked and the question type is GLOBE_QUESTION, call OnSubmitButtonClicked()
        //if (clickedButtonIndex == 3 && questions[currentQuestionIndex].type == GLOBE_QUESTION)
        //{
        //    CheckGlobeAnswer();
        //    MoveToNextQuestion();

        //}
        //else
        //{
        //}

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

        switch (currentQuestion.type)
        {
            case MULTIPLE_CHOICE:
                DisplayAnswers(currentQuestion.answers, 4);
                break;
            case TRUE_FALSE:
                DisplayAnswers(currentQuestion.answers, 2);
                break;
            //case GLOBE_QUESTION:
            //    break;
            default:
                Debug.LogError($"Unsupported question type: {currentQuestion.type}");
                MoveToNextQuestion();
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
        MoveToNextQuestion();
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

    private void MoveToNextQuestion()
    {
        if (currentQuestionIndex < questions.Count - 1)
        {
            currentQuestionIndex++;
            DisplayQuestion();
            remainingTime = timerDuration;
            timerSlider.value = timerDuration;
        }
        else
        {
            Debug.Log("Quiz completed");
            // Implement quiz completion logic
        }
    }

    private void CheckAnswer(int clickedButtonIndex)
    {
        if (clickedButtonIndex == correctAnswerIndex)
        {
            Debug.Log("Correct answer!");
        }
        else
        {
            Debug.Log("Incorrect answer.");
        }
    }
}


//   public void CheckGlobeAnswer()
//{
//    CountryClickHandler countryClickHandler = FindObjectOfType<CountryClickHandler>();

//    QuestionsList.Question currentQuestion = questions[currentQuestionIndex];

//    // Check if the selected country is the correct answer
//    if (countryClickHandler.SelectedCountryName == currentQuestion.targetCountryName)
//    {
//        Debug.Log("Correct answer!");
//        // Increase the score and show the correct feedback (e.g., change color, show a message, etc.)
//    }
//    else
//    {
//        Debug.Log("Wrong answer!");
//        // Show the wrong feedback (e.g., change color, show a message, etc.)
//    }


//    // Reset the SelectedCountryName
//    countryClickHandler.SelectedCountryName = null;
//    //// Add a delay before moving to the next question
//    //StartCoroutine(ProceedToNextQuestion());
//}

//private IEnumerator GlobeQuestionCoroutine(QuestionsList.Question question)
//{
//    // Set the question text
//    questionText.text = question.question;

//    // Enable the CountryClickHandler script and disable the ARQuizDashboardAnchorPlacer
//    CountryClickHandler countryClickHandler = FindObjectOfType<CountryClickHandler>();
//    ARQuizDashboardAnchorPlacer arQuizDashboardAnchorPlacer = FindObjectOfType<ARQuizDashboardAnchorPlacer>();
//    countryClickHandler.enabled = true;
//    arQuizDashboardAnchorPlacer.enabled = false;

//    bool submitClicked = false;

//    // Wait until the player clicks on a country and it gets lifted
//    while (!submitClicked)
//    {
//        if (countryClickHandler.SelectedCountryName != null)
//        {
//            // Wait for the user to click the submit button
//            Button submitButton = answerButtons[3];
//            submitButton.onClick.AddListener(() => submitClicked = true);
//        }
//        yield return null;
//    }

//    // Check if the selected country is the correct answer
//    if (countryClickHandler.SelectedCountryName == question.targetCountryName)
//    {
//        Debug.Log("Correct answer!");
//        // Increase the score and show the correct feedback (e.g., change color, show a message, etc.)
//    }
//    else
//    {
//        Debug.Log("Wrong answer!");
//        // Show the wrong feedback (e.g., change color, show a message, etc.)
//    }

//    // Reset the SelectedCountryName
//    countryClickHandler.SelectedCountryName = null;
//    // Add a delay before moving to the next question
//    yield return new WaitForSeconds(2f);

//    // Move to the next question
//    MoveToNextQuestion();
//}
