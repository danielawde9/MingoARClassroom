using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARManager : MonoBehaviour
{
    public static ARManager Instance { get; private set; }

    public ARRaycastManager ARRaycastManager { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ARRaycastManager = GetComponent<ARRaycastManager>();
    }
}
