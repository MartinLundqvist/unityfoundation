using UnityEngine;
using UnityEngine.UI;

public class NodeClickHandler : MonoBehaviour
{
    // Assign your Main Camera in the Inspector.
    public Camera mainCamera;
    // Reference to the information panel.
    public GameObject infoPanel;
    // Reference to the Text (or TextMeshProUGUI) component in the info panel.
    public TMPro.TextMeshProUGUI infoText;

    void Update()
    {
        // On left mouse button click.
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera through the mouse position.
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Assume the node object has the NodeHighlight component.
                NodeHighlightHandler highlight = hit.collider.GetComponent<NodeHighlightHandler>();
                if (highlight != null)
                {
                    highlight.StartHighlight();
                }
                // Check if the hit object has a component that contains node info.
                // For this example, assume each node object has a NodeInfo component.
                NodeInfo nodeInfo = hit.collider.GetComponent<NodeInfo>();
                if (nodeInfo != null)
                {
                    // Show the info panel and update its text.
                    ShowNodeInfo(nodeInfo);
                }
            }
        }
    }

    void ShowNodeInfo(NodeInfo nodeInfo)
    {
        System.Text.StringBuilder info = new System.Text.StringBuilder();
        info.AppendLine($"ID: {nodeInfo.nodeID}");
        info.AppendLine($"Type: {nodeInfo.nodeType}");
        info.AppendLine($"Name: {nodeInfo.name}");

        if (!string.IsNullOrEmpty(nodeInfo.description))
        {
            info.AppendLine($"Description: {nodeInfo.description}");
        }

        // Add attributes section if there are any
        if (nodeInfo.attributes != null && nodeInfo.attributes.Count > 0)
        {
            info.AppendLine("\nAttributes:");
            foreach (var attr in nodeInfo.attributes)
            {
                info.AppendLine($"  {attr.Key}: {attr.Value}");
            }
        }

        infoText.text = info.ToString();
        infoPanel.SetActive(true);
    }
}
