using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;

namespace MingoData.Scripts.Utils
{

    public class ClickMotion : MonoBehaviour
    {

        public SVGImage imageObject; // The UI Image object

        public float clickScale = 0.9f; // The scale when clicked
        public float animationDuration = 0.1f; // Duration of the animation

        private void Start()
        {
            StartCoroutine(ClickAnimation());
        }


        private IEnumerator ClickAnimation()
        {
            Vector3 originalScale = imageObject.rectTransform.localScale;
            Vector3 targetScale = new Vector3(clickScale, clickScale, clickScale);

            while (true)
            {
                // Scale down
                float startTime = Time.time;
                while (Time.time < startTime + animationDuration)
                {
                    imageObject.rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, (Time.time - startTime) / animationDuration);
                    yield return null;
                }

                // Scale back up
                startTime = Time.time;
                while (Time.time < startTime + animationDuration)
                {
                    imageObject.rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, (Time.time - startTime) / animationDuration);
                    yield return null;
                }

                // Wait a moment before starting the process again
                yield return new WaitForSeconds(1f);
            }
        }
    }

}
