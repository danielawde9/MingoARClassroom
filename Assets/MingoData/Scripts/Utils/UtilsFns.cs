using System;
using System.Collections.Generic;
using MingoData.Scripts.MainUtil;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

namespace MingoData.Scripts.Utils
{
    public class UtilsFns : MonoBehaviour
    {
        private static readonly Dictionary<string, Color> PlanetColorLegend = new Dictionary<string, Color>();
      
        public static GameObject directionalLight;

        public static GameObject CreateDarkBackground()
        {
            // Create new GameObject
            GameObject darkBackground = new GameObject("DarkBackground");

            // Add it as a child of the parent canvas
            darkBackground.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

            // Add an Image component to the GameObject
            Image image = darkBackground.AddComponent<Image>();

            Button button = darkBackground.AddComponent<Button>();
            
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
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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



        public static void CreateDistanceLineAndTextFromSun(GameObject parentDistanceLinesObject, SolarSystemSimulationWithMoons.PlanetData planet)
        {
            if (parentDistanceLinesObject == null)
            {
                Debug.LogError("parentDistanceLinesObject is null");
                return;
            }

            if (planet == null)
            {
                Debug.LogError("planet is null");
                return;
            }

            GameObject parentObject = CreateGameObject($"{planet.name}_ParentDistanceInfo", parentDistanceLinesObject, Vector3.zero, Quaternion.identity);

            // Create line renderer and text mesh for displaying distance from the sun
            GameObject lineObject = CreateGameObject($"{planet.name}_DistanceLine", parentObject, Vector3.zero, Quaternion.identity);
            Color planetLineColor = CreateRandomPlanetLineColor();

            PlanetColorLegend.Add(planet.name, planetLineColor);
            planet.distanceLineRenderer = CreateLineRenderer(lineObject, 0.01f, 0.01f, 2, Vector3.zero, planet.celestialBodyInstance.transform.position, planetLineColor);

            GameObject textDistanceTextObject = CreateGameObject($"{planet.name}_DistanceText", parentObject, Vector3.zero, Quaternion.identity);
            textDistanceTextObject.AddComponent<FaceCamera>();

            planet.distanceText = CreateTextMeshPro(textDistanceTextObject, "", 4.25f, planetLineColor, TextAlignmentOptions.Center, new Vector2(2.0f, 2.0f));

            parentObject.SetActive(false);
        }
       
        private static GameObject CreateGameObject(string name, GameObject parent, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject newGameObject = new GameObject(name);
            if (parent != null)
            {
                newGameObject.transform.SetParent(parent.transform, false);
            }
            newGameObject.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            return newGameObject;
        }

        private static Color CreateRandomPlanetLineColor()
        {
            // Generate a random line color for each planet
            Random random = new Random();
            float r = (float)random.NextDouble();
            float g = (float)random.NextDouble();
            float b = (float)random.NextDouble();
            return new Color(r, g, b);
        }

        private static TextMeshPro CreateTextMeshPro(GameObject gameObject, string text, float fontSize, Color color, TextAlignmentOptions alignment, Vector2 rectTransformSizeDelta)
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
        
        public static Dictionary<string, Color> GetPlanetColorLegend()
        {
            return PlanetColorLegend;
        }

        private static LineRenderer CreateLineRenderer(GameObject gameObject, float startWidth, float endWidth, int positionCount, Vector3 startPosition, Vector3 endPosition, Color color)
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
            inclinationText.text = localizationManager.GetLocalizedValue("Degrees", inclinationText, true, planet.obliquityToOrbit.ToString("F2"));

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
            planetNameTextMeshPro.text = localizationManager.GetLocalizedValue(planet.name, planetNameTextMeshPro, true);
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

            string lastAllowedPlanet = allowedPlanets[^1];

            if (localPlanetDataDictionary.TryGetValue(lastAllowedPlanet, out SolarSystemSimulationWithMoons.PlanetData lastPlanetData))
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
