using UnityEngine;

public class KeplerOrbit : MonoBehaviour
{
    public Transform orbitAround; // The object to orbit around, in this case, the Sun
    public float semiMajorAxis; // Semi-major axis of the orbit
    public float eccentricity; // Eccentricity of the orbit
    public float inclination; // Inclination of the orbit
    public float speedFactor; // Speed factor to control the speed of the orbit

    private float orbitSpeed; // Computed based on the semi-major axis
    private float orbitProgress; // The current progress of the orbit

    void Start()
    {
        // Calculate the orbit speed based on the semi-major axis
        orbitSpeed = Mathf.Sqrt(1 / semiMajorAxis) * speedFactor;
    }

    void Update()
    {
        // Update the progress of the orbit
        orbitProgress += orbitSpeed * Time.deltaTime;

        // Calculate the current position in the orbit
        float angle = orbitProgress * 2 * Mathf.PI;
        float r = semiMajorAxis * (1 - Mathf.Pow(eccentricity, 2)) / (1 + eccentricity * Mathf.Cos(angle));
        Vector3 position = new Vector3(r * Mathf.Cos(angle), r * Mathf.Sin(angle) * Mathf.Sin(Mathf.Deg2Rad * inclination), r * Mathf.Sin(angle) * Mathf.Cos(Mathf.Deg2Rad * inclination));

        // Update the position and rotation of the object
        transform.position = orbitAround.position + position;
        transform.RotateAround(orbitAround.position, Vector3.up, orbitSpeed * Time.deltaTime);
    }
}
