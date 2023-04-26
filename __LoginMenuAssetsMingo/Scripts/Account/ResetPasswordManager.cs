using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetPasswordManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public Button resetPasswordButton;
    public TextMeshProUGUI errorText;
    private bool isEmailValid = false;
    private Image emailBorderImage;

    private void Start()
    {
        errorText.enabled = false;
        resetPasswordButton.onClick.AddListener(OnResetPasswordButtonClicked);
        resetPasswordButton.interactable = false;

        emailInputField.onValueChanged.AddListener(_ => UpdateEmailStatus());
        emailBorderImage = emailInputField.GetComponent<Image>();

    }

    private void OnResetPasswordButtonClicked()
    {
        ResetPassword();
    }

    private void ResetPassword()
    {
        string email = emailInputField.text;

        string emailValidationResult = AuthUtility.ValidateEmail(email);
        if (emailValidationResult != "Valid email address")
        {
            errorText.text = emailValidationResult;
            errorText.enabled = true;
            return;
        }

        AuthManager.Instance.Auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            string errorMessage = AuthUtility.LogTaskCompletion(task, "SendPasswordResetEmailAsync");
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
                    errorText.enabled = true;
                    errorText.color = Color.green;
                    errorText.text = "Password reset email sent successfully !";

                }
            });
        });
    }

    private void UpdateEmailStatus()
    {
        string result = AuthUtility.ValidateEmail(emailInputField.text);
        isEmailValid = result == "Valid email address";
        if(isEmailValid)
        {
            emailBorderImage.color = Color.clear;
            errorText.enabled = false;
            errorText.text = ""; // clear error text if password is valid
        }

        else
        {
            emailBorderImage.color = Color.red;
            errorText.enabled = true;

            errorText.text = result; // show error text if email is invalid
        }

        UpdateSignUpButtonStatus();
    }

    private void UpdateSignUpButtonStatus()
    {
        resetPasswordButton.interactable = isEmailValid;
    }
}
