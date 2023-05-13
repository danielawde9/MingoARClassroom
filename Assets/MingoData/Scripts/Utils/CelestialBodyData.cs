using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class CelestialBodyData
{
    public string name;
    public float mass;
    public float diameter;
    public float rotationPeriod;
    public float orbitalPeriod;
    public string prefabName;
    public float orbitalInclination;
    public float orbitalEccentricity;
    public float obliquityToOrbit;
    public GameObject celestialBodyInstance;
    public float rotationSpeed;
    public float orbitProgress;
    public float rotationProgress;
    public int completedOrbits;
    public int completedRotations;
    public Vector3 rotationAxis;
    public float perihelionDistance;
    public float aphelionDistance;
    public int completedSelfRotations;
   
}
