using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
namespace MingoData.Scripts.Utils
{

    public class MiddleIconHelper
    {
        private GameObject MiddleIconGameObject { get; set; }
        public readonly GameObject middleIconsTopHelper;
        public readonly TextMeshProUGUI middleIconsTopHelperTitleText;
        public readonly Button middleIconsTopHelperCloseButton;
        public readonly TextMeshProUGUI middleIconsTextHelper;
        public SVGImage bottomIconsImage;

        public MiddleIconHelper(GameObject gameObject)
        {
            MiddleIconGameObject = gameObject;
            middleIconsTopHelper = gameObject.transform.Find("MiddleIconsTopHelper").gameObject;
            middleIconsTopHelperTitleText = gameObject.transform.Find("MiddleIconsTopHelper/MiddleIconsTopHelperTitleText").GetComponent<TextMeshProUGUI>();
            middleIconsTopHelperCloseButton = gameObject.transform.Find("MiddleIconsTopHelper/MiddleIconsTopHelperCloseButton").GetComponent<Button>();
            middleIconsTextHelper = gameObject.transform.Find("MiddleIconsMiddleHelper/MiddleIconsTextHelper").GetComponent<TextMeshProUGUI>();
            bottomIconsImage = gameObject.transform.Find("MiddleIconsBottomHelper/IconImage").GetComponent<SVGImage>();
        }
        
        public void Destroy()
        {
            if (MiddleIconGameObject == null)
                return;
            Object.Destroy(MiddleIconGameObject);
            MiddleIconGameObject = null;
        }

        
    }

}
