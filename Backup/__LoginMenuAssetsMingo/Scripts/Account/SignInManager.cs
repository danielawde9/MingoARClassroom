using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button signInButton;
    public TextMeshProUGUI errorText;

    private bool isEmailValid = false;
    private bool isPasswordValid = false;
    public Color defaultBorderColor = Color.clear;
    public Color errorBorderColor = Color.red;
    private Image emailBorderImage;
    private Image passwordBorderImage;

    private void Start()
    {
        errorText.enabled = false;
        signInButton.onClick.AddListener(OnSignInButtonClicked);

        emailInputField.onValueChanged.AddListener(_ => UpdateEmailStatus());
        passwordInputField.onValueChanged.AddListener(_ => UpdatePasswordStatus());

        emailBorderImage = emailInputField.GetComponent<Image>();
        passwordBorderImage = passwordInputField.GetComponent<Image>();
    }

    private void OnSignInButtonClicked()
    {
        SignIn();
    }

    private void SignIn()
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

        AuthManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            string errorMessage = AuthUtility.LogTaskCompletion(task, "SignInWithEmailAndPasswordAsync");
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
                    FirebaseUser loggedInUser = task.Result;
                    Debug.LogFormat("Firebase user created successfully: {0} ({1})", loggedInUser.DisplayName, loggedInUser.UserId);
                    errorText.enabled = true;
                    errorText.color = Color.green;
                    errorText.text = "User logged in successfully!";

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
        signInButton.interactable = isEmailValid && isPasswordValid;
    }


}
