using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
/// <summary>
/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
///
/// If a raycast hits a trackable, the <see cref="PlacedUIQuizDashboardPrefab"/> is instantiated
/// and moved to the hit position.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class ARQuizDashboardAnchorPlacer : BasePressInputHandler
{
    [SerializeField]
    GameObject m_PlacedUIQuizDashboardPrefab;

    [SerializeField]
    ARPlaneManager m_PlaneManager;


    [SerializeField]
    private TextMeshProUGUI scanningInfoText;


    public GameObject SpawnedObject { get; private set; }

    public ARPlaneManager PlaneManager
    {
        get { return m_PlaneManager; }
        set { m_PlaneManager = value; }
    }

    public GameObject PlacedUIQuizDashboardPrefab
    {
        get { return m_PlacedUIQuizDashboardPrefab; }
        set { m_PlacedUIQuizDashboardPrefab = value; }
    }

    bool m_Pressed;
    protected override void OnPress(Vector3 position) => m_Pressed = true;

    protected override void OnPressCancel() => m_Pressed = false;

    static readonly List<ARRaycastHit> s_Hits = new();

    ARRaycastManager m_RaycastManager;

    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        float totalArea = CalculateTotalPlaneArea();
        Debug.Log($"Total area: {totalArea}, Planes detected: {m_PlaneManager.trackables.count}");

        if (Pointer.current == null || m_Pressed == false)
        {
            if (totalArea < 2 || m_PlaneManager.trackables.count < 2)
            {
                // Update the scanning info text
                if (scanningInfoText != null)
                {
                    scanningInfoText.text = $"You need to scan at least 2 meters of the room. Scanned: {totalArea:F2} m�";
                }
                else
                {
                    Debug.LogWarning("scanningInfoText is not assigned");
                }
            }
            else
            {
                if (SpawnedObject == null)
                {
                    // Show the text to press anywhere to place the dashboard
                    if (scanningInfoText != null)
                    {
                        scanningInfoText.text = "Press anywhere to place the dashboard";
                    }
                }
                else
                {
                    // Hide the scanning info text when the required area is scanned and the object is placed
                    if (scanningInfoText != null)
                    {
                        scanningInfoText.text = "";
                    }
                }
            }
            return;
        }

        var touchPosition = Pointer.current.position.ReadValue();

        // Minimum area of 2 square meters or 2 planes detected
        if (totalArea < 2 || m_PlaneManager.trackables.count < 2)
        {
            return;
        }


        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;

            if (SpawnedObject == null)
            {
                SpawnedObject = Instantiate(m_PlacedUIQuizDashboardPrefab, hitPose.position, hitPose.rotation);

                //Instantiate(m_PlacedGlobePrefab,hitPose.position+new Vector3(0f, 1.0f, 1.0f), hitPose.rotation);
               
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
