using System.Collections;
using UnityEngine;

public class NodeHighlightHandler : MonoBehaviour
{
    // The color to use for highlighting.
    public Color highlightColor = Color.yellow;
    // How fast the color pulsing animation runs.
    public float pulseSpeed = 5f;

    // Internal storage for the original color.
    private Color originalColor;
    private Renderer nodeRenderer;
    // Tracks whether this node is currently highlighted.
    private bool isHighlighted = false;

    // Static reference to the currently highlighted node.
    public static NodeHighlightHandler currentlyHighlighted = null;

    void Start()
    {
        nodeRenderer = GetComponent<Renderer>();
        if (nodeRenderer != null)
        {
            originalColor = nodeRenderer.material.color;
        }
    }

    /// <summary>
    /// Call this to start the continuous color pulsing highlight.
    /// If another node is already highlighted, its highlight will be stopped.
    /// </summary>
    public void StartHighlight()
    {
        // Stop any currently highlighted node.
        if (currentlyHighlighted != null && currentlyHighlighted != this)
        {
            currentlyHighlighted.StopHighlight();
        }
        currentlyHighlighted = this;

        if (!isHighlighted)
        {
            isHighlighted = true;
            StopAllCoroutines(); // Cancel any running routines.
            StartCoroutine(PulseColorRoutine());
        }
    }

    /// <summary>
    /// Immediately stops the highlight and resets the node's color.
    /// </summary>
    public void StopHighlight()
    {
        isHighlighted = false;
        StopAllCoroutines();
        if (nodeRenderer != null)
        {
            nodeRenderer.material.color = originalColor;
        }
        if (currentlyHighlighted == this)
        {
            currentlyHighlighted = null;
        }
    }

    /// <summary>
    /// Continuously animates the node's color by interpolating between the original and highlight colors.
    /// </summary>
    private IEnumerator PulseColorRoutine()
    {
        while (isHighlighted)
        {
            // Use pulseSpeed to control the frequency of the sine wave
            // Multiply by 2π to complete one full sine wave cycle
            float t = (Mathf.Sin(Time.time * pulseSpeed * 2f * Mathf.PI) + 1f) / 2f;

            if (nodeRenderer != null)
            {
                nodeRenderer.material.color = Color.Lerp(originalColor, highlightColor, t);
            }
            yield return null;
        }
    }

}
