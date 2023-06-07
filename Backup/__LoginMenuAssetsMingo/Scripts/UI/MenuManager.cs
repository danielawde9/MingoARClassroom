using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject signUpMenu;
    public GameObject signInMenu;
    public GameObject resetPasswordMenu;
    public GameObject loggedInMenu;

    private void Start()
    {
        // Set the initial active menu
        SwitchMenus("SignInMenu");
    }

    public void SwitchMenus(string menuToActivate)
    {
        signUpMenu.SetActive(false);
        signInMenu.SetActive(false);
        resetPasswordMenu.SetActive(false);
        loggedInMenu.SetActive(false);

        switch (menuToActivate)
        {
            case "SignUpMenu":
                signUpMenu.SetActive(true);
                break;
            case "SignInMenu":
                signInMenu.SetActive(true);
                break;
            case "ResetPasswordMenu":
                resetPasswordMenu.SetActive(true);
                break;
            case "LoggedInMenu":
                loggedInMenu.SetActive(true);
                break;
        }
    }
}
