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
        // Build your info string. Customize as needed.
        string info = $"ID: {nodeInfo.nodeID}\n" +
                      $"Type: {nodeInfo.nodeType}\n" +
                      $"Additional Info: {nodeInfo.description}";
        infoText.text = info;
        infoPanel.SetActive(true);
    }
}
