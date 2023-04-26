using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreenPanel;

    public void ShowLoadingScreen()
    {
        loadingScreenPanel.SetActive(true);
    }
    public void HideLoadingScreen()
    {
        loadingScreenPanel.SetActive(false);
    }
}
