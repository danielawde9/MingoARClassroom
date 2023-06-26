using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MingoData.Scripts.MainUtil;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MingoData.Scripts.Utils
{
    public class UtilsFns : MonoBehaviour
    {
        public static GameObject directionalLight;

        public enum AnimationDirection
        {
            LeftRight,
            UpDown
        }
        
        public static IEnumerator AnimateIcon(Transform iconRectTransform, float duration, AnimationDirection direction)
        {
            // Store the initial position
            Vector3 initialPosition = iconRectTransform.localPosition;

            // Calculate the target position
            Vector3 targetPosition;
            if (direction == AnimationDirection.LeftRight)
            {
                // For example, move 50 units to the right
                targetPosition = initialPosition + new Vector3(50f, 0f, 0f);
            }
            else // AnimationDirection.UpDown
            {
                // For example, move 50 units upwards
                targetPosition = initialPosition + new Vector3(0f, 50f, 0f);
            }

            while (Constants.AnimateTrue)
            {
                float timeElapsed = 0f;

                while (timeElapsed < duration)
                {
                    if (iconRectTransform == null)
                    {
                        // The icon has been destroyed, so stop the coroutine
                        yield break;
                    }

                    timeElapsed += Time.deltaTime;

                    // Use Lerp to smoothly transition between the initial and target position
                    iconRectTransform.localPosition = Vector3.Lerp(initialPosition, targetPosition, timeElapsed / duration);

                    yield return null;  // Wait until the next frame
                }

                // Ensure the final position is the target position
                iconRectTransform.localPosition = targetPosition;

                // Swap the initial and target positions for the next loop iteration
                (initialPosition, targetPosition) = (targetPosition, initialPosition);
            }
        }

        public static GameObject CreateDarkBackground(string objectName)
        {
            // Create new GameObject
            GameObject darkBackground = new GameObject("DarkBackground_"+objectName);

            // Add it as a child of the parent canvas
            darkBackground.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

            // Add an Image component to the GameObject
            Image image = darkBackground.AddComponent<Image>();

            darkBackground.AddComponent<Button>();
            
            // Set the image color to black with 50% opacity
            image.color = new Color(0, 0, 0, 0.5f);

            // Set the RectTransform to stretch in both directions and cover the whole screen
            RectTransform rectTransform = darkBackground.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = rectTransform.offsetMax = new Vector2(0, 0);
            // Set the created game object to a low priority by setting its sibling index to a lower value
            darkBackground.transform.SetSiblingIndex(0);

            return darkBackground;
        }
        
        public static void LoadNewScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        public static void BringToFront(GameObject uiElement)
        {
            Canvas parentCanvas = uiElement.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                uiElement.transform.SetSiblingIndex(parentCanvas.transform.childCount - 1);
            }
        }

        public static (string timeUnitKey, string timeValue) TimeScaleConversion(float timeScale)
        {
            return timeScale switch
            {
                <= 1 => ("1_second_real_life_equals_seconds", (timeScale).ToString("F2")),
                < 60 => ("1_second_real_life_equals_minutes", (timeScale).ToString("F2")),
                < 3600 => ("1_second_real_life_equals_hours", (timeScale / 60).ToString("F2")),
                < 86400 => ("1_second_real_life_equals_days", (timeScale / 3600).ToString("F2")),
                < 604800 => ("1_second_real_life_equals_weeks", (timeScale / 86400).ToString("F2")),
                < 2629800 => ("1_second_real_life_equals_months", (timeScale / 604800).ToString("F2")),
                _ => ("1_second_real_life_equals_years", (timeScale / 2629800).ToString("F2"))
            };
        }

        public static GameObject CreateGameObject(string name, GameObject parent, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject newGameObject = new GameObject(name);
            if (parent != null)
            {
                newGameObject.transform.SetParent(parent.transform, false);
            }
            newGameObject.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            return newGameObject;
        }

        public static TextMeshPro CreateTextMeshPro(GameObject gameObject, string text, float fontSize, Color color, TextAlignmentOptions alignment, Vector2 rectTransformSizeDelta)
        {
            TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = color;
            textMeshPro.alignment = alignment;
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts/IBMPlexSansArabic"); // Set the font to IBMPlexSansArabic inside the "Fonts" folder
            gameObject.GetComponent<RectTransform>().sizeDelta = rectTransformSizeDelta;
            return textMeshPro;
        }

        public static Color32 CreateHexToColor(string hex)
        {
            hex = hex.Replace("0x", "");
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            // Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }

        public static LineRenderer CreateLineRenderer(GameObject gameObject, float startWidth, float endWidth, int positionCount, Vector3 startPosition, Vector3 endPosition, Color color)
        {
            // NOTE: unity editor: Edit->Project Settings-> Graphics Then in the inspector where it says "Always Included Shaders" add "Unlit/Color"
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
            lineRenderer.positionCount = positionCount;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = color // Set the material color directly
            };

            return lineRenderer;
        }

        public static void CreateInclinationLine(PlanetData planet, GameObject planetInstance, LocalizationManager localizationManager)
        {
            // Create a parent game object for text and y-axis line. This object doesn't rotate.
            GameObject parentObject = new GameObject(planet.name + "_InclinationLinesParent");
            parentObject.transform.SetParent(planetInstance.transform, false);
            parentObject.transform.localPosition = Vector3.zero;

            // Make sure the parentObject always faces the camera
            parentObject.AddComponent<FaceCamera>();

            GameObject inclinationLine = CreateGameObject(planet.name + "_InclinationLine", planetInstance, Vector3.zero, Quaternion.Euler(planet.obliquityToOrbit, 0f, 0f));
            CreateLineRenderer(inclinationLine, 0.01f, 0.01f, 2, Vector3.down, Vector3.up, Color.yellow); // Add color parameter

            inclinationLine.SetActive(false);

            GameObject inclinationTextObject = CreateGameObject(planet.name + "_InclinationLineText", parentObject, Vector3.up * 1.1f, Quaternion.identity);
            TextMeshPro inclinationText = CreateTextMeshPro(inclinationTextObject, null, 4.25f, Color.white, TextAlignmentOptions.Center, new Vector2(1.5f, 1.5f));
            inclinationText.text = localizationManager.GetLocalizedValue("Degrees", inclinationText, true, Constants.ColorGreen, planet.obliquityToOrbit.ToString("F2"));

            inclinationTextObject.SetActive(false);

            GameObject yAxisGameObject = CreateGameObject(planet.name + "_YAxis", parentObject, Vector3.zero, Quaternion.identity);

            if (Mathf.Abs(planet.obliquityToOrbit) > 2f)
            {
                CreateLineRenderer(yAxisGameObject, 0.01f, 0.01f, 2, Vector3.down, Vector3.up, Color.white); // Add color parameter
            }
            parentObject.SetActive(false);

        }

        public static void CreatePlanetName(PlanetData planet, GameObject planetInstance, LocalizationManager localizationManager)
        {
            GameObject parentObject = new GameObject(planet.name + "_PlanetNameParent");
            parentObject.transform.SetParent(planetInstance.transform, false);
            parentObject.transform.localPosition = Vector3.zero;
            // Make sure the parentObject always faces the camera
            parentObject.AddComponent<FaceCamera>();

            GameObject planetTextObject = CreateGameObject($"{planet.name}_PlanetName", parentObject, Vector3.down * 1.1f, Quaternion.identity);
            TextMeshPro planetNameTextMeshPro = CreateTextMeshPro(planetTextObject, null, 4.25f, Color.white, TextAlignmentOptions.Center, new Vector2(2f, 2f));
            planetNameTextMeshPro.text = localizationManager.GetLocalizedValue(planet.name, planetNameTextMeshPro, true, Constants.ColorWhite );
            parentObject.SetActive(false);
        }

        public static void CreateOrbitLine(GameObject planet, CelestialBodyData body, Func<CelestialBodyData, float, Vector3> calculatePosition)
        {
            GameObject orbitLine = new GameObject($"{body.name}_Orbit_Line");
            LineRenderer lineRenderer = orbitLine.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = CreateHexToColor(body.planetColor).ToUnityColor()
            };

            //lineRenderer.widthMultiplier = body.diameter * sizeScale * diameterScaleFactor;
            lineRenderer.widthMultiplier = 0.01f;
            body.orbitLineRenderer = lineRenderer;

            lineRenderer.positionCount = 360;
            orbitLine.transform.SetParent(planet.transform);
            float angleStep = 360f / lineRenderer.positionCount;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                float angle = i * angleStep;
                Vector3 position = calculatePosition(body, angle);
                lineRenderer.SetPosition(i, position);
            }

            orbitLine.SetActive(true);

        }
        
        public static void CreateDirectionalLight(Transform sunTransform, float distanceScale, IReadOnlyDictionary<string, PlanetData> localPlanetDataDictionary, List<string> allowedPlanets)
        {
            directionalLight = new GameObject("Directional Light");
            if (!directionalLight.TryGetComponent(out Light lightComponent))
            {
                lightComponent = directionalLight.AddComponent<Light>();
            }
            lightComponent.type = LightType.Point;
            lightComponent.color = Color.white;
            lightComponent.intensity = 1.0f;

            // Find the last planet from the localPlanetDataDictionary order
            string lastAllowedPlanet = localPlanetDataDictionary.Keys.Reverse().FirstOrDefault(allowedPlanets.Contains);

            if (lastAllowedPlanet != null && localPlanetDataDictionary.TryGetValue(lastAllowedPlanet, out PlanetData lastPlanetData))
            {
                float distanceToLastPlanet = lastPlanetData.distanceFromSun * distanceScale * 10;
                Debug.Log("Distance from Sun to " + lastPlanetData.name + ": " + distanceToLastPlanet);

                // Adjust the range of the directional light
                lightComponent.range = distanceToLastPlanet;
            }

            directionalLight.transform.SetParent(sunTransform);
            directionalLight.transform.localPosition = Vector3.zero;
        }
    }
}
