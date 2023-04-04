using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]

public class FreeFoarmCountry : BasePressInputHandler
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    private GameObject m_PlacedPrefab;

    public GameObject PlacedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject SpawnedObject { get; private set; }

    private ARRaycastManager m_RaycastManager;
    private Camera mainCamera;
    private bool isMoving = false;

    private bool globePlaced = false;

    private class CountryData
    {
        public Color OriginalColor { get; set; }
        public bool IsLifted { get; set; }

    }
    public string SelectedCountryName { get; set; }

    private readonly Dictionary<GameObject, CountryData> countryData = new();

    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
        mainCamera = Camera.main;
    }

    protected override void OnPress(Vector3 position)
    {
        if (!globePlaced)
        {
            PlaceGlobe(position);
            globePlaced = true;
        }
        else
        {
            DetectCountryTouch(position);
        }

        DetectCountryTouch(position);
    }

    void PlaceGlobe(Vector2 touchPosition)
    {
        List<ARRaycastHit> s_Hits = new();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            Vector3 placementPosition = hitPose.position + new Vector3(0, m_PlacedPrefab.transform.localScale.y, 0);

            SpawnedObject = Instantiate(m_PlacedPrefab, placementPosition, hitPose.rotation);
        }
    }

    public void PlaceGlobeAtPosition(Vector3 position)
    {
        if (!globePlaced)
        {
            Vector3 placementPosition = position + new Vector3(0, m_PlacedPrefab.transform.localScale.y, 0);
            SpawnedObject = Instantiate(m_PlacedPrefab, placementPosition, Quaternion.identity);
            globePlaced = true;
        }
    }

    void DetectCountryTouch(Vector2 touchPosition)
    {
        if (isMoving) return;

        Ray ray = mainCamera.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Country")) // Make sure to set the tag "Country" on each country GameObject
            {
                Debug.Log("Country touched: " + hitObject.name);

                if (!countryData.TryGetValue(hitObject, out CountryData data))
                {
                    Renderer renderer = hitObject.GetComponent<Renderer>();
                    data = new CountryData
                    {
                        OriginalColor = renderer.material.color,
                        IsLifted = false
                    };
                    countryData[hitObject] = data;
                }

                // Change the color and position of the country
                ChangeCountryColorAndPosition(hitObject, hit, data);
            }
        }
    }

    void ChangeCountryColorAndPosition(GameObject country, RaycastHit hit, CountryData countryData)
    {


        Renderer renderer = country.GetComponent<Renderer>();
        renderer.material.color = countryData.IsLifted ? countryData.OriginalColor : Color.yellow;

        Vector3 originalPosition = country.transform.position;
        Vector3 direction = countryData.IsLifted ? -hit.normal.normalized : hit.normal.normalized;
        Vector3 targetPosition = originalPosition + direction * 0.1f;

        StartCoroutine(MoveObject(country.transform, originalPosition, targetPosition, 0.5f));

        countryData.IsLifted = !countryData.IsLifted;
        if (!countryData.IsLifted)
        {
            SelectedCountryName = country.name;
        }
        else
        {
            SelectedCountryName = null;
        }
    }



    IEnumerator MoveObject(Transform objectTransform, Vector3 startPos, Vector3 endPos, float duration)
    {
        isMoving = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / duration;
            objectTransform.position = Vector3.Lerp(startPos, endPos, normalizedTime);
            yield return null;
        }

        isMoving = false;
    }
}
