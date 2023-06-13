using System.Collections.Generic;
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

        private void Start()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystemWithMoon/planet_data_with_moon");
            SolarSystemSimulationWithMoons.PlanetDataList planetDataList = JsonUtility.FromJson<SolarSystemSimulationWithMoons.PlanetDataList>(jsonFile.text);

            foreach (Transform child in planetsRowItemListParent)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < planetDataList.planets.Count; i+=2)
            {
                GameObject planetRow = Instantiate(planetsRowItemPrefab, planetsRowItemListParent);
                planetRow.name = "PlanetRow";

                int planetIndex = 0;
                foreach (Transform child in planetRow.transform)
                {
                    if(i + planetIndex >= planetDataList.planets.Count)
                        break;

                    int currentIndex = i + planetIndex;  

                    
                    Image planetImage = child.Find("PlanetImage").GetComponent<Image>();
                    TextMeshProUGUI planetName = child.Find("PlanetName").GetComponent<TextMeshProUGUI>();
                    Toggle planetToggle = child.GetComponent<Toggle>();

                    planetImage.sprite = Resources.Load<Sprite>("SolarSystemWithMoon/PlanetImages/" + planetDataList.planets[currentIndex].name);
                    planetName.text = planetDataList.planets[currentIndex].name;
                    planetToggle.onValueChanged.AddListener(delegate { TogglePlanet(planetToggle, planetDataList.planets[currentIndex].name); });

                    planetIndex++;
                }
            }
        }

        
        private void TogglePlanet(Toggle toggle, string planetName)
        {
            if (toggle.isOn)
            {
                if(!selectedPlanets.Contains(planetName)) 
                    selectedPlanets.Add(planetName);
            }
            else
            {
                selectedPlanets.Remove(planetName);
            }

            Debug.Log("Currently selected planets: " + string.Join(", ", selectedPlanets));
        }
    }
}
