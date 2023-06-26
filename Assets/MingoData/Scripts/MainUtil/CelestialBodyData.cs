using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MingoData.Scripts.MainUtil
{

    [Serializable]
    public class CelestialBodyData
    {
        public string name;
        public float diameter;
        public float rotationSpeed;
        public float rotationPeriod;
        public float lengthOfDay;
        public float perihelion;
        public float aphelion;
        public float orbitalPeriod;
        public float orbitalVelocity;
        public string planetColor;
        public float orbitalInclination;
        public float orbitalEccentricity;
        public float obliquityToOrbit;
        public float orbitProgress;
        public float rotationProgress;
        public int completedOrbits;
        public int completedSelfRotations;
        public float orbitalEccentricitySquared;
        public string prefabName;
        public Vector3 rotationAxis;
        public GameObject celestialBodyInstance;
        public LineRenderer orbitLineRenderer;

    }
    
    [Serializable]
    public class PlanetData : CelestialBodyData
    {
        public List<MoonData> moons;
        public float distanceFromSun;

        public GameObject planetGuidance;
        public RectTransform planetGuidanceRectTransform;
        public Image planetGuidanceImage;

        [NonSerialized] public LineRenderer distanceLineRenderer;
        [NonSerialized] public TextMeshPro distanceText;
    }

    [Serializable]
    public class MoonData : CelestialBodyData
    {
        public float distanceFromPlanet;
    }

    [Serializable]
    public class PlanetDataList
    {
        public List<PlanetData> planets;
    }

}
/*
diameter in km
rotation period in hours 
lenght of day also in hours
distance from sun 10^6 km 
Perihelion (10^6 km)
Aphelion (10^6 km)
Orbital Period (days)
Orbital Velocity (km/s)	
Orbital Inclination (degrees)
Obliquity to Orbit (degrees)
rotation speed degrees per second
*/