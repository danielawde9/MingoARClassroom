using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static SolarSystemSimulationWithMoons;

public class SolarSystemUtility
{
    public static GameObject directionalLight;
    public static PlanetDataList planetDataList;
    public static Dictionary<string, PlanetData> planetDataDictionary;
    private static Dictionary<string, Color> planetColorLegend = new Dictionary<string, Color>();

    public static Vector3 CalculatePlanetPosition(PlanetData planet, float angle, float distanceScale)
    {
        float radians = angle * Mathf.Deg2Rad;

        float semiMajorAxis = planet.distanceFromSun;
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - planet.orbitalEccentricitySquared);

        float eccentricAnomaly = 2 * Mathf.Atan(Mathf.Tan(radians / 2) * Mathf.Sqrt((1 - planet.orbitalEccentricitySquared) / (1 + planet.orbitalEccentricity)));
        float distance = semiMajorAxis * (1 - planet.orbitalEccentricitySquared) / (1 + planet.orbitalEccentricity * Mathf.Cos(eccentricAnomaly));

        float x = distance * Mathf.Cos(eccentricAnomaly);
        float z = distance * Mathf.Sin(eccentricAnomaly);

        // Apply the orbital inclination
        float y = Mathf.Sin(eccentricAnomaly) * semiMinorAxis * Mathf.Tan(planet.orbitalInclination * Mathf.Deg2Rad);

        return new Vector3(x * distanceScale, y * distanceScale, z * distanceScale);
    }

    public static void CreateInclinationLineAndPlanetName(PlanetData planet, GameObject planetInstance)
    {
        // Create a parent game object for text and y-axis line. This object doesn't rotate.
        GameObject parentObject = new(planet.name + "_FaceCameraGameObjects");
        parentObject.transform.SetParent(planetInstance.transform, false);
        parentObject.transform.localPosition = Vector3.zero;

        // Make sure the parentObject always faces the camera
        parentObject.AddComponent<FaceCamera>();

        GameObject inclinationLine = CreateGameObject(planet.name + "_InclinationLine", planetInstance, Vector3.zero, Quaternion.Euler(planet.obliquityToOrbit, 0f, 0f));
        CreateLineRenderer(inclinationLine, 0.01f, 0.01f, 2, Vector3.down, Vector3.up, Color.yellow); // Add color parameter

        GameObject inclinationTextObject = CreateGameObject(planet.name + "_InclinationLineText", parentObject, Vector3.up * 1.1f, Quaternion.identity);
        CreateTextMeshPro(inclinationTextObject, planet.obliquityToOrbit.ToString("F2") + "°", 4.25f, Color.white, TextAlignmentOptions.Center, new Vector2(1.5f, 1.5f));

        GameObject yAxisGameObject = CreateGameObject(planet.name + "_YAxis", parentObject, Vector3.zero, Quaternion.identity);

        if (Mathf.Abs(planet.obliquityToOrbit) > 2f)
        {
            CreateLineRenderer(yAxisGameObject, 0.01f, 0.01f, 2, Vector3.down, Vector3.up, Color.white); // Add color parameter
        }


        GameObject planetTextObject = CreateGameObject($"{planet.name}_PlanetName", parentObject, Vector3.down * 1.1f, Quaternion.identity);
        CreateTextMeshPro(planetTextObject, planet.name, 4.25f, Color.white, TextAlignmentOptions.Center, new Vector2(2f, 2f));
    }

    public static void CreateOrbitLine(GameObject planet, CelestialBodyData body, Func<CelestialBodyData, float, Vector3> calculatePosition)
    {
        GameObject orbitLine = new($"{body.name}_Orbit_Line");
        LineRenderer lineRenderer = orbitLine.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.white; // Set the material color directly

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
    }

    public static void CreateDirectionalLight(Transform sunTransform, float distanceScale, Dictionary<string, PlanetData> planetDataDictionary)
    {
        directionalLight = new GameObject("Directional Light");
        if (!directionalLight.TryGetComponent<Light>(out Light lightComponent))
        {
            lightComponent = directionalLight.AddComponent<Light>();
        }
        lightComponent.type = LightType.Point;
        lightComponent.color = Color.white;
        lightComponent.intensity = 1.0f;
        if (planetDataDictionary.TryGetValue("Pluto", out var plutoData))
        {
            float distanceToPluto = plutoData.distanceFromSun * distanceScale;
            Debug.Log("Distance from Sun to Pluto: " + distanceToPluto);

            // Adjust the range of the directional light
            // todo fix this range and add scale bs yekbar
            lightComponent.range = distanceToPluto;
        }

        directionalLight.transform.SetParent(sunTransform);
        directionalLight.transform.localPosition = Vector3.zero;
    }

    public static GameObject CreateGameObject(string name, GameObject parent, Vector3 localPosition, Quaternion localRotation)
    {
        GameObject newGameObject = new(name);
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
        gameObject.GetComponent<RectTransform>().sizeDelta = rectTransformSizeDelta;
        return textMeshPro;
    }

    public static LineRenderer CreateLineRenderer(GameObject gameObject, float startWidth, float endWidth, int positionCount, Vector3 startPosition, Vector3 endPosition, Color color)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = color; // Set the material color directly

        return lineRenderer;
    }

    public static void LoadPlanetData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystemWithMoon/planet_data_with_moon");
        planetDataList = JsonUtility.FromJson<PlanetDataList>(jsonFile.text);
        foreach (var planetData in planetDataList.planets)
        {
            planetData.rotationPeriod *= 3600; // convert hours to seconds
            planetData.orbitalPeriod *= 86400; // convert days to seconds
            planetData.perihelion *= 1E6f; // convert 10^6 km to km
            planetData.aphelion *= 1E6f; // convert 10^6 km to km
            planetData.distanceFromSun *= 1E6f; // convert 10^6 km to km
            planetData.orbitalEccentricitySquared = Mathf.Pow(planetData.orbitalEccentricity, 2);

        }
        planetDataDictionary = planetDataList.planets.ToDictionary(p => p.name, p => p);
    }


    public static void CreateDistanceFromSunLine(GameObject parentDistanceLinesObject, PlanetData planet)
    {

        GameObject parentObject = CreateGameObject($"{planet.name}_ParentDistanceInfo", parentDistanceLinesObject, Vector3.zero, Quaternion.identity);

        // Create line renderer and text mesh for displaying distance from the sun
        GameObject lineObject = CreateGameObject($"{planet.name}_DistanceLine", parentObject, Vector3.zero, Quaternion.identity);
        Color planetLineColor = GetRandomPlanetLineColor();

        planetColorLegend.Add(planet.name, planetLineColor);
        planet.distanceLineRenderer = CreateLineRenderer(lineObject, 0.01f, 0.01f, 2, Vector3.zero, planet.celestialBodyInstance.transform.position, planetLineColor);

        GameObject textDistanceTextObject = CreateGameObject($"{planet.name}_DistanceText", parentObject, Vector3.zero, Quaternion.identity);
        textDistanceTextObject.AddComponent<FaceCamera>();

        planet.distanceText = CreateTextMeshPro(textDistanceTextObject, "", 4.25f, planetLineColor, TextAlignmentOptions.Center, new Vector2(2.0f, 2.0f));

        GameObject planetNameObject = CreateGameObject($"{planet.name}_PlanetName", parentObject, Vector3.zero, Quaternion.identity);
        planetNameObject.AddComponent<FaceCamera>();

        planet.distanceTextPlanetName = CreateTextMeshPro(planetNameObject, "", 4.25f, planetLineColor, TextAlignmentOptions.Center, new Vector2(2.0f, 2.0f));
        planet.distanceTextPlanetName.text = planet.name;
    }

    public static Dictionary<string, Color> GetPlanetColorLegend()
    {
        return planetColorLegend;
    }

    private static Color GetRandomPlanetLineColor()
    {
        // Generate a random line color for each planet
        System.Random random = new();
        float r = (float)random.NextDouble();
        float g = (float)random.NextDouble();
        float b = (float)random.NextDouble();
        return new Color(r, g, b);
    }


    public static void UpdateDistanceFromSunLine(PlanetData planetData)
    {
        planetData.distanceLineRenderer.SetPosition(0, Vector3.zero);
        planetData.distanceLineRenderer.SetPosition(1, planetData.celestialBodyInstance.transform.position - planetData.distanceLineRenderer.transform.position);

        planetData.distanceText.transform.position = planetData.celestialBodyInstance.transform.position / 2f + new Vector3(0,-0.5f) ; 
        planetData.distanceTextPlanetName.transform.position = planetData.celestialBodyInstance.transform.position / 2f + new Vector3(0, 0.5f); 
        planetData.distanceText.text = $"{planetData.celestialBodyInstance.transform.position.magnitude:N1} KM";
    }

    public static void UpdateOrbitLine(CelestialBodyData body, Func<CelestialBodyData, float, Vector3> calculatePosition)
    {
        if (body.orbitLineRenderer != null)
        {
            float angleStep = 360f / body.orbitLineRenderer.positionCount;
            for (int i = 0; i < body.orbitLineRenderer.positionCount; i++)
            {
                float angle = i * angleStep;
                Vector3 position = calculatePosition(body, angle);
                body.orbitLineRenderer.SetPosition(i, position);
            }
        }
    }
}
