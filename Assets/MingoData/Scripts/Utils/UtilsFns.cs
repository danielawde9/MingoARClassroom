using System;
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

        private static readonly Dictionary<string, Color> PlanetColorMap = new Dictionary<string, Color>
        {
            {"Mercury", new Color(0.5f, 0.5f, 0.5f)}, // Gray
            {"Venus", new Color(0.956f, 0.643f, 0.376f)}, // Yellowish-brown
            {"Earth", new Color(0.118f, 0.565f, 1.0f)}, // Blue
            {"Mars", new Color(0.8f, 0.4f, 0.2f)}, // Reddish-brown
            {"Jupiter", new Color(0.682f, 0.667f, 0.584f)}, // Brown
            {"Saturn", new Color(0.882f, 0.882f, 0.690f)}, // Pale gold
            {"Uranus", new Color(0.667f, 0.855f, 0.882f)}, // Pale blue
            {"Neptune", new Color(0.118f, 0.565f, 1.0f)} // Blue
        };

        public static Color GetPlanetColor(string planetName)
        {
            // Return a default color in case the planet name is not found
            return PlanetColorMap.TryGetValue(planetName, out Color color) ? color : Color.white;
            
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

        public static void CreateInclinationLine(SolarSystemSimulationWithMoons.PlanetData planet, GameObject planetInstance, LocalizationManager localizationManager)
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

        public static void CreatePlanetName(SolarSystemSimulationWithMoons.PlanetData planet, GameObject planetInstance, LocalizationManager localizationManager)
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
                color = Color.white // Set the material color directly
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
        
        public static void CreateDirectionalLight(Transform sunTransform, float distanceScale, IReadOnlyDictionary<string, SolarSystemSimulationWithMoons.PlanetData> localPlanetDataDictionary, List<string> allowedPlanets)
        {
            directionalLight = new GameObject("Directional Light");
            if (!directionalLight.TryGetComponent(out Light lightComponent))
            {
                lightComponent = directionalLight.AddComponent<Light>();
            }
            lightComponent.type = LightType.Point;
            lightComponent.color = Color.white;
            lightComponent.intensity = 1.0f;

            // todo later on fix it
            // List of planets in the correct order
            List<string> planetOrder = new List<string> { "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto" };

            // Find the last planet from the order which is also in the allowedPlanets list and the localPlanetDataDictionary
            string lastAllowedPlanet = planetOrder.LastOrDefault(planet => allowedPlanets.Contains(planet) && localPlanetDataDictionary.ContainsKey(planet));

            if (lastAllowedPlanet != null && localPlanetDataDictionary.TryGetValue(lastAllowedPlanet, out SolarSystemSimulationWithMoons.PlanetData lastPlanetData))
            {
                float distanceToPluto = lastPlanetData.distanceFromSun * distanceScale * 10;
                Debug.Log("Distance from Sun to " + lastPlanetData.name + ": " + distanceToPluto);

                // Adjust the range of the directional light
                lightComponent.range = distanceToPluto;
            }

            directionalLight.transform.SetParent(sunTransform);
            directionalLight.transform.localPosition = Vector3.zero;
        }


    }

}
