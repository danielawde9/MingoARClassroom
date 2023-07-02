using System.Collections.Generic;
using MingoData.Scripts.MainUtil;
using MingoData.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace MingoData.Scripts
{
    public class MainMenuScript : MonoBehaviour
    {
        public GameObject planetsRowItemPrefab;
        public GameObject middleIconHelperPrefab;
        public Transform planetsRowItemListParent;
        private readonly List<string> selectedPlanets = new List<string>();
        public Button proceedButton;
        public Sprite checkedImage;
        public Sprite unCheckedImage;
        public AudioSource clickAudioSource;
        private GameObject darkImageBackgroundInitialUI;
        public Sprite swipeUpDownIcon;

        [SerializeField]
        private LocalizationManager localizationManager;
        public TextMeshProUGUI mainTitle;
        public TextMeshProUGUI subTitle;
        public TextMeshProUGUI chooseLang;
        public TextMeshProUGUI choosePlanet;
        
        private UnityAction CreateUiHelperFunction(MiddleIconHelper uiHelper)
        {
            return () =>
            {
                PlayClickSound();
                darkImageBackgroundInitialUI.SetActive(false);
                uiHelper.Destroy();
            };
        }
        
        private MiddleIconHelper SpawnMiddleIconHelper(string middleIconsTopHelperTitleKey, string middleIconsTextHelperKey, Sprite bottomIconsImage, bool isMiddleIconsTopHelper, UtilsFns.AnimationDirection direction)
        {
            // Instantiate the prefab
            GameObject instance = Instantiate(middleIconHelperPrefab, transform);

            instance.name = "MiddleIconHelper";

            // Get the components
            MiddleIconHelper helper = new MiddleIconHelper(instance);
            string middleIconsTopHelperTitleText = localizationManager.GetLocalizedValue(middleIconsTopHelperTitleKey, helper.middleIconsTopHelperTitleText, false, Constants.ColorWhite);

            helper.middleIconsTopHelperTitleText.text = middleIconsTopHelperTitleText;
            string middleIconsTextHelper = localizationManager.GetLocalizedValue(middleIconsTextHelperKey, helper.middleIconsTextHelper, false, Constants.ColorWhite);

            helper.middleIconsTextHelper.text = middleIconsTextHelper;

            helper.bottomIconsImage.sprite = bottomIconsImage;

            // Set the activity of the top helper
            helper.middleIconsTopHelper.SetActive(isMiddleIconsTopHelper);

            if (isMiddleIconsTopHelper)
            {
                helper.middleIconsTopHelperCloseButton.onClick.RemoveAllListeners();

                UnityAction uiHelperFunction = CreateUiHelperFunction(helper);

                helper.middleIconsTopHelperCloseButton.onClick.AddListener(uiHelperFunction);

            }

            HorizontalLayoutGroup layoutGroup = helper.middleIconsTopHelper.GetComponent<HorizontalLayoutGroup>();
            
            UtilsFns.ReverseOrderIfArabic(layoutGroup);

            instance.SetActive(true);
            
            // Get the RectTransform of the icon
            RectTransform iconRectTransform = helper.bottomIconsImage.GetComponent<RectTransform>();

            // Start the animation
            StartCoroutine(UtilsFns.AnimateIcon(iconRectTransform, 1f, direction));  // Animate over 1 second

            return helper;
        }
        
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
            
            darkImageBackgroundInitialUI = UtilsFns.CreateDarkBackground("InitialUI");
            darkImageBackgroundInitialUI.SetActive(true);
            
            SpawnMiddleIconHelper(
                "Instructions",
                "Click_on_any_planet_or_click_on_the_menu_below_to_display_more_settings",
                swipeUpDownIcon,
                true,
                UtilsFns.AnimationDirection.UpDown);
            
            localizationManager.LoadLocalizedText();

            Application.targetFrameRate = 60;

            mainTitle.text = localizationManager.GetLocalizedValue("MainTitle", mainTitle, false, Constants.ColorWhite);
            subTitle.text = localizationManager.GetLocalizedValue("SubTitle", subTitle, false, Constants.ColorWhite);
            chooseLang.text = localizationManager.GetLocalizedValue("ChooseLang", chooseLang, false, Constants.ColorWhite);
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
