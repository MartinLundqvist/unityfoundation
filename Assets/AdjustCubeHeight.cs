using TMPro;
using UnityEngine;

public class AdjustCubeHeight : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float padding = 0.2f;
    public float scaleFactor = 1f;
    private Vector3 initialScale;
    private Material originalMaterial;

    void Start()
    {
        initialScale = transform.localScale;

        // Store the original material
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.sharedMaterial;
        }

        if (textMesh == null)
        {
            textMesh = GetComponentInParent<TextMeshPro>();
            Debug.Log($"[{gameObject.name}] TextMesh found in parent: {textMesh != null}");
        }

        AdjustHeight();
    }

    public void UpdateText(string text)
    {
        if (!textMesh) return;

        textMesh.text = text;
        AdjustHeight();

        // Ensure material is preserved
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.sharedMaterial = originalMaterial;
        }
    }

    void AdjustHeight()
    {
        if (!textMesh) return;

        textMesh.ForceMeshUpdate();

        // Get base sizes and apply scaling
        float baseWidth = textMesh.preferredWidth * scaleFactor;
        float baseHeight = textMesh.preferredHeight * scaleFactor;

        // Add padding after scaling
        float targetWidth = baseWidth + padding;
        float targetHeight = baseHeight + padding;

        Debug.Log($"[{gameObject.name}] Text size: {textMesh.preferredWidth} x {textMesh.preferredHeight}");
        Debug.Log($"[{gameObject.name}] Target size: {targetWidth} x {targetHeight}");

        transform.localScale = new Vector3(targetWidth, targetHeight, initialScale.z);
    }
}
