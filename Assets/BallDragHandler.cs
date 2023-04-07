using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

public class BallDragHandler : BasePressInputHandler
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private ForceVisualizer forceVisualizerPrefab;


    private Camera mainCamera;
    private GameObject ballInstance;
    private Rigidbody ballRigidbody;
    private ForceVisualizer forceVisualizer;
    private Vector3 offset;
    private Vector3 initialPosition;
    private bool dragging;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    protected override void OnPressBegan(Vector3 position)
    {
        if (ballInstance == null)
        {
            List<ARRaycastHit> arHits = new List<ARRaycastHit>();
            if (ARManager.Instance.ARRaycastManager.Raycast(position, arHits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                ballInstance = Instantiate(ballPrefab, arHits[0].pose.position, Quaternion.identity);
                ballRigidbody = ballInstance.GetComponent<Rigidbody>();
                forceVisualizer = Instantiate(forceVisualizerPrefab);
                forceVisualizer.HideForce();
            }
        }
        else
        {
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(ballInstance.transform.position);
            offset = ballInstance.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, screenPoint.z));
            initialPosition = ballInstance.transform.position;
            ballRigidbody.isKinematic = true;
            dragging = true;
        }
    }

    protected override void OnPress(Vector3 position)
    {
        if (dragging)
        {
            Vector3 mousePosition = Pointer.current.position.ReadValue();
            Vector3 screenPoint = new Vector3(mousePosition.x, mousePosition.y, mainCamera.WorldToScreenPoint(initialPosition).z);
            List<ARRaycastHit> arHits = new List<ARRaycastHit>();

            if (ARManager.Instance.ARRaycastManager.Raycast(screenPoint, arHits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Vector3 newPosition = arHits[0].pose.position + offset;
                ballRigidbody.MovePosition(newPosition);

                Vector3 force = newPosition - initialPosition;
                forceVisualizer.ShowForce(newPosition, force);
            }
        }
    }

    protected override void OnPressCancel()
    {
        if (dragging)
        {
            ballRigidbody.isKinematic = false;
            dragging = false;
            forceVisualizer.HideForce();
        }
    }
}
