using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class DatabaseManager : MonoBehaviour
{
    DatabaseReference reference;
    FirebaseApp app;
    FirebaseAuth auth;
    bool isFirebaseInitialized = false;

    private void Start()
    {


        // Access FirebaseAuth instance from AuthManager
        auth = AuthManager.Instance.Auth;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                string databaseURL = "https://mingo-ar-classroom-default-rtdb.europe-west1.firebasedatabase.app";
                app = FirebaseApp.DefaultInstance;
                Uri uri = new(databaseURL);
                app.Options.DatabaseUrl = uri;

                // Get the root reference location of the database.
                reference = FirebaseDatabase.DefaultInstance.RootReference;

                isFirebaseInitialized = true;
            }
            else
            {
                Debug.LogError(String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });


    }

    public void SaveData()
    {
        // Get the user's UID
        string userId = auth.CurrentUser.UserId;

        // Save some data to the database using the user's UID
        reference.Child("users").Child(userId).Child("exampleData").SetValueAsync("Some data");
    }

    public void GetData()
    {
        // Get the user's UID
        string userId = auth.CurrentUser.UserId;

        // Retrieve data from the database using the user's UID
        reference.Child("users").Child(userId).Child("exampleData").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("An error occurred while retrieving data from the database: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string data = snapshot.Value.ToString();
                Debug.Log("Data retrieved from the database: " + data);
            }
        });
    }

    public async Task<List<Question>> FetchAllQuestions()
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogWarning("Firebase is not initialized yet. Retrying in 1 second.");
            await Task.Delay(1000);
            return await FetchAllQuestions();
        }

        var snapshot = await FirebaseDatabase.DefaultInstance.GetReference("questions").GetValueAsync();
        Debug.LogWarning("snapshot." + snapshot);

        if (snapshot.Exists)
        {
            List<Question> questions = new();

            foreach (var questionData in snapshot.Children)
            {
                QuestionData rawData = JsonUtility.FromJson<QuestionData>(questionData.GetRawJsonValue());
                Debug.LogWarning("rawData." + rawData);

                string[] answersArray = rawData.answers.Split(',');

                Question question = new(rawData.questionText, answersArray, rawData.userScore);
                questions.Add(question);
            }

            return questions;
        }
        else
        {
            Debug.LogError("No questions found in the database.");
            return null;
        }
    }


}



/*    public async Task SaveUserScore(int userScore)
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogWarning("Firebase is not initialized yet. Retrying in 1 second.");
            await Task.Delay(1000);
            await SaveUserScore(userScore);
            return;
        }

        string userID = "testID";

        DatabaseReference userScoreRef = reference.Child("userScores").Child(userID);
        await userScoreRef.SetValueAsync(userScore);
        Debug.Log("User score saved successfully.");
    }

    public async Task<int> FetchUserScore()
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogWarning("Firebase is not initialized yet. Retrying in 1 second.");
            await Task.Delay(1000);
            return await FetchUserScore();
        }

        var snapshot = await FirebaseDatabase.DefaultInstance.GetReference("userScore").GetValueAsync();

        if (snapshot.Exists)
        {
            return int.Parse(snapshot.Value.ToString());
        }
        else
        {
            Debug.LogWarning("No user score found in the database.");
            return 0;
        }
    }
*/