using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
    public Vector3 rotationAxis;
    public float rotationSpeed;
    public Vector3 centerPoint;

    private void Update()
    {
        transform.RotateAround(centerPoint, rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
