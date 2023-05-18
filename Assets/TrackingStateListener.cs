using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// this script used to track initially the user to scan the room and show them the image to tap on the scanned area
public class TrackingStateListener : MonoBehaviour
{
    public ARPlaneManager m_PlaneManager; // The ARPlaneManager

    public GameObject objectToDisable; // Object to disable when count > 0
    public GameObject objectToEnable; // Object to enable when count > 0

    public TextMeshProUGUI titleTextMenu;
    void Update()
    {
        if (m_PlaneManager.trackables.count > 0)
        {
            objectToDisable.SetActive(false);
            objectToEnable.SetActive(true);
            titleTextMenu.text = "tap on the scanned area to place the solar system";
            // Optionally, disable this script after we've done the switch
            this.enabled = false;
        }
    }
}
