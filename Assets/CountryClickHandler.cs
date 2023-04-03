using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class CountryClickHandler : PressInputBase
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    public GameObject PlacedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject SpawnedObject { get; private set; }

    ARRaycastManager m_RaycastManager;
    Camera mainCamera;

    float cooldownTimer = 0f;
    readonly float cooldownDuration = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    protected override void OnPress(Vector3 position)
    {
        if (cooldownTimer > 0) return;

        if (SpawnedObject == null)
        {
            PlaceGlobe(position);
        }
        else
        {
            DetectCountryTouch(position);
        }

        cooldownTimer = cooldownDuration;
    }

    void PlaceGlobe(Vector2 touchPosition)
    {
        List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            SpawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
        }
    }

    void DetectCountryTouch(Vector2 touchPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Country")) // Make sure to set the tag "Country" on each country GameObject
            {
                Debug.Log("Country touched: " + hitObject.name);
            }
        }
    }
}
