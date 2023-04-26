using Firebase.Auth;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button signUpButton;
    public TextMeshProUGUI errorText;
    private bool isEmailValid = false;
    private bool isPasswordValid = false;
    public Color defaultBorderColor = Color.clear;
    public Color errorBorderColor = Color.red;
    private Image emailBorderImage;
    private Image passwordBorderImage;

    private void Start()
    {
        errorText.enabled= false;
        signUpButton.onClick.AddListener(OnSignUpButtonClicked);
        signUpButton.interactable = false;

        //emailInputField.onValueChanged.AddListener(delegate { UpdateEmailStatus(); });
        emailInputField.onValueChanged.AddListener(_ => UpdateEmailStatus());
        passwordInputField.onValueChanged.AddListener(_ => UpdatePasswordStatus());

        emailBorderImage = emailInputField.GetComponent<Image>();
        passwordBorderImage = passwordInputField.GetComponent<Image>();

    }
    private void OnSignUpButtonClicked()
    {
        SignUp();
    }

    private void SignUp()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (!isEmailValid)
        {
            errorText.text = "Please enter a valid email.";
            errorText.enabled = true;
            return;
        }
        if (!isPasswordValid)
        {
            errorText.text = "Please enter a valid password.";
            errorText.enabled = true;
            return;
        }


        AuthManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            string errorMessage = AuthUtility.LogTaskCompletion(task, "CreateUserWithEmailAndPasswordAsync");
            MainThreadDispatcher.Enqueue(() =>
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    errorText.enabled = true;
                    errorText.color = Color.red;
                    errorText.text = errorMessage;
                }
                else
                {
                    FirebaseUser newUser = task.Result;
                    Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
                    errorText.enabled = true;
                    errorText.color = Color.green;
                    errorText.text = "User created successfully!";

                }
            });
        });

    }

    private void UpdatePasswordStatus()
    {
        isPasswordValid = AuthUtility.IsValidPassword(passwordInputField.text);

        if (isPasswordValid)
        {
            passwordBorderImage.color = defaultBorderColor;
            errorText.enabled = false;
            errorText.text = ""; // clear error text if password is valid
        }
        else
        {
            passwordBorderImage.color = errorBorderColor;
            errorText.enabled = true;
            errorText.text = "Password must be at least 6 characters long.";
        }

        UpdateSignUpButtonStatus();
    }

    private void UpdateEmailStatus()
    {
        string result = AuthUtility.ValidateEmail(emailInputField.text);
        isEmailValid = result == "Valid email address";

        if (isEmailValid)
        {
            emailBorderImage.color = defaultBorderColor;
            // check if email already exists
            //CheckEmailExists(emailInputField.text);
            errorText.enabled = true;

            errorText.text = ""; // clear error text if email is valid
        }
        else
        {
            emailBorderImage.color = errorBorderColor;
            errorText.enabled = true;

            errorText.text = result; // show error text if email is invalid
        }

        UpdateSignUpButtonStatus();
    }


    private void UpdateSignUpButtonStatus()
    {
        signUpButton.interactable = isEmailValid && isPasswordValid;
    }



  
    //private void SendVerificationEmail()
    //{
    //    FirebaseUser user = AuthManager.Instance.Auth.CurrentUser;
    //    user?.SendEmailVerificationAsync().ContinueWith(task =>
    //        {
    //            if (task.IsCanceled)
    //            {
    //                Debug.LogError("SendEmailVerificationAsync was canceled.");
    //                return;
    //            }

    //            if (task.IsFaulted)
    //            {
    //                Debug.LogError("SendEmailVerificationAsync encountered an error: " + task.Exception);
    //                return;
    //            }

    //            Debug.Log("Verification email sent successfully.");
    //        });
    //}

}