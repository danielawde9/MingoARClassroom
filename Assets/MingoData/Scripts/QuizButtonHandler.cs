using UnityEngine;
using UnityEngine.UI;

public class QuizButtonHandler : MonoBehaviour
{
    public static Button LastClickButton { get; private set; }
    public void OnButtonClicked(Button button)
    {
        LastClickButton = button;
        Debug.Log("Clicked button: " + button.name);
    }

    public static void ResetLastClickButton()
    {
        LastClickButton = null;
    }
}
