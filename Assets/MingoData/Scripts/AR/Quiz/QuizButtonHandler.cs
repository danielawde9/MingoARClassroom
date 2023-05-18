using UnityEngine;
using UnityEngine.UI;

public class QuizButtonHandler : MonoBehaviour
{
    [SerializeField] private QuizManager quizManager;

    // This function will be called when a button is clicked using Event Triggers.
    public void OnButtonClicked(Button button)
    {
        quizManager.OnButtonClicked(button);
    }
}
