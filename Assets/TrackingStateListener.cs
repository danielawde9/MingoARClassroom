using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// this script used to track initially the user to scan the room and show them the image to tap on the scanned area
public class TrackingStateListener : BasePressInputHandler
{
    public ARPlaneManager planeManager;
    public GameObject initialObject;
    public GameObject objectAfterScan;
    //public GameObject objectAfterClick;

    [SerializeField]
    ARPlaneManager m_PlaneManager;
    private ARRaycastManager m_RaycastManager;

    public UIHandler uIHandler;

    private void Start()
    {
        ShowInitial();
    }

    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    private bool isAfterScanShown = false; 
    private bool isPlaneHit = false;

    protected override void OnPress(Vector3 touchPosition)
    {
        List<ARRaycastHit> s_Hits = new();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            isPlaneHit = true;
        }
        else
        {
            isPlaneHit = false;
        }
    }

    void Update()
    {
        float totalArea = CalculateTotalPlaneArea();

        if (Pointer.current != null && isPlaneHit)
        {
            ShowAfterClick();
            return;
        }

        if (totalArea >= 1 && m_PlaneManager.trackables.count >= 1)
        {
            if (!isAfterScanShown)
            {
                ShowAfterScan();
                isAfterScanShown = true;
            }
        }
        else
        {
            uIHandler.SetMenuTextTitle($"You need to scan at least 2 meters of the room. Scanned: {totalArea:F2} m²");
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

    private void ShowInitial()
    {
        initialObject.SetActive(true);
        uIHandler.SetMenuTextTitle("Move your phone to start scanning the room");
        objectAfterScan.SetActive(false);
        Debug.Log("show initial");

        // objectAfterClick.SetActive(false);
    }

    private void ShowAfterScan()
    {
        initialObject.SetActive(false);
        uIHandler.SetMenuTextTitle("Tap on the place the solar system ");
        objectAfterScan.SetActive(true);
        //objectAfterClick.SetActive(false);
        Debug.Log("show after scan");

    }

    private void ShowAfterClick()
    {
        initialObject.SetActive(false);
        objectAfterScan.SetActive(false);
        Debug.Log("show after click");
        uIHandler.SetMenuTextTitle("Click on any planet or click on the menu below to display more settings");
        //objectAfterClick.SetActive(true);
    }
}
// todo move the string to constants 