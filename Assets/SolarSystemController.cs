using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SolarSystemController : MonoBehaviour
{
    [System.Serializable]
    public class CelestialBody
    {
        public string planetName;
        public float orbitSpeed;
        public float maxOrbitSpeed;
        public float selfRotationSpeed;
        public float inclination;
        public float semiMajorAxis;
        public float eccentricity;
        public float axialTilt;
        public float longitudeOfAscendingNode;
        public float meanAnomalyAtEpoch;
        public float argumentOfPerihelion;
        public float distanceFromSun;
    }
   

    [System.Serializable]
    public class CelestialBodyList
    {
        public List<CelestialBody> celestialBodies;
    }


    public string jsonFileName = "MingoData/Resources/SolarSystem/CelestialBodyData.json";
    private CelestialBodyList celestialBodyList;
    private PlanetController planetController;
    public float orbitAccelerationMultiplier =1.0f;
    
    private void Start()
    {
        // Load JSON data
        string filePath = Path.Combine(Application.dataPath, jsonFileName);
        string jsonString = File.ReadAllText(filePath);
        celestialBodyList = JsonUtility.FromJson<CelestialBodyList>(jsonString);

        foreach (CelestialBody celestialBody in celestialBodyList.celestialBodies)
        {
            GameObject planetObject = GameObject.Find(celestialBody.planetName);
            if (planetObject != null)
            {
                planetController = planetObject.GetComponent<PlanetController>();
                if (planetController != null)
                {
                    planetController.SetData(celestialBody);


                }
            }
        }
    }

}
