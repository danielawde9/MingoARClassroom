using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    [SerializeField] private Rigidbody objectRigidbody;

    public void OnSelected(Pose hitPose)
    {
        // Apply force or change properties based on user input
        // Example: Apply force in the upward direction
        objectRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }
}
