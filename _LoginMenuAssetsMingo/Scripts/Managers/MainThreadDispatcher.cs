using System.Collections.Generic;
using UnityEngine;
using System;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance;
    // the queue action to be executed 
    private readonly Queue<Action> actions = new Queue<Action>();
    // queue action and excuted it i n the main thread
    // called when the game object is first created 
    private void Awake()
    {
        // check to see if there is already an instance of the main thread dispatcher
        if (instance == null)
        {
            // create new instamce
            instance = this;
            // dont destroy when the scene is unloaded
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // destroy
            Destroy(gameObject);
        }
    }


    // enqueue an anction 
    public static void Enqueue(Action action)
    {
        // lock the queye
        lock (instance.actions)
        {
            // enquee
            instance.actions.Enqueue(action);
        }
    }


    // remove an action from the queye and excute it 
    private void Update()
    {

        while (actions.Count > 0)
        {
            // get next action to excecute
            Action action;
            lock (actions)
            {
                action = actions.Dequeue();
            }
            // excecute
            action?.Invoke();
        }
    }
}

/*
  Here are a few examples of where you can use MainThreadDispatcher:

Example 1: Updating the UI after an asynchronous HTTP request

csharp
Copy code
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HttpExample : MonoBehaviour
{
    public Text responseText;

    private void Start()
    {
        StartCoroutine(GetRequest("https://jsonplaceholder.typicode.com/posts/1"));
    }

    private IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                // Enqueue error message update on the main thread
                MainThreadDispatcher.Enqueue(() =>
                {
                    responseText.text = "Error: " + webRequest.error;
                });
            }
            else
            {
                // Enqueue response update on the main thread
                MainThreadDispatcher.Enqueue(() =>
                {
                    responseText.text = webRequest.downloadHandler.text;
                });
            }
        }
    }
}
Example 2: Spawning a GameObject after an asynchronous task

csharp
Copy code
using UnityEngine;
using System.Threading.Tasks;

public class SpawnExample : MonoBehaviour
{
    public GameObject prefab;

    private async void Start()
    {
        await Task.Run(() =>
        {
            // Simulate a background task by waiting for 2 seconds
            System.Threading.Thread.Sleep(2000);
        });

        // Enqueue spawning the prefab on the main thread
        MainThreadDispatcher.Enqueue(() =>
        {
            Instantiate(prefab, Vector3.zero, Quaternion.identity);
        });
    }
}
Example 3: Moving a GameObject after an asynchronous database query

csharp
Copy code
using UnityEngine;
using Firebase;
using Firebase.Database;

public class DatabaseExample : MonoBehaviour
{
    public GameObject targetObject;
    private DatabaseReference reference;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp.Create();
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            MoveObjectAsync();
        });
    }

    private async void MoveObjectAsync()
    {
        DataSnapshot snapshot = await reference.Child("position").GetValueAsync();
        Vector3 newPosition = JsonUtility.FromJson<Vector3>(snapshot.GetRawJsonValue());

        // Enqueue moving the object on the main thread
        MainThreadDispatcher.Enqueue(() =>
        {
            targetObject.transform.position = newPosition;
        });
    }
}
*/