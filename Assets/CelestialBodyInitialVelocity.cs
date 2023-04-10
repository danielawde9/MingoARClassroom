using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CelestialBodyInitialVelocity : MonoBehaviour
{
    public float initialVelocity;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // Calculate the direction of the initial velocity
        Vector3 direction = Vector3.Cross(transform.position, Vector3.up).normalized;

        // Set the initial velocity
        rb.velocity = direction * initialVelocity;

    }
}
