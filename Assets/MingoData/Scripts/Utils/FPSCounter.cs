using TMPro;
using UnityEngine;

namespace MingoData.Scripts.Utils
{

    public class FPSCounter : MonoBehaviour
    {
        public TextMeshProUGUI fpsText;
        private float deltaTime;

        private void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = $"FPS: {Mathf.Round(fps)}";
        }
    }

}
