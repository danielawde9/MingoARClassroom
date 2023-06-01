using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class ArcMotion : MonoBehaviour
{
    public SVGImage imageObject; // The UI Image object
    public float speed = 1f;  // Speed of the movement
    public float angle = 22.5f; // Angle of rotation

    public float moveX = 0.01f; // X movement per step
    public float moveY = 0.01f; // Y movement per step

    void Start()
    {
        StartCoroutine(MoveImageInArc());
    }

    IEnumerator MoveImageInArc()
    {
        while (true)
        {
            // Rotate and move to the left
            for (float i = 0; i < angle; i++)
            {
                imageObject.rectTransform.Rotate(Vector3.forward, speed);
                imageObject.rectTransform.localPosition += new Vector3(-moveX, moveY, 0);
                yield return new WaitForSeconds(0.01f);
            }

            // Wait a moment
            yield return new WaitForSeconds(1f);

            // Rotate and move to the right
            for (float i = 0; i < 2 * angle; i++)
            {
                imageObject.rectTransform.Rotate(Vector3.forward, -speed);
                imageObject.rectTransform.localPosition += new Vector3(moveX, -moveY, 0);
                yield return new WaitForSeconds(0.01f);
            }

            // Wait a moment
            yield return new WaitForSeconds(1f);

            // Rotate and move back to the left
            for (float i = 0; i < angle; i++)
            {
                imageObject.rectTransform.Rotate(Vector3.forward, speed);
                imageObject.rectTransform.localPosition += new Vector3(-moveX, moveY, 0);
                yield return new WaitForSeconds(0.01f);
            }

            // Wait a moment before starting the process again
            yield return new WaitForSeconds(1f);
        }
    }
}
