using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
    public float speed = 20f;
    public Vector3 axis = Vector3.up;
    void Update()
    {
        transform.RotateAround(transform.parent.position, axis, speed * Time.deltaTime);
    }
}