using System;
using System.Collections.Generic;
using UnityEngine;


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
    public float orbitalInclination;
    public float orbitalEccentricity;
    public float obliquityToOrbit;
    public string prefabName;
    public GameObject celestialBodyInstance;
    public float orbitProgress;
    public float rotationProgress;
    public int completedOrbits;
    public int completedRotations;
    public Vector3 rotationAxis;
    public int completedSelfRotations;
    public float orbitalEccentricitySquared;
    public LineRenderer orbitLineRenderer;

}
