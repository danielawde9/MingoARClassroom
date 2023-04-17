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
        public float? electronegativity;
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

            // Update startPosition for the next element
            startPosition.x += xOffset;
            if ((i + 1) % maxColumns == 0)
            {
                startPosition.x = 0;
                startPosition.y -= yOffset;
            }
        }
    }

    private void SetupElectronShells(GameObject bohrModel, int atomicNumber)
    {
        int[] maxElectronsPerShell = { 2, 8, 18, 32 };
        int remainingElectrons = atomicNumber;
        int shellIndex = 0;

        while (remainingElectrons > 0)
        {
            int electronsInShell = Mathf.Min(remainingElectrons, maxElectronsPerShell[shellIndex]);
            for (int i = 0; i < electronsInShell; i++)
            {
                GameObject electron = Instantiate(bohrModel.transform.Find("Electron").gameObject, bohrModel.transform.position, Quaternion.identity);
                electron.transform.SetParent(bohrModel.transform.Find($"ElectronShell{shellIndex + 1}"));
                float angle = 360f / electronsInShell * i;
                electron.transform.localPosition = Quaternion.Euler(0, angle, 0) * Vector3.right * (shellIndex + 1);
            }

            remainingElectrons -= electronsInShell;
            shellIndex++;
        }
    }
}
