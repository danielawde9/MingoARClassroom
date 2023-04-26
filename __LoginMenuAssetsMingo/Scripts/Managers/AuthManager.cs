using Firebase;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    // we will use signleton patern to be able to access it within any scene
    // you only need to add these scripts to a single gameobject at first scene
    // also the loggedin state will be handle by firebase 
    // AuthManager.Instance.auth.CurrentUser


    public static AuthManager Instance { get; private set; }
    public FirebaseAuth Auth { get; private set; }

    public GameObject loginMenu;
    public GameObject signUpMenu;
    public GameObject mainMenu;
    public GameObject resetPasswordMenu;

    public event Action OnUserLoggedIn;
    public event Action OnUserLoggedOut;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            Auth = FirebaseAuth.DefaultInstance;
            Auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, EventArgs.Empty);
        });
    }
    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        FirebaseUser user = Auth.CurrentUser;
        if (user != null)
        {
            Debug.LogFormat("User logged in: {0} ({1})", user.DisplayName, user.UserId);
            OnUserLoggedIn?.Invoke();
        }
        else
        {
            Debug.Log("User logged out");
            OnUserLoggedOut?.Invoke();
        }
    }


    private void OnDestroy()
    {
        if (Auth != null)
        {
            Auth.StateChanged -= AuthStateChanged;
        }
    }
}