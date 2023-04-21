using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PeriodicTableController : MonoBehaviour
{
    [System.Serializable]
    public class ElementData
    {
        public int atomicNumber;
        public string symbol;
        public string name;
        public int group;
        public int period;
        public float atomicMass;
        public float? electroNegativity;
        public int ionizationEnergy;
        public float density;
        public float meltingPoint;
        public float boilingPoint;
        public string color;
    }

    [System.Serializable]
    public class ElementList
    {
        public List<ElementData> elements;
    }

    public GameObject bohrModelPrefab;

    public float electronShellDistanceScale = 1.5f;
    public float nucleusDistanceScale = 0.1f;
    public float atomSizeScale = 1.0f;

    private ElementList elementList;

    private void Start()
    {
        LoadElementData();
        SpawnElements();
    }

    private void LoadElementData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("elements_data");
        elementList = JsonUtility.FromJson<ElementList>(jsonFile.text);
    }

    private void SpawnElements()
    {
        Vector3 startPosition = Vector3.zero;
        float xOffset = 1.0f;
        float yOffset = 1.0f;
        int maxColumns = 9;

        for (int i = 0; i < elementList.elements.Count; i++)
        {
            ElementData elementData = elementList.elements[i];
            GameObject bohrModelInstance = Instantiate(bohrModelPrefab, startPosition, Quaternion.identity);
            bohrModelInstance.transform.SetParent(transform);

            // Set element info text
            TextMeshPro infoText = bohrModelInstance.GetComponentInChildren<TextMeshPro>();
            if (infoText != null)
            {
                infoText.text = $"{elementData.name} ({elementData.symbol})\nAtomic Number: {elementData.atomicNumber}\nAtomic Mass: {elementData.atomicMass}";
            }

            // Set up electron shells
            SetupElectronShells(bohrModelInstance, elementData.atomicNumber);

            // Set up nucleus (protons and neutrons)
            SetupNucleus(bohrModelInstance, elementData.atomicNumber, i);

            // Update startPosition for the next element
            startPosition.x += xOffset;
            if ((i + 1) % maxColumns == 0)
            {
                startPosition.x = 0;
                startPosition.y -= yOffset;
            }
        }
    }
    private void SetupNucleus(GameObject bohrModel, int atomicNumber, int elementIndex)
    {
        GameObject protonPrefab = bohrModel.transform.Find("Nucleus/Proton").gameObject;
        GameObject neutronPrefab = bohrModel.transform.Find("Nucleus/Neutron").gameObject;

        Transform nucleusTransform = bohrModel.transform.Find("Nucleus");
        // Set the nucleus radius based on the number of protons and neutrons
        float nucleusRadius = Mathf.Pow(atomicNumber, 1f / 3f) * nucleusDistanceScale;

        // Instantiate protons
        for (int i = 0; i < atomicNumber; i++)
        {
            GameObject proton = Instantiate(protonPrefab, nucleusTransform.position, Quaternion.identity, nucleusTransform);
            proton.SetActive(true);
            // You can adjust the position of protons within the nucleus here
            Vector3 randomPosition = Random.insideUnitSphere * nucleusRadius;
            proton.transform.localPosition = randomPosition;

        }

        // Instantiate neutrons
        int neutronCount = Mathf.RoundToInt(elementList.elements[elementIndex].atomicMass) - atomicNumber;

        for (int i = 0; i < neutronCount; i++)
        {
            GameObject neutron = Instantiate(neutronPrefab, nucleusTransform.position, Quaternion.identity, nucleusTransform);
            neutron.SetActive(true);
            // You can adjust the position of neutrons within the nucleus here
            Vector3 randomPosition = Random.insideUnitSphere * nucleusRadius;
            neutron.transform.localPosition = randomPosition;

        }
    }


    private void SetupElectronShells(GameObject bohrModel, int atomicNumber)
    {
        int[] maxElectronsPerShell = { 2, 8, 18, 32 };
        int remainingElectrons = atomicNumber;
        int shellIndex = 0;

        GameObject electronTemplate = bohrModel.transform.Find("Electron").gameObject;

        while (remainingElectrons > 0)
        {
            int electronsInShell = Mathf.Min(remainingElectrons, maxElectronsPerShell[shellIndex]);

            // Create a new empty GameObject to represent the electron shell
            GameObject electronShell = new GameObject($"ElectronShell{shellIndex + 1}");
            electronShell.transform.SetParent(bohrModel.transform);
            electronShell.transform.localPosition = Vector3.zero;

            for (int i = 0; i < electronsInShell; i++)
            {
                GameObject electron = Instantiate(electronTemplate, bohrModel.transform.position, Quaternion.identity);
                electron.transform.SetParent(electronShell.transform);
                float angle = 360f / electronsInShell * i;
                electron.SetActive(true); // Enable the instantiated electron

                electron.transform.localPosition = Quaternion.Euler(0, angle, 0) * Vector3.right * (shellIndex + 1)* electronShellDistanceScale;
            }

            remainingElectrons -= electronsInShell;
            shellIndex++;
        }

        // Disable the original electron template object
        electronTemplate.SetActive(false);
    }

}
