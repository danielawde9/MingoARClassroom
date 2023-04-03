using System.Collections;
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

    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject spawnedObject { get; private set; }

    ARRaycastManager m_RaycastManager;
    Camera mainCamera;

    bool globePlaced = false;

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
    }

    void PlaceGlobe(Vector2 touchPosition)
    {
        List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
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

                // Change the color of the country
                ChangeCountryColor(hitObject);

                // Set the original and target positions for the lerp animation
                Vector3 originalPosition = hitObject.transform.position;
                Vector3 targetPosition = originalPosition + Vector3.up * 0.1f; // adjust the height as desired

                // Start the lerp animation
                StartCoroutine(MoveObject(hitObject.transform, originalPosition, targetPosition, 0.5f));
            }
        }
    }
    IEnumerator MoveObject(Transform objectTransform, Vector3 startPos, Vector3 endPos, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / duration;
            objectTransform.position = Vector3.Lerp(startPos, endPos, normalizedTime);
            yield return null;
        }
    }


    void ChangeCountryColor(GameObject country)
    {
        Renderer renderer = country.GetComponent<Renderer>();
        Color originalColor = renderer.material.color;
        renderer.material.color = Color.yellow; // Set the color to yellow, or any other color you prefer
    }

}
