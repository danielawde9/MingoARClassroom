using System;
using System.Collections.Generic;
using System.Linq;
using MingoData.Scripts.Utils;
using UnityEngine;
using static MingoData.Scripts.SolarSystemSimulationWithMoons;

namespace MingoData.Scripts.MainUtil
{
    public abstract class SolarSystemUtility
    {
        private static PlanetDataList _planetDataList;
        public static Dictionary<string, PlanetData> planetDataDictionary;

        public static void AssignDirectionalLight(Transform planetInstance, float distanceScale, List<string> desiredPlanets)
        {
            UtilsFns.CreateDirectionalLight(planetInstance, distanceScale, planetDataDictionary, desiredPlanets);
            GameObject localDirectionalLight = GameObject.Find("Directional Light");
            if (localDirectionalLight == null)
                return;
            localDirectionalLight.transform.SetParent(planetInstance);
            localDirectionalLight.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
        }

        public static void InitPlanetProgress(PlanetData planet)
        {
            planet.rotationSpeed = 360f / planet.rotationPeriod;
            planet.orbitProgress = 0f;
            planet.rotationProgress = 0f;
        }

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

        public static void LoadPlanetData(List<string> desiredPlanets)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystemWithMoon/planet_data_with_moon");
            _planetDataList = JsonUtility.FromJson<PlanetDataList>(jsonFile.text);
            foreach (PlanetData planetData in _planetDataList.planets.Where(planetData => desiredPlanets.Contains(planetData.name)))
            {
                planetData.rotationPeriod *= 3600; // convert hours to seconds
                planetData.orbitalPeriod *= 86400; // convert days to seconds
                planetData.perihelion *= 1E6f; // convert 10^6 km to km
                planetData.aphelion *= 1E6f; // convert 10^6 km to km
                planetData.distanceFromSun *= 1E6f; // convert 10^6 km to km
                planetData.orbitalEccentricitySquared = Mathf.Pow(planetData.orbitalEccentricity, 2);
            }
            planetDataDictionary = _planetDataList.planets
                    .Where(p => desiredPlanets.Contains(p.name))
                    .ToDictionary(p => p.name, p => p);
        }

   

        public static void UpdateDistanceFromSunLine(PlanetData planetData)
        {
            planetData.distanceLineRenderer.SetPosition(0, Vector3.zero);
            Vector3 position = planetData.celestialBodyInstance.transform.position;
            planetData.distanceLineRenderer.SetPosition(1, position - planetData.distanceLineRenderer.transform.position);
            planetData.distanceText.transform.position = position / 2f + new Vector3(0, -0.5f);

        }

        public static void UpdateDistanceFromSunText(PlanetData planetData, LocalizationManager localizationManager)
        {
            float distanceInKm = planetData.celestialBodyInstance.transform.position.magnitude * Mathf.Pow(10, 6);
            string formattedDistance = distanceInKm.ToString("N0");
            planetData.distanceText.text = localizationManager.GetLocalizedValue("Distance_In_KM", planetData.distanceText, false, formattedDistance);
        }


        public static void UpdateOrbitLine(CelestialBodyData body, Func<CelestialBodyData, float, Vector3> calculatePosition)
        {
            if (body.orbitLineRenderer == null)
                return;
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
