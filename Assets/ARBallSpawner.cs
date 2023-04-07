using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARBallSpawner : BasePressInputHandler
{
    public GameObject ballPrefab;
    private GameObject spawnedBall;
    private ARRaycastManager arRaycastManager;
    private static List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    protected override void Awake()
    {
        base.Awake();
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    protected override void OnPress(Vector3 position)
    {
        if (spawnedBall != null)
        {
            return;
        }

        if (arRaycastManager.Raycast(position, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycastHits[0].pose;
            spawnedBall = Instantiate(ballPrefab, hitPose.position, Quaternion.identity);
            spawnedBall.AddComponent<BallDragHandler>();
        }
    }
}
