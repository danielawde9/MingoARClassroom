using System.Collections.Generic;
using System.Linq;
using MingoData.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MingoData.Scripts
{

    public class MainMenuScript : MonoBehaviour
    {
        public GameObject planetsRowItemPrefab;
        public Transform planetsRowItemListParent;
        private readonly List<string> selectedPlanets = new List<string>();
        public Button proceedButton;
        public Sprite checkedImage;
        public Sprite unCheckedImage;
        public ToggleGroup chooseLangToggleGroup;
        private void Start()
        {
            PlayerPrefs.SetString(Constants.SelectedPlanets, "");
            PlayerPrefs.Save();

            TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystemWithMoon/planet_data_with_moon");
            SolarSystemSimulationWithMoons.PlanetDataList planetDataList = JsonUtility.FromJson<SolarSystemSimulationWithMoons.PlanetDataList>(jsonFile.text);
            proceedButton.onClick.AddListener(LoadSolarSystemScene);
            proceedButton.interactable = false;

            foreach (Transform child in planetsRowItemListParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < planetDataList.planets.Count; i += 2)
            {
                GameObject planetRow = Instantiate(planetsRowItemPrefab, planetsRowItemListParent);
                planetRow.name = "PlanetRow";

                int planetIndex = 0;
                foreach (Transform child in planetRow.transform)
                {
                    if (i + planetIndex >= planetDataList.planets.Count)
                        break;

                    int currentIndex = i + planetIndex;

                    Image planetImage = child.Find("PlanetImage").GetComponent<Image>();
                    TextMeshProUGUI planetName = child.Find("PlanetName").GetComponent<TextMeshProUGUI>();
                    Toggle planetToggle = child.GetComponent<Toggle>();
                    Image toggleImage = child.GetComponent<Image>();
                    
                    planetImage.sprite = Resources.Load<Sprite>("SolarSystemWithMoon/PlanetImages/" + planetDataList.planets[currentIndex].name);
                    planetName.text = planetDataList.planets[currentIndex].name;
                    
                    if (planetDataList.planets[currentIndex].name == "Sun") 
                    {
                        planetToggle.isOn = true;
                        TogglePlanet(toggleImage, planetToggle, planetDataList.planets[currentIndex].name); 
                    }
                    
                    planetToggle.onValueChanged.AddListener(delegate { TogglePlanet(toggleImage,planetToggle, planetDataList.planets[currentIndex].name); });
                    
                    planetIndex++;
                }
            }
        }

        private void LoadSolarSystemScene()
        {
            Toggle activeToggle = chooseLangToggleGroup.ActiveToggles().FirstOrDefault();
            // Save the name of the selected planet from the toggle group to PlayerPrefs
            if (activeToggle != null)
            {
                PlayerPrefs.SetString(Constants.SelectedLanguage, activeToggle.gameObject.name);
            }

            string selectedPlanetsString = string.Join(",", selectedPlanets);

            PlayerPrefs.SetString(Constants.SelectedPlanets, selectedPlanetsString);
            PlayerPrefs.Save();

            UtilsFns.LoadNewScene("SolarSystem");
        }


        private void TogglePlanet(Image toggleImage, Toggle toggle, string planetName)
        {
            
            if (toggle.isOn)
            {
                if (!selectedPlanets.Contains(planetName))
                    selectedPlanets.Add(planetName);
                toggleImage.sprite = checkedImage;
            }
            else
            {
                selectedPlanets.Remove(planetName);
                toggleImage.sprite = unCheckedImage;

            }
            proceedButton.interactable = selectedPlanets.Count > 0;

            Debug.Log("Currently selected planets: " + string.Join(", ", selectedPlanets));
        }


    }

}
