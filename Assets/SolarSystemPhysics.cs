using System.Collections.Generic;
using UnityEngine;

public class SolarSystemPhysics : MonoBehaviour
{
    public List<Rigidbody> celestialBodies;
    public float gravitationalConstant = 3.358f * Mathf.Pow(10, -5);

    private void Start()
    {
        GameObject[] celestialBodyObjects = GameObject.FindGameObjectsWithTag("CelestialBody");
        foreach (GameObject celestialBodyObject in celestialBodyObjects)
        {
            if (celestialBodyObject.TryGetComponent<Rigidbody>(out var rb))
            {
                celestialBodies.Add(rb);
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (var body1 in celestialBodies)
        {
            foreach (var body2 in celestialBodies)
            {
                if (body1 != body2)
                {
                    Vector3 direction = body1.position - body2.position;
                    float distance = direction.magnitude;
                    float forceMagnitude = gravitationalConstant * (body1.mass * body2.mass) / Mathf.Pow(distance, 2);
                    Vector3 force = direction.normalized * forceMagnitude;
                    body2.AddForce(force);
                }
            }
        }
    }
}
