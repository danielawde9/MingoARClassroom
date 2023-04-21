using Firebase.Auth;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class AuthUtility
{


    public static bool IsValidPassword(string password)
    {
        // firebase password
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static string LogTaskCompletion(Task task, string operation)
    {
        string errorMessage = "";
        if (task.IsCanceled)
        {
            errorMessage = operation + " canceld.";

        }
        else if (task.IsFaulted)
        {
            foreach (System.Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                if (exception is Firebase.FirebaseException firebaseException)
                {
                    AuthError authError = (AuthError)firebaseException.ErrorCode;
                    switch (authError)
                    {
                        case AuthError.EmailAlreadyInUse:
                            errorMessage = "This email is already registered.";
                            break;
                        case AuthError.None:
                            break;
                        case AuthError.Unimplemented:
                            errorMessage = "This feature is not implemented.";
                            break;
                        case AuthError.Failure:
                            errorMessage = "The operation failed. Please try again later.";
                            break;
                        case AuthError.InvalidCustomToken:
                            errorMessage = "Invalid custom token.";
                            break;
                        case AuthError.CustomTokenMismatch:
                            errorMessage = "The custom token does not match the current Firebase app instance.";
                            break;
                        case AuthError.InvalidCredential:
                            errorMessage = "Invalid credentials.";
                            break;
                        case AuthError.UserDisabled:
                            errorMessage = "This user account has been disabled.";
                            break;
                        case AuthError.AccountExistsWithDifferentCredentials:
                            errorMessage = "An account already exists with the same email address but different sign-in credentials.";
                            break;
                        case AuthError.OperationNotAllowed:
                            errorMessage = "This operation is not allowed.";
                            break;
                        case AuthError.RequiresRecentLogin:
                            errorMessage = "This operation requires a recent login.";
                            break;
                        case AuthError.CredentialAlreadyInUse:
                            errorMessage = "The credential is already in use by another user.";
                            break;
                        case AuthError.InvalidEmail:
                            errorMessage = "Please enter a valid email address.";
                            break;
                        case AuthError.WrongPassword:
                            errorMessage = "The password is incorrect.";
                            break;
                        case AuthError.TooManyRequests:
                            errorMessage = "Too many requests. Please try again later.";
                            break;
                        case AuthError.UserNotFound:
                            errorMessage = "The email account was not found.";
                            break;
                        case AuthError.ProviderAlreadyLinked:
                            errorMessage = "The user account is already linked to this provider.";
                            break;
                        case AuthError.NoSuchProvider:
                            errorMessage = "This provider does not exist.";
                            break;
                        case AuthError.InvalidUserToken:
                            errorMessage = "Invalid user token.";
                            break;
                        case AuthError.UserTokenExpired:
                            errorMessage = "The user token has expired.";
                            break;
                        case AuthError.NetworkRequestFailed:
                            errorMessage = "The network request failed. Please check your internet connection.";
                            break;
                        case AuthError.InvalidApiKey:
                            errorMessage = "Invalid API key.";
                            break;
                        case AuthError.AppNotAuthorized:
                            errorMessage = "The app is not authorized to use Firebase Authentication.";
                            break;
                        case AuthError.UserMismatch:
                            errorMessage = "The provided credentials do not match the previously signed-in user.";
                            break;
                        case AuthError.WeakPassword:
                            errorMessage = "The password is too weak.";
                            break;
                        case AuthError.NoSignedInUser:
                            errorMessage = "No user is currently signed in.";
                            break;
                        case AuthError.ApiNotAvailable:
                            errorMessage = "The API is not available.";
                            break;
                        case AuthError.ExpiredActionCode:
                            errorMessage = "The action code has expired.";
                            break;
                        case AuthError.InvalidActionCode:
                            errorMessage = "The action code is invalid.";
                            break;
                        case AuthError.InvalidMessagePayload:
                            errorMessage = "The message payload is invalid.";
                            break;
                        case AuthError.InvalidPhoneNumber:
                            errorMessage = "Please enter a valid phone number.";
                            break;
                        case AuthError.MissingPhoneNumber:
                            errorMessage = "The phone number is missing.";
                            break;
                        case AuthError.InvalidRecipientEmail:
                            errorMessage = "The recipient email address is invalid.";
                            break;
                        case AuthError.InvalidSender:
                            errorMessage = "The sender email address is invalid.";
                            break;
                        case AuthError.InvalidVerificationCode:
                            errorMessage = "The verification code is invalid.";
                            break;
                        case AuthError.InvalidVerificationId:
                            errorMessage = "The verification ID is invalid.";
                            break;
                        case AuthError.MissingVerificationCode:
                            errorMessage = "MissingVerificationCode";
                            break;
                        case AuthError.InvalidContinueUri: errorMessage = "The provided continuation URL is invalid."; break;
                        case AuthError.MissingContinueUri: errorMessage = "A continuation URL must be provided in the request."; break;
                        case AuthError.KeychainError: errorMessage = "An error occurred while accessing the keychain."; break;
                        case AuthError.MissingAppToken: errorMessage = "An App Store Connect auth token is missing from the request."; break;
                        case AuthError.MissingIosBundleId: errorMessage = "The bundle ID of the iOS app is missing from the request."; break;
                        case AuthError.NotificationNotForwarded: errorMessage = "Push notifications have not been forwarded to the device."; break;
                        case AuthError.UnauthorizedDomain: errorMessage = "The domain of the URL is not authorized to perform the requested action."; break;
                        case AuthError.WebContextAlreadyPresented: errorMessage = "The sign-in flow has already been presented in a web context."; break;
                        case AuthError.WebContextCancelled: errorMessage = "The sign-in flow in the web context was canceled by the user."; break;
                        case AuthError.DynamicLinkNotActivated: errorMessage = "Firebase Dynamic Links have not been activated in the project."; break;
                        case AuthError.Cancelled: errorMessage = "The sign-in operation was canceled by the user."; break;
                        case AuthError.InvalidProviderId: errorMessage = "The given provider ID is invalid."; break;
                        case AuthError.WebInternalError: errorMessage = "An internal error occurred during the sign-in flow in the web context."; break;
                        case AuthError.WebStorateUnsupported: errorMessage = "The web storage API is not supported in this browser or device."; break;
                        case AuthError.TenantIdMismatch: errorMessage = "The provided tenant ID does not match the authenticated user's tenant ID."; break;
                        case AuthError.UnsupportedTenantOperation: errorMessage = "The operation is not supported in a multi-tenant context."; break;
                        case AuthError.InvalidLinkDomain: errorMessage = "The domain of the link provided in the request is not whitelisted."; break;
                        case AuthError.RejectedCredential: errorMessage = "The requested credential is rejected by the server."; break;
                        case AuthError.PhoneNumberNotFound: errorMessage = "The phone number was not found."; break;
                        case AuthError.InvalidTenantId: errorMessage = "The tenant ID provided in the request is invalid."; break;
                        case AuthError.MissingClientIdentifier: errorMessage = "A client ID must be provided in the request."; break;
                        case AuthError.MissingMultiFactorSession: errorMessage = "A multi-factor session must be provided in the request."; break;
                        case AuthError.MissingMultiFactorInfo: errorMessage = "A multi-factor info object must be provided in the request."; break;
                        case AuthError.InvalidMultiFactorSession: errorMessage = "The provided multi-factor session is invalid."; break;
                        case AuthError.MultiFactorInfoNotFound: errorMessage = "The multi-factor info object was not found."; break;
                        case AuthError.AdminRestrictedOperation: errorMessage = "The requested operation is restricted to administrators only."; break;
                        case AuthError.UnverifiedEmail: errorMessage = "The email is not verified."; break;
                        case AuthError.SecondFactorAlreadyEnrolled: errorMessage = "The second factor is already enrolled."; break;
                        case AuthError.MaximumSecondFactorCountExceeded: errorMessage = "The maximum number of second factors has been exceeded."; break;
                        case AuthError.UnsupportedFirstFactor: errorMessage = "The first factor is not supported in this context."; break;
                        case AuthError.EmailChangeNeedsVerification: errorMessage = "A user attempting to change their email address must verify the new email address before it can be used."; break;
                        default:
                            errorMessage = "an error has occured please try again later";
                            break;
                    }
                }
            }
        }
        return errorMessage;
    }

    public static string ValidateEmail(string email)
    {

        // Regex pattern for matching an email
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        Regex regex = new Regex(pattern);
        // Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
        Match match = regex.Match(email);
        if (match.Success)
        {
            // return "Valid email address";

            string domain = email.Split('@')[1];
            try
            {
                IPHostEntry host = Dns.GetHostEntry(domain);
                return "Valid email address";
            }
            catch (SocketException)
            {
                return "Invalid domain name";
            }
        }
        else
        {
            // check for common mistakes
            if (email.Contains(" "))
            {
                return " email cannot contain spaces";
            }
            else if (email.StartsWith("@") || email.EndsWith("@"))
            {
                return "Email cannot start or end with '@'";
            }
            else if (email.Contains("@") && email.IndexOf("@") != email.LastIndexOf("@"))
            {
                return "Email cannot contain more than one '@' symbol";
            }
            else if (IsCommonTypo(email))
            {
                return "Did you mean " + email.Replace("gmial", "gmail")
                    .Replace("gamil", "gmail")
                    .Replace("htomail", "hotmail")
                    .Replace("yaho", "yahoo")
                    .Replace("yhaoo", "yahoo")
                    .Replace("outllok", "outlook")
                    .Replace("outook", "outlook")
                    .Replace("outloook", "outlook") + "?";
            }
            else
            {
                return "Invalid email format";
            }
        }
    }




    public static bool IsCommonTypo(string email)
    {
        // List of common typos in email addresses
        string[] typos = { "gmial", "hotmal", "gamil", "htomail", "yaho", "yhaoo", "outllok", "outook", "outloook" };

        foreach (string t in typos)
        {
            if (email.Contains(t))
            {
                return true;
            }
        }

        // email does not contain any common typos
        return false;
    }
}
