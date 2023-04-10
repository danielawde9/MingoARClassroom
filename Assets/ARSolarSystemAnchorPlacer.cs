using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARSolarSystemAnchorPlacer : BasePressInputHandler
{

    [SerializeField]
    GameObject m_SolarSystemPrefab;

    public GameObject SpawnedObject { get; private set; }


    public GameObject SolarSystemPrefab
    {
        get { return m_SolarSystemPrefab; }
        set { m_SolarSystemPrefab = value; }
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
        if (Pointer.current == null || m_Pressed == false)
            return;

        var touchPosition = Pointer.current.position.ReadValue();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;

            if (SpawnedObject == null)
            {
                if (SpawnedObject == null)
                {
                    SpawnedObject = Instantiate(m_SolarSystemPrefab, hitPose.position + new Vector3(0f, 0f, 0f), hitPose.rotation);
                }


            }
        }
    }
}
