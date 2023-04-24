using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public float electronShellDistanceScale = 0.2f;
    public float nucleusDistanceScale = 0.01f;
    public float atomSizeScale = 0.06f;
    public float electronSizeScale = 0.02f;
    public Slider electronShellDistanceSlider;
    public Slider nucleusDistanceSlider;
    public Slider atomSizeSlider;
    public Slider electronSizeSlider;
    private ElementList elementList;


    public Color upSpinElectronColor = Color.yellow;
    public Color downSpinElectronColor = Color.green;


    private void Start()
    {
        LoadElementData();
        SpawnElements();

        electronShellDistanceSlider.value = electronShellDistanceScale;
        nucleusDistanceSlider.value = nucleusDistanceScale;
        atomSizeSlider.value= atomSizeScale;
        electronSizeSlider.value= electronSizeScale;

        electronShellDistanceSlider.onValueChanged.AddListener(UpdateElectronShellDistanceScale);
        nucleusDistanceSlider.onValueChanged.AddListener(UpdateNucleusDistanceScale);
        atomSizeSlider.onValueChanged.AddListener(UpdateAtomSizeScale);
        electronSizeSlider.onValueChanged.AddListener(UpdateElectronSizeScale);

    }

    private void UpdateElectronShellDistanceScale(float value)
    {
        electronShellDistanceScale = value;
        // Update all instances of the elements to reflect the new scale
        UpdateElementScales();
    }

    private void UpdateNucleusDistanceScale(float value)
    {
        nucleusDistanceScale = value;
        // Update all instances of the elements to reflect the new scale
        UpdateElementScales();
    }

    private void UpdateAtomSizeScale(float value)
    {
        atomSizeScale = value;
        // Update all instances of the elements to reflect the new scale
        UpdateElementScales();
    }
    private void UpdateElectronSizeScale(float value)
    {
        electronSizeScale = value;
        // Update all instances of the elements to reflect the new scale
        UpdateElementScales();
    }

    private void AddElectronRotation(GameObject electron, float rotationSpeed)
    {
        electron.AddComponent<RotateAroundAxis>();
        RotateAroundAxis rotateScript = electron.GetComponent<RotateAroundAxis>();
        rotateScript.rotationAxis = Vector3.up;
        rotateScript.rotationSpeed = rotationSpeed;
        rotateScript.centerPoint = electron.transform.parent.position;
    }



    private void UpdateElementScales()
    {
        foreach (Transform elementTransform in transform)
        {
            GameObject bohrModel = elementTransform.gameObject;
            if (!bohrModel.name.StartsWith("Element_"))
            {
                continue;
            }
            int atomicNumber = int.Parse(bohrModel.name.Split('_')[1]);

            // Set up electron shells
            SetupElectronShells(bohrModel, atomicNumber);

            // Set up nucleus (protons and neutrons)
            Transform nucleusTransform = bohrModel.transform.Find("Nucleus");
            if (nucleusTransform == null)
            {
                nucleusTransform = new GameObject("Nucleus").transform;
                nucleusTransform.SetParent(bohrModel.transform);
                nucleusTransform.localPosition = Vector3.zero;
            }
            int elementIndex = elementList.elements.FindIndex(element => element.atomicNumber == atomicNumber);
            SetupNucleus(bohrModel, nucleusTransform, atomicNumber, elementIndex);
        }
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

            bohrModelInstance.name = $"Element_{elementData.atomicNumber}";

            // Set element info text
            TextMeshPro infoText = bohrModelInstance.GetComponentInChildren<TextMeshPro>();
            if (infoText != null)
            {
                infoText.text = $"{elementData.name} ({elementData.symbol})\nAtomic Number: {elementData.atomicNumber}\nAtomic Mass: {elementData.atomicMass}";
            }

            // Set up electron shells
            SetupElectronShells(bohrModelInstance, elementData.atomicNumber);

            // Set up nucleus (protons and neutrons)
            Transform nucleusTransform = bohrModelInstance.transform.Find("Nucleus");
            SetupNucleus(bohrModelInstance, nucleusTransform, elementData.atomicNumber, i);

            // Update startPosition for the next element
            startPosition.x += xOffset;
            if ((i + 1) % maxColumns == 0)
            {
                startPosition.x = 0;
                startPosition.y -= yOffset;
            }
        }
    }
    private void SetupNucleus(GameObject bohrModel, Transform nucleusTransform, int atomicNumber, int elementIndex)
    {
        GameObject protonPrefab = bohrModel.transform.Find("Nucleus/Proton").gameObject;
        GameObject neutronPrefab = bohrModel.transform.Find("Nucleus/Neutron").gameObject;

        // Remove old protons and neutrons
        foreach (Transform child in nucleusTransform)
        {
            if (child.gameObject != protonPrefab && child.gameObject != neutronPrefab)
            {
                Destroy(child.gameObject);
            }
        }

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
            proton.transform.localScale = Vector3.one * atomSizeScale;


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
            neutron.transform.localScale = Vector3.one * atomSizeScale;

        }
    }




    private void SetupElectronShells(GameObject bohrModel, int atomicNumber)
    {
        int[] maxElectronsPerShell = { 2, 8, 18, 32 };
        int remainingElectrons = atomicNumber;
        int shellIndex = 0;

        GameObject electronTemplate = bohrModel.transform.Find("Electron").gameObject;

        // Remove old electron shells
        foreach (Transform child in bohrModel.transform)
        {
            if (child.name.StartsWith("ElectronShell"))
            {
                Destroy(child.gameObject);
            }
        }

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

                electron.transform.localPosition = Quaternion.Euler(0, angle, 0) * Vector3.right * (shellIndex + 1) * electronShellDistanceScale;

                // Update electron size
                electron.transform.localScale = Vector3.one * electronSizeScale;

                float rotationSpeed = 30f; // Adjust this value to change the rotation speed of the electrons

                AddElectronRotation(electron, rotationSpeed);

                // Change electron color based on spin
                if (i % 2 == 0)
                {
                    electron.GetComponent<Renderer>().material.color = upSpinElectronColor;
                }
                else
                {
                    electron.GetComponent<Renderer>().material.color = downSpinElectronColor;
                }
            }

            remainingElectrons -= electronsInShell;
            shellIndex++;
        }

        // Disable the original electron template object
        electronTemplate.SetActive(false);
    }


}
