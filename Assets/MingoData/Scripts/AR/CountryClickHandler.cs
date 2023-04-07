using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountryClickHandler : MonoBehaviour
{
    public string SelectedCountryName { get; set; }
    private bool isLifting = false;
    private GameObject selectedCountry = null;

    private readonly Dictionary<string, CountryData> countriesData = new();

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isLifting)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layerMask = 1 << LayerMask.NameToLayer("Countries");

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                GameObject country = hit.collider.gameObject;

                if (!countriesData.ContainsKey(country.name))
                {
                    Material material = country.GetComponent<Renderer>().material;
                    Color color = material.shader.name == "Standard" ? material.GetColor("_Color") : material.color;
                    countriesData[country.name] = new CountryData(color);
                }

                if (selectedCountry != null && selectedCountry != country)
                {
                    DeselectCountry(selectedCountry);
                }

                ChangeCountryColorAndPosition(country, hit, countriesData[country.name]);
            }
        }
    }

    void DeselectCountry(GameObject country)
    {
        if (countriesData.ContainsKey(country.name))
        {
            CountryData countryData = countriesData[country.name];
            if (countryData.IsLifted)
            {
                ChangeCountryColorAndPosition(country, default(RaycastHit), countryData);
            }
        }
    }

    void ChangeCountryColorAndPosition(GameObject country, RaycastHit hit, CountryData countryData)
    {
        isLifting = true;

        Renderer renderer = country.GetComponent<Renderer>();
        renderer.material.SetColor("_BaseColor", countryData.IsLifted ? countryData.OriginalColor : Color.yellow);

        Vector3 originalPosition = country.transform.position;
        Vector3 direction = countryData.IsLifted ? -hit.normal.normalized : hit.normal.normalized;
        Vector3 targetPosition = originalPosition + direction * 0.1f;

        StartCoroutine(MoveObject(country.transform, originalPosition, targetPosition, 0.5f));

        countryData.IsLifted = !countryData.IsLifted;
        if (!countryData.IsLifted)
        {
            SelectedCountryName = country.name;
            selectedCountry = country;
        }
        else
        {
            SelectedCountryName = null;
            selectedCountry = null;
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
