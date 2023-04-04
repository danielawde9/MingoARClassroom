using System.Collections.Generic;
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
    GameObject m_PlacedGlobePrefab;



    public GameObject PlacedUIQuizDashboardPrefab
    {
        get { return m_PlacedUIQuizDashboardPrefab; }
        set { m_PlacedUIQuizDashboardPrefab = value; }
    }


    public GameObject PlacedGlobePrefab
    {
        get { return m_PlacedGlobePrefab; }
        set { m_PlacedGlobePrefab = value; }
    }



    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject SpawnedObject { get; private set; }

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

        if (Pointer.current == null || m_Pressed == false)
            return;

        var touchPosition = Pointer.current.position.ReadValue();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;

            if (SpawnedObject == null)
            {
                SpawnedObject = Instantiate(m_PlacedUIQuizDashboardPrefab, hitPose.position, hitPose.rotation);

                Instantiate(m_PlacedGlobePrefab,hitPose.position+new Vector3(0f, 1.0f, 0f), hitPose.rotation);
               
            }
        }
    }

}
