using System.Collections.Generic;
using MingoData.Scripts.MainUtil;
using MingoData.Scripts.Utils;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;


namespace MingoData.Scripts
{
    public class MainMenuScript : MonoBehaviour
    {
        public GameObject planetsRowItemPrefab;
        public Transform planetsRowItemListParent;
        public Button proceedButton;
        public Sprite checkedImage;
        public Sprite unCheckedImage;
        public AudioSource clickAudioSource;
        private GameObject darkImageBackgroundInitialUI;
        private MiddleIconHelper uiHandler;
        private readonly List<string> selectedPlanets = new List<string>();

        [SerializeField]
        private LocalizationManager localizationManager;
        public TextMeshProUGUI mainTitle;
        public TextMeshProUGUI subTitle;
        public TextMeshProUGUI choosePlanet;

        [Header("Middle Icon Helper")]
        public GameObject middleIconHelperGameObject;
        public TextMeshProUGUI middleIconsTopHelperTitleText;
        public Button middleIconsTopHelperCloseButton;
        public TextMeshProUGUI middleIconsTextHelper;
        public SVGImage bottomIconsImage;
        
        private void Start()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    localizationManager.SetLanguage(Constants.LangEn);
                    break;
                case SystemLanguage.Arabic:
                    localizationManager.SetLanguage(Constants.LangAR);
                    break;
                default:
                    localizationManager.SetLanguage(Constants.LangEn);
                    break;
            }

            localizationManager.SetLanguage(Constants.LangAR);

            localizationManager.LoadLocalizedText();
            
            // Check if it's the first time the app is run
            if (!PlayerPrefs.HasKey("FirstTimeRun"))
            {

                middleIconHelperGameObject.transform.parent.gameObject.SetActive(true);
                
                string middleIconsTopHelperTitleString = localizationManager.GetLocalizedValue("Instructions", middleIconsTopHelperTitleText, false, Constants.ColorWhite);
                middleIconsTopHelperTitleText.text = middleIconsTopHelperTitleString;

                string middleIconsHelperString = localizationManager.GetLocalizedValue("MainScreenWelcome", middleIconsTextHelper, false, Constants.ColorWhite);
                middleIconsTextHelper.text = middleIconsHelperString;

                middleIconsTopHelperCloseButton.onClick.AddListener(() =>
                {
                    middleIconHelperGameObject.transform.parent.gameObject.SetActive(false);
                });

                UtilsFns.ReverseOrderIfArabic(middleIconsTopHelperTitleText.transform.parent.GetComponent<HorizontalLayoutGroup>());

                // Set the flag to indicate the app has been run at least once
                PlayerPrefs.SetInt("FirstTimeRun", 1);
                PlayerPrefs.Save();
            }

            // Start the animation
            StartCoroutine(UtilsFns.AnimateIcon(bottomIconsImage.GetComponent<RectTransform>(), 1f, UtilsFns.AnimationDirection.UpDown));  // Animate over 1 second

            Application.targetFrameRate = 60;

            mainTitle.text = localizationManager.GetLocalizedValue("MainTitle", mainTitle, false, Constants.ColorWhite);
            subTitle.text = localizationManager.GetLocalizedValue("SubTitle", subTitle, false, Constants.ColorWhite);
            choosePlanet.text = localizationManager.GetLocalizedValue("ChoosePlanet", choosePlanet, false, Constants.ColorWhite);
            TextMeshProUGUI proceedButtonText = proceedButton.GetComponentInChildren<TextMeshProUGUI>();
            proceedButtonText.text = localizationManager.GetLocalizedValue("Proceed", proceedButtonText, true, Constants.ColorWhite);

            PlayerPrefs.SetString(Constants.SelectedPlanets, "");
            PlayerPrefs.Save();

            TextAsset jsonFile = Resources.Load<TextAsset>("SolarSystemWithMoon/planet_data_with_moon");
            PlanetDataList planetDataList = JsonUtility.FromJson<PlanetDataList>(jsonFile.text);
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

                    string localizedPlanetName = localizationManager.GetLocalizedValue(planetDataList.planets[currentIndex].name, planetName, true, Constants.ColorWhite);
                    planetName.text = localizedPlanetName;

                    if (planetDataList.planets[currentIndex].name == Constants.PlanetSun)
                    {
                        planetToggle.isOn = true;
                        TogglePlanet(toggleImage, planetToggle, planetDataList.planets[currentIndex].name);
                    }

                    planetToggle.onValueChanged.AddListener(delegate { TogglePlanet(toggleImage, planetToggle, planetDataList.planets[currentIndex].name); });

                    planetIndex++;
                }
            }
        }

        private void LoadSolarSystemScene()
        {
            PlayClickSound();

            string selectedPlanetsString = string.Join(",", selectedPlanets);

            PlayerPrefs.SetString(Constants.SelectedPlanets, selectedPlanetsString);
            PlayerPrefs.Save();

            UtilsFns.LoadNewScene("SolarSystem");
        }

        public void PlayClickSound()
        {
            clickAudioSource.Play();
        }

        private void TogglePlanet(Image toggleImage, Toggle toggle, string planetName)
        {
            PlayClickSound();

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
        }
    }

}
