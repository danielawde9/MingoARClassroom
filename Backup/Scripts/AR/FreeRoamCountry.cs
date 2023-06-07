using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
[RequireComponent(typeof(ARRaycastManager))]

public class FreeRoamCountry : BasePressInputHandler
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    private GameObject m_PlacedGlobePrefab;

    public GameObject PlacedGlobePrefab
    {
        get { return m_PlacedGlobePrefab; }
        set { m_PlacedGlobePrefab = value; }
    }

    [SerializeField]
    ARPlaneManager m_PlaneManager;

    public GameObject SpawnedObject { get; private set; }

    public TextMeshProUGUI scanningInfoText;


    private ARRaycastManager m_RaycastManager;
    private Camera mainCamera;
    private bool globePlaced = false;


    private bool isMoving = false;


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
    }


    void PlaceGlobe(Vector2 touchPosition)
    {
        List<ARRaycastHit> s_Hits = new();

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            PlaceGlobeAtPosition(hitPose.position);
        }
    }

    public void PlaceGlobeAtPosition(Vector3 position)
    {
        if (!globePlaced)
        {
            Vector3 placementPosition = position + new Vector3(0, m_PlacedGlobePrefab.transform.localScale.y/2, 0);
            SpawnedObject = Instantiate(m_PlacedGlobePrefab, placementPosition, Quaternion.identity);
            globePlaced = true;
        }
    }

    private void Update()
    {
        float totalArea = CalculateTotalPlaneArea();
        if (!globePlaced)
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

    void DetectCountryTouch(Vector2 touchPosition)
    {
        if (isMoving) return;

        Ray ray = mainCamera.ScreenPointToRay(touchPosition);

        int layerMask = 1 << LayerMask.NameToLayer("Countries");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Country")) // Make sure to set the tag "Country" on each country GameObject
            {
                Debug.Log("Country touched: " + hitObject.name);
                scanningInfoText.text = hitObject.name.ToString();
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



// Mechanics (motion, forces, energy, and momentum)
//Waves(sound, light, and electromagnetic waves)
//Thermodynamics(heat, temperature, and energy transfer)
//Electricity and magnetism (electric fields, circuits, and magnetic fields)
//Optics(reflection, refraction, and lenses)
//Mechanics(motion, forces, energy, and momentum):
//Create 3D objects that users can interact with to apply forces or change their properties (mass, velocity, etc.)
//Visualize motion paths, vectors representing forces, and other related properties.
//Waves (sound, light, and electromagnetic waves):
//Display waveforms in 3D space to show the propagation and interaction of waves.
//Enable users to adjust wave properties such as frequency, amplitude, and wavelength.
//Thermodynamics (heat, temperature, and energy transfer):
//Use color gradients or 3D particle systems to represent heat distribution within objects.
//Show energy transfer processes (conduction, convection, and radiation) using animated visuals.
//Electricity and magnetism (electric fields, circuits, and magnetic fields):
//Display electric field lines and potential surfaces around charged objects.
//Create interactive 3D circuit components (batteries, resistors, capacitors, etc.) for users to build and test circuits.
//Visualize magnetic field lines and forces around magnets and current - carrying conductors.
//Optics(reflection, refraction, and lenses):
//Create interactive 3D scenes with mirrors, lenses, and light sources to explore reflection, refraction, and image formation.
//Allow users to manipulate the position and properties of objects in the scene to see how light behaves in different scenarios.