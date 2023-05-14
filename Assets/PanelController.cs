using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public RectTransform panelRectTransform;
    public Button button;
    public float transitionDuration = 1f;

    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private bool isPanelVisible = false;
    private TextMeshProUGUI buttonText;


    private void Start()
    {
        initialPosition = panelRectTransform.anchoredPosition;
        button.onClick.AddListener(TogglePanel);
        targetPosition = initialPosition + new Vector2(0f, panelRectTransform.rect.height);
        buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;

        if (isPanelVisible)
        {
            StartCoroutine(TransitionPanel(initialPosition, targetPosition));
            buttonText.text = "↓";
        }
        else
        {
            StartCoroutine(TransitionPanel(targetPosition, initialPosition));
            buttonText.text = "↑";
        }
    }

    private IEnumerator TransitionPanel(Vector2 startPosition, Vector2 endPosition)
    {
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            panelRectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panelRectTransform.anchoredPosition = endPosition;
    }
}
