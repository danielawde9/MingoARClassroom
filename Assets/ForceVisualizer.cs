using UnityEngine;

public class ForceVisualizer : MonoBehaviour
{
    public GameObject arrowPrefab;

    private GameObject arrowInstance;

    public void ShowForce(Vector3 position, Vector3 force)
    {
        if (arrowInstance == null)
        {
            arrowInstance = Instantiate(arrowPrefab, position, Quaternion.identity);
        }

        arrowInstance.transform.position = position;
        arrowInstance.transform.rotation = Quaternion.LookRotation(force);
        arrowInstance.transform.localScale = new Vector3(1, 1, force.magnitude);
    }

    public void HideForce()
    {
        if (arrowInstance != null)
        {
            Destroy(arrowInstance);
        }
    }
}
