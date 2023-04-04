using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class CountryClickHandler : MonoBehaviour
{
    public string SelectedCountryName { get; set; }
    private bool isLifting = false;

    private Dictionary<string, CountryData> countriesData = new Dictionary<string, CountryData>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isLifting)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Create a layer mask to only detect objects on the "Countries" layer
            int layerMask = 1 << LayerMask.NameToLayer("Countries");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))

            {
                GameObject country = hit.collider.gameObject;

                Debug.Log(country.name);
                if (!countriesData.ContainsKey(country.name))
                {
                    Material material = country.GetComponent<Renderer>().material;
                    Color color = material.shader.name == "Standard" ? material.GetColor("_Color") : material.color;
                    countriesData[country.name] = new CountryData(color);


                }

                StartCoroutine(ChangeCountryColorAndPosition(country, hit, countriesData[country.name]));
            }
        }
    }

    IEnumerator ChangeCountryColorAndPosition(GameObject country, RaycastHit hit, CountryData countryData)
    {
        isLifting = true;

        Renderer renderer = country.GetComponent<Renderer>();
        Material material = renderer.material;
        Color newColor = countryData.IsLifted ? countryData.OriginalColor : Color.yellow;
        if (material.shader.name == "Standard")
        {
            material.SetColor("_Color", newColor);
        }
        else
        {
            material.color = newColor;
        }

        Vector3 originalPosition = country.transform.position;
        Vector3 direction = countryData.IsLifted ? -hit.normal.normalized : hit.normal.normalized;
        Vector3 targetPosition = originalPosition + direction * 0.1f;

        yield return StartCoroutine(MoveObject(country.transform, originalPosition, targetPosition, 0.5f));

        countryData.IsLifted = !countryData.IsLifted;
        if (!countryData.IsLifted)
        {
            SelectedCountryName = country.name;
        }
        else
        {
            SelectedCountryName = null;
        }

        isLifting = false;
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

    private class CountryData
    {
        public Color OriginalColor { get; private set; }
        public bool IsLifted { get; set; }

        public CountryData(Color originalColor)
        {
            OriginalColor = originalColor;
            IsLifted = false;
        }
    }
}