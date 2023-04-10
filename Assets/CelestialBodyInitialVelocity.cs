using UnityEngine;

public class CelestialBodyInitialVelocity : MonoBehaviour
{
    public float initialVelocity;
    public float speedFactor = 10.0f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Calculate the direction of the initial velocity
        Vector3 direction = Vector3.Cross(transform.position - transform.parent.position, Vector3.up).normalized;

        // Set the initial velocity
        rb.velocity = speedFactor * initialVelocity * direction / 299792.458f;

        // Add the parent's velocity
        if (transform.parent.TryGetComponent<Rigidbody>(out var parentRb))
        {
            rb.velocity += parentRb.velocity;
        }
    }

    private void Update()
    {
        // Calculate the direction of the initial velocity
        Vector3 direction = Vector3.Cross(transform.position - transform.parent.position, Vector3.up).normalized;

        // Update the velocity
        rb.velocity = speedFactor * initialVelocity * direction / 299792.458f;

        // Add the parent's velocity
        if (transform.parent.TryGetComponent<Rigidbody>(out var parentRb))
        {
            rb.velocity += parentRb.velocity;
        }
    }
}
