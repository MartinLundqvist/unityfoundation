using TMPro;
using UnityEngine;

public class LabelManager : MonoBehaviour
{
    [Header("References")]
    // Reference to the TextMeshPro component on the label.
    public TextMeshPro textLabel;
    // Reference to the LabelAnchor (optional, for billboard behavior).
    public Transform labelAnchor;
    // Reference to the Cube transform.
    public Transform cubeTransform;

    [Header("Settings")]
    // Fixed width for the text container (in world units).
    public float fixedWidth = 10f;
    // Optional padding (in world units) to add to the rendered text height.
    public float verticalPadding = 0.5f;
    // Set to true if you want the text to always face the camera.
    // public bool faceCamera = true;

    /// <summary>
    /// Updates the node’s label text and adjusts the cube’s height accordingly.
    /// </summary>
    public void SetLabel(string label)
    {
        if (textLabel == null || cubeTransform == null)
        {
            Debug.LogWarning("Missing TextLabel or CubeTransform reference.");
            return;
        }

        // Update the text.
        textLabel.text = label;

        // Ensure fixed width and wrapping on the text container.
        // textLabel.textContainer.width = fixedWidth;
        // textLabel.enableWordWrapping = true;

        // Force the text mesh to update so we can get correct measurements.
        textLabel.ForceMeshUpdate();

        // Compute the preferred values.
        // Here we specify a width of fixedWidth and an infinite height.
        Vector2 preferredValues = textLabel.GetPreferredValues(label, fixedWidth, float.PositiveInfinity);
        float renderedHeight = preferredValues.y;

        // Optionally add some vertical padding.
        float finalHeight = renderedHeight + verticalPadding;

        // Update the cube's scale.
        // Assume the cube's x-dimension should match the fixed width,
        // and adjust the y-dimension (height) to match the text.
        cubeTransform.localScale = new Vector3(fixedWidth, finalHeight, cubeTransform.localScale.z);
    }

    void LateUpdate()
    {
        //     if (faceCamera && labelAnchor != null)
        //     {
        //         // Make the label anchor (and thus the text) face the main camera.
        //         Camera cam = Camera.main;
        //         if (cam != null)
        //         {
        //             // Rotate the anchor to look at the camera.
        //             labelAnchor.LookAt(cam.transform);
        //             // Adjust by 180° if the text appears mirrored.
        //             labelAnchor.Rotate(0, 180f, 0);
        //         }
        //         else
        //         {
        //             Debug.LogWarning("No main camera found.");
        //         }
        //     }
        // }
    }
}