using UnityEngine;

public class BillboardText : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        transform.forward = mainCamera.transform.forward;
    }
}