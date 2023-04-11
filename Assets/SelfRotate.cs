using UnityEngine;

public class SelfRotate : MonoBehaviour
{
    public float rotationSpeed = 1f;
    private void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
