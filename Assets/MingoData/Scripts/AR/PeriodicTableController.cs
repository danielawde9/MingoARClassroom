using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private float electronShellDistanceScale = 0.2f;
    private float nucleusDistanceScale = 0.01f;
    private float nucleusSizeScale = 0.06f;
    private float electronSizeScale = 0.02f;
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
        atomSizeSlider.value = nucleusSizeScale;
        electronSizeSlider.value = electronSizeScale;

        electronShellDistanceSlider.onValueChanged.AddListener(UpdateElectronShellDistanceScale);
        nucleusDistanceSlider.onValueChanged.AddListener(UpdateNucleusDistanceScale);
        atomSizeSlider.onValueChanged.AddListener(UpdateAtomSizeScale);
        electronSizeSlider.onValueChanged.AddListener(UpdateElectronSizeScale);

    }

    private Dictionary<string, Color> subshellColors = new Dictionary<string, Color>
    {
        { "s", Color.red },
        { "p", Color.green },
        { "d", Color.blue },
        { "f", Color.yellow }
    };


    private List<(int, string, int)> CalculateElectronConfiguration(int atomicNumber)
    {
        List<(int, string, int)> electronConfiguration = new List<(int, string, int)>();
        int[] maxElectronsPerSubshell = { 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54, 58, 62, 66, 70, 74, 78, 82, 86, 90, 94, 98 };
        string[] subshellLabels = { "s", "p", "d", "f", "g", "h" };
        int remainingElectrons = atomicNumber;
        int shellIndex = 0;

        while (remainingElectrons > 0)
        {
            for (int subshellIndex = 0; subshellIndex < subshellLabels.Length; subshellIndex++)
            {
                if (remainingElectrons == 0)
                {
                    break;
                }

                int maxElectronsInSubshell = maxElectronsPerSubshell[subshellIndex];

                if (shellIndex >= maxElectronsPerSubshell.Length)
                {
                    Debug.LogWarning($"Unsupported atomic number '{atomicNumber}'. Electron configuration calculation may be incorrect.");
                    break;
                }

                int electronsInSubshell = Mathf.Min(remainingElectrons, maxElectronsInSubshell);
                electronConfiguration.Add((shellIndex + 1, subshellLabels[subshellIndex], electronsInSubshell));
                remainingElectrons -= electronsInSubshell;
            }

            shellIndex++;
        }

        return electronConfiguration;
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
        nucleusSizeScale = value;
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
    private void SetupElectronShells(GameObject bohrModel, int atomicNumber)
    {
        //int[] maxElectronsPerShell = { 2, 8, 18, 32, 50, 72, 98 };
        int[] maxElectronsPerSubshell = { 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54, 58, 62, 66, 70, 74, 78, 82, 86, 90, 94, 98 };

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

        List<(int, string, int)> electronConfiguration = CalculateElectronConfiguration(atomicNumber);

        int electronConfigIndex = 0;

        while (remainingElectrons > 0)
        {
            int electronsInShell = Mathf.Min(remainingElectrons, maxElectronsPerSubshell[shellIndex]);

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

                // Change electron color based on subshell
                if (electronConfigIndex < electronConfiguration.Count)
                {
                    var (_, subshellType, _) = electronConfiguration[electronConfigIndex];
                    if (subshellColors.TryGetValue(subshellType, out Color color))
                    {
                        electron.GetComponent<Renderer>().material.color = color;
                    }
                }

                TextMeshPro electronText = electron.GetComponentInChildren<TextMeshPro>();
                if (electronText != null)
                {
                    int shellNumber = electronConfiguration[electronConfigIndex].Item1;
                    string subshellType = electronConfiguration[electronConfigIndex].Item2;
                    int subshellCount = electronConfiguration[electronConfigIndex].Item3;
                    electronText.text = $"{shellNumber}{subshellType}{subshellCount}";
                }

                // Update electronConfiguration index
                electronConfiguration[electronConfigIndex] = (electronConfiguration[electronConfigIndex].Item1, electronConfiguration[electronConfigIndex].Item2, electronConfiguration[electronConfigIndex].Item3 - 1);
                if (electronConfiguration[electronConfigIndex].Item3 == 0)
                {
                    electronConfigIndex++;
                }
            }

            remainingElectrons -= electronsInShell;
            shellIndex++;
        }

        // Disable the original electron template object
        electronTemplate.SetActive(false);
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

        int numProtons = atomicNumber;
        int numNeutrons = Mathf.RoundToInt(elementList.elements[elementIndex].atomicMass) - numProtons;

        // Set the nucleus radius based on the number of protons and neutrons
        float nucleusRadius = Mathf.Pow(atomicNumber, 1f / 3f) * nucleusDistanceScale;

        // Spawn protons
        for (int i = 0; i < numProtons; i++)
        {
            GameObject newProton = Instantiate(protonPrefab, nucleusTransform);
            newProton.name = $"Proton_{i}";
            newProton.SetActive(true);

            newProton.transform.localPosition = RandomInsideSphere(nucleusRadius);
            newProton.transform.localScale = Vector3.one * nucleusSizeScale;
        }

        // Spawn neutrons
        for (int i = 0; i < numNeutrons; i++)
        {
            GameObject newNeutron = Instantiate(neutronPrefab, nucleusTransform);
            newNeutron.SetActive(true);
            newNeutron.name = $"Neutron_{i}";
            newNeutron.transform.localPosition = RandomInsideSphere(nucleusRadius);
            newNeutron.transform.localScale = Vector3.one * nucleusSizeScale;
        }

        // Deactivate original proton and neutron prefabs
        protonPrefab.SetActive(false);
        neutronPrefab.SetActive(false);
    }

    private Vector3 RandomInsideSphere(float radius)
    {
        return Random.insideUnitSphere * radius;
    }
}

