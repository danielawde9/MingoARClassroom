using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

using Photon.Pun;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviourPunCallbacks
{

    public GameObject questionObject;
    public GameObject[] answerObjects;
    private List<Question> questions;
    private int currentQuestionIndex = -1;
    private Dictionary<string, int> answerObjectDict;

    public GameData gameData;

    public UIManager uiManager;
    public float timerDuration = 90f;
    private float currentTimer;

    public SceneSwitcher sceneSwitcher;
    public Slider timerSlider; 

    [SerializeField] Color targetColor;
    [SerializeField] float maxRange = 0.00001f;
    [SerializeField] Camera mainCamera;
    [SerializeField] AudioSource audioClick;
    [SerializeField] AudioSource audioFinishLevel;
    [SerializeField] private LoadingScreenManager loadingScreenManager;
    [SerializeField] private DatabaseManager databaseManager;

    private Dictionary<int, int> playerAnswers;
    private Dictionary<int, int> playerScores;
    private Dictionary<int, PlayerData> playerDataDict = new Dictionary<int, PlayerData>();

    // Initialize the list of questions
    private void Start()
    {

        gameData.questions = new List<Question>();

        timerSlider.maxValue = timerDuration;

        timerSlider.value = timerDuration;

        //gameData.userScore = await databaseManager.FetchUserScore();

        // Enable the loading modal
        loadingScreenManager.ShowLoadingScreen();

        InvokeRepeating(nameof(UpdateTimer), 0f, 0.1f);

        // Initialize the list of questions
        InitListQuestionArray();

        // Fetch the latest questions from the remote database
        StartCoroutine(LoadQuestionsFromDatabase());

        playerAnswers = new Dictionary<int, int>();

        currentTimer = timerDuration;

        playerScores = new Dictionary<int, int>();

        Debug.LogError("init started");


    }
    private void UpdatePlayerScore(int playerId, int score)
    {
        if (playerScores.ContainsKey(playerId))
        {
            playerScores[playerId] += score;
        }
        else
        {
            playerScores.Add(playerId, score);
        }

        // Update the UI based on the updated player score
        uiManager.UpdatePlayerScore(playerId, playerScores[playerId]);
    }

    // Initialize the dictionary for answer objects
    private void InitListQuestionArray()
    {
        answerObjectDict = new Dictionary<string, int>();
        for (int i = 0; i < answerObjects.Length; i++)
        {
            answerObjectDict.Add($"AnswerBlock{i + 1}", i);
        }
    }


    // Coroutine to load questions from the database
    private IEnumerator LoadQuestionsFromDatabase()
    {
        Debug.LogError("LoadQuestionsFromDatabase started");
        while (databaseManager == null)
        {
            yield return null;
        }

        var task = databaseManager.FetchAllQuestions();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to fetch questions: {task.Exception}");
        }
        else
        {
            questions = task.Result;

            loadingScreenManager.HideLoadingScreen();

            ShowNextQuestion();

        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Return if there are no touches or if the first touch is not in the Began phase
        if (Input.touchCount == 0 || Input.touches[0].phase != TouchPhase.Began) return;
        // Cast a ray from the touch position on the screen
        Ray ray = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
        // Check if the ray hits any object
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        // Get the answer index from the hit object's name
        int answerIndex = GetAnswerIndex(hit.transform.name);
        // Return if the answer index is invalid
        if (answerIndex == -1) return;
        // Play the click audio
        audioClick.Play();

    
        SelectAnswer(answerIndex);
    }

    private void UpdateTimer()
    {
        currentTimer -= Time.deltaTime;
        timerSlider.value = currentTimer;

        if (currentTimer <= 0)
        {
            RevealAnswersAndProceed();
            currentTimer = timerDuration;
        }
    }

    [PunRPC]
    public void RpcSelectAnswer(int playerId, int answerIndex)
    {
        if (!playerAnswers.ContainsKey(playerId))
        {
            playerAnswers.Add(playerId, answerIndex);
            // Get the answer text from the hit object
            string answerText = answerObjects[answerIndex].GetComponentInChildren<TextMeshPro>().text;
            // Check if the selected answer is correct
            bool isCorrectAnswer = IsCorrectAnswer(answerText);
            // Update the answer color based on the correctness
            StartCoroutine(UpdateAnswerColor(isCorrectAnswer, answerIndex));
            // Update the instructions text based on the correctness
            uiManager.UpdateTextInstructions(isCorrectAnswer ? "Correct" : "Wrong");
            // If the answer is correct, update the user score and show the next question after a delay
            if (isCorrectAnswer)
            {
                gameData.userScore += questions[currentQuestionIndex].UserScore;
                uiManager.UpdateUserScore(gameData.userScore);
                StartCoroutine(ShowNextQuestionAfterDelay(1f));
            }
            else
            {
                Handheld.Vibrate();
            }

        }
    }


    // Coroutine to update the answer color
    // Input: isCorrectAnswer (bool), answerIndex (int)
    // Return: IEnumerator
    private IEnumerator UpdateAnswerColor(bool isCorrectAnswer, int answerIndex)
    {
        for (int i = 0; i < answerObjects.Length; i++)
        {
            TextMeshPro textMeshPro = answerObjects[i].GetComponentInChildren<TextMeshPro>();
            if (i == 0)
            {
                textMeshPro.color = Color.green;
                answerObjects[0].GetComponent<Animator>().enabled = true;
                answerObjects[0].GetComponent<Animator>().SetTrigger("ScaleUp");
            }
            else if (!isCorrectAnswer && i == answerIndex)
            {
                textMeshPro.color = Color.red;
                // animator.SetTrigger("ScaleUp"); // Uncomment and replace with your incorrect answer animation trigger
            }
            else
            {
                textMeshPro.color = Color.white;
            }
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < answerObjects.Length; i++)
        {
            answerObjects[i].GetComponentInChildren<TextMeshPro>().color = Color.white;
        }
    }

    // Get the answer index from the answer block name
    // Input: answerBlockName (string)
    // Return: int
    private int GetAnswerIndex(string answerBlockName)
    {
        return answerObjectDict.TryGetValue(answerBlockName, out int index) ? index : -1;
    }

    // Check if the given answer is the correct answer
    // Input: answer (string)
    // Return: bool
    public bool IsCorrectAnswer(string answer)
    {
        if (currentQuestionIndex < 0 || currentQuestionIndex >= questions.Count)
        {
            return false;
        }

        return answer == questions[currentQuestionIndex].Answers[0];
    }

    // Coroutine to show the next question after a delay
    // Input: delay (float)
    // Return: IEnumerator
    private IEnumerator ShowNextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowNextQuestion();
    }

    // Show the next question in the list
    public void ShowNextQuestion()
    {

        uiManager.UpdateProgress((float)currentQuestionIndex / (questions.Count - 1));

        // will loop through the question then get back to 0, when reached max ex: 5/5 remainder is 0
        if (questions == null || questions.Count == 0)
        {
            Debug.LogWarning("Questions list is not initialized or is empty.");
            return;
        }
        // to loop thru the questions again 
        //currentQuestionIndex = (currentQuestionIndex + 1) % questions.Count;
        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Count)
        {
            FinishedQuestionsAndLoadNextScene();
            return;
        }

        // Play level complete audio when all questions are answered
        if (currentQuestionIndex == 0) audioFinishLevel.Play();

        Question currentQuestion = questions[currentQuestionIndex];
        questionObject.GetComponentInChildren<TextMeshPro>().text = currentQuestion.QuestionText;

        //questionObject.transform.position = RandomVector();

        for (int i = 0; i < answerObjects.Length; i++)
        {
            answerObjects[i].GetComponentInChildren<TextMeshPro>().text = currentQuestion.Answers[i];
            //answerObjects[i].transform.position = RandomVector();
        }
    }

    private bool AreQuestionsFinished()
    {
        return currentQuestionIndex >= questions.Count;
    }

    public void FinishedQuestionsAndLoadNextScene()
    {
        if (AreQuestionsFinished())
        {
            //await databaseManager.SaveUserScore(gameData.userScore);
            uiManager.UpdateTextInstructions("clicked bl 25r");
            sceneSwitcher.LoadScene("MainMenuScene");
        }
    }

    // Called when the anchor is found
    public void OnAnchorFound()
    {
        uiManager.UpdateTextInstructions("Anchor Found");
    }

    // Called when the anchor is lost
    public void OnAnchorLost()
    {
        uiManager.UpdateTextInstructions("Anchor Lost");
    }

    // Generate a random Vector3 within a range
    // Return: Vector3
    private Vector3 RandomVector()
    {
        Vector2 randomPos = Random.insideUnitCircle * maxRange;

        // Check if any answer object is overlapping with the new position
        bool overlap = false;
        foreach (GameObject answerObj in answerObjects)
        {
            if (Vector2.Distance(randomPos, answerObj.transform.position) < 0.1f)
            {
                overlap = true;
                break;
            }
        }

        // If there is an overlap, generate a new random position
        if (overlap)
        {
            return RandomVector();
        }

        // If there is no overlap, return the new position
        return new Vector3(randomPos.x, questionObject.transform.position.y, randomPos.y);
    }


    // to synchronize the answer selection across all players using an RPC call
    public void SelectAnswer(int answerIndex)
    {
        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC("RpcSelectAnswer", RpcTarget.AllBuffered, playerId, answerIndex);
    }

    private void RevealAnswersAndProceed()
    {
        int timeBasedScore = Mathf.RoundToInt((currentTimer / timerDuration) * 1000);
        foreach (var playerAnswer in playerAnswers)
        {
            int playerId = playerAnswer.Key;
            int answerIndex = playerAnswer.Value;
            string answerText = answerObjects[answerIndex].GetComponentInChildren<TextMeshPro>().text;
            bool isCorrectAnswer = IsCorrectAnswer(answerText);
            // Update the user score and other UI elements based on the correctness of the answer
            // You might need to modify this part depending on how you want to display the results for each player
            if (isCorrectAnswer)
            {
                UpdatePlayerScore(playerId, timeBasedScore);
            }
        }
        // Clear the playerAnswers for the next question
        playerAnswers.Clear();

        // Proceed to the next question after a delay
        StartCoroutine(ShowNextQuestionAfterDelay(1f));
    }
}