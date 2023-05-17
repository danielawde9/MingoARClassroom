using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public RectTransform panelRectTransform;
    public Button toggleButton;
    public float transitionDuration = 1f;

    private Vector2 initialPosition;
    private Vector2 targetPosition;
    public bool isMenuPanelVisible = false;
    private Image buttonImage;

    public Sprite downSprite;
    public Sprite upSprite;

    private void Start()
    {
        initialPosition = panelRectTransform.anchoredPosition;
        toggleButton.onClick.AddListener(TogglePanel);
        targetPosition = initialPosition + new Vector2(0f, panelRectTransform.rect.height);
        buttonImage = toggleButton.GetComponentInChildren<Image>();
    }

    public void TogglePanel()
    {
        isMenuPanelVisible = !isMenuPanelVisible;

        if (isMenuPanelVisible)
        {
            StartCoroutine(TransitionPanel(initialPosition, targetPosition));
            buttonImage.sprite= downSprite;
        }
        else
        {
            StartCoroutine(TransitionPanel(targetPosition, initialPosition));
            buttonImage.sprite = upSprite;
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
