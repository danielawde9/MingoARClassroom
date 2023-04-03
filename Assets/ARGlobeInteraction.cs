using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARGlobeInteraction : MonoBehaviour
{
    public Color selectedCountryColor = Color.yellow;

    private ARRaycastManager arRaycastManager;
    private Color originalCountryColor;
    private Renderer previouslySelectedCountryRenderer;

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.touches[0].position;
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                if (previouslySelectedCountryRenderer != null)
                {
                    // Restore the original color of the previously selected country
                    previouslySelectedCountryRenderer.material.color = originalCountryColor;
                }

                // Store the original color of the clicked country
                //Renderer clickedCountryRenderer = hits[0].pose.HitTest().collider.gameObject.GetComponent<Renderer>();
                //originalCountryColor = clickedCountryRenderer.material.color;

                //// Change the color of the clicked country
                //clickedCountryRenderer.material.color = selectedCountryColor;

                //// Update the previouslySelectedCountryRenderer variable
                //previouslySelectedCountryRenderer = clickedCountryRenderer;

                // Invoke your custom event here, if necessary
            }
        }
    }
}
