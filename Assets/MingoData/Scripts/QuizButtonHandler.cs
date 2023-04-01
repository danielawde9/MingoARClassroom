using UnityEngine;
using UnityEngine.UI;

public class QuizButtonHandler : MonoBehaviour
{
    public void OnButtonClicked(Button button)
    {
        Debug.Log("Clicked button: " + button.name);
    }
}
