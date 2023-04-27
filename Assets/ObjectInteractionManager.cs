using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectInteractionManager : BasePressInputHandler
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    private GameObject m_PlacedCubePrefab;

    public GameObject PlacedCubePrefab
    {
        get { return m_PlacedCubePrefab; }
        set { m_PlacedCubePrefab = value; }
    }

    [SerializeField]
    ARPlaneManager m_PlaneManager;

    public GameObject SpawnedObject { get; private set; }
    private GameObject selectedObject;

    public TextMeshProUGUI scanningInfoText;

    private ARRaycastManager m_RaycastManager;
    private Camera mainCamera;
    private bool cubePlaced = false;

    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
        mainCamera = Camera.main;
    }
    protected override void OnPress(Vector3 position)
    {
        if (!cubePlaced)
        {
            PlaceCube(position);
            cubePlaced = true;
        }
        else
        {
            DetectCubeTouch(position);
        }
    }

    void PlaceCube(Vector2 touchPosition)
    {
        List<ARRaycastHit> s_Hits = new();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            PlaceCubeAtPosition(hitPose.position);
        }
    }
    public void PlaceCubeAtPosition(Vector3 position)
    {
        if (!cubePlaced)
        {
            Vector3 placementPosition = position + new Vector3(0, m_PlacedCubePrefab.transform.localScale.y / 2, 0);
            SpawnedObject = Instantiate(m_PlacedCubePrefab, placementPosition, Quaternion.identity);
            cubePlaced = true;
        }
    }

    private void Update()
    {
        float totalArea = CalculateTotalPlaneArea();
        if (!cubePlaced)
        {
            if (totalArea < 2)
            {
                scanningInfoText.text = $"You need to scan at least 2 meters of the room. Scanned: {totalArea:F2} m²";
            }
            else
            {
                scanningInfoText.text = "You can place the globe in the scanned area.";
            }
        }

        if (selectedObject != null && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            MoveSelectedObject(Input.GetTouch(0).position);
        }
    }
    void MoveSelectedObject(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (m_RaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            selectedObject.transform.position = hitPose.position;
        }
    }
    void DetectCubeTouch(Vector2 touchPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == SpawnedObject)
            {
                selectedObject = SpawnedObject;
            }
        }
    }


    private float CalculateTotalPlaneArea()
    {
        float totalArea = 0f;
        foreach (var plane in m_PlaneManager.trackables)
        {
            totalArea += plane.size.x * plane.size.y;
        }
        return totalArea;
    }
}