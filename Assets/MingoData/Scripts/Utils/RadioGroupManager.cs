using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MingoData.Scripts.Utils
{
    public class RadioGroupManager : MonoBehaviour
    {
        public List<Toggle> toggles = new List<Toggle>();

        private void Start()
        {
            // Initialize with the first toggle selected.
            OnToggleClick(toggles[0]);
        }
        private void OnToggleClick(Object selectedToggle)
        {
            foreach (Toggle toggle in toggles)
            {
                toggle.isOn = toggle == selectedToggle;
            }
        }
    }
}
