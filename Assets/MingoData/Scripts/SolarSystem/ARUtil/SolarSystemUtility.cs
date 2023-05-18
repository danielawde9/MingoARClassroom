using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SolarSystemSimulationWithMoons;

public class SolarSystemUtility
{
    public static GameObject directionalLight;

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

    public static void CreateInclinationLine(PlanetData planet, GameObject planetInstance)
    {
        // Create a parent game object for text and y-axis line. This object doesn't rotate.
        GameObject parentObject = new(planet.name + "_LabelParent");
        parentObject.transform.SetParent(planetInstance.transform, false);
        parentObject.transform.localPosition = Vector3.zero;

        // Make sure the parentObject always faces the camera
        parentObject.AddComponent<FaceCamera>();

        GameObject inclinationLine = CreateGameObject(planet.name + "_PlanetInfo", planetInstance, Vector3.zero, Quaternion.Euler(planet.obliquityToOrbit, 0f, 0f));
        CreateLineRenderer(inclinationLine, 0.01f, 0.01f, 2, Vector3.down, Vector3.up, Color.yellow); // Add color parameter

        GameObject inclinationTextObject = CreateGameObject(planet.name + "_InclinationText", parentObject, Vector3.up * 1.1f, Quaternion.identity);
        CreateTextMeshPro(inclinationTextObject, planet.obliquityToOrbit.ToString("F2") + "°", 4.25f, Color.white, TextAlignmentOptions.Center, new Vector2(1.5f, 1.5f));

        if (Mathf.Abs(planet.obliquityToOrbit) > 2f)
        {
            GameObject yAxisGameObject = CreateGameObject(planet.name + "_YAxis", parentObject, Vector3.zero, Quaternion.identity);
            CreateLineRenderer(yAxisGameObject, 0.01f, 0.01f, 2, Vector3.down, Vector3.up, Color.white); // Add color parameter
        }

        GameObject planetTextObject = CreateGameObject($"{planet.name}_Label", parentObject, Vector3.down * 1.1f, Quaternion.identity);
        CreateTextMeshPro(planetTextObject, planet.name, 4.25f, Color.white, TextAlignmentOptions.Center, new Vector2(1.5f, 1.5f));
    }

    public static void CreateOrbitLine(GameObject planet, CelestialBodyData body, Func<CelestialBodyData, float, Vector3> calculatePosition)
    {
        GameObject orbitLine = new($"{body.name} Orbit Line");
        LineRenderer lineRenderer = orbitLine.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.white; // Set the material color directly

        // lineRenderer.widthMultiplier = body.diameter * sizeScale * diameterScaleFactor;
        lineRenderer.widthMultiplier = 0.1f;
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
        newGameObject.transform.SetParent(parent.transform, false);
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

}
