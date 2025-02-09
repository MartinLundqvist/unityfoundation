using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class NodeClickHandler : MonoBehaviour
{
    // Assign your Main Camera in the Inspector.
    public Camera mainCamera;
    // Reference to the information panel.
    public GameObject infoPanel;
    // Reference to the Text (or TextMeshProUGUI) component in the info panel.
    public TMPro.TextMeshProUGUI infoText;
    // Reference to the TimeSeriesGraphRenderer component.
    public TimeSeriesGraphRenderer graphRenderer;

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

                    // If the node is a sensor, show the time series graph.
                    if (nodeInfo.nodeType == "sensor")
                    {
                        ShowTimeSeriesGraph(nodeInfo);
                    }
                    else
                    {
                        HideTimeSeriesGraph();
                    }
                }
            }
        }
    }

    void HideTimeSeriesGraph()
    {
        if (graphRenderer != null)
        {
            graphRenderer.gameObject.SetActive(false);
        }
    }
    void ShowTimeSeriesGraph(NodeInfo nodeInfo)
    {
        // Use the referenced renderer instead of trying to GetComponent
        if (graphRenderer != null)
        {
            graphRenderer.gameObject.SetActive(true);
            // Example data points (time, value)
            List<Vector2> dataPoints = new List<Vector2>();
            // Generate random number of data points between 10-30
            int numPoints = Random.Range(10, 31);
            float lastValue = Random.Range(10f, 40f);

            for (int i = 0; i < numPoints; i++)
            {
                // Add some random variation to the next value
                float variation = Random.Range(-5f, 5f);
                lastValue = Mathf.Clamp(lastValue + variation, 0f, 100f);
                dataPoints.Add(new Vector2(i, lastValue));
            }

            graphRenderer.SetData(dataPoints);
            graphRenderer.UpdateValueText(nodeInfo.nodeName);
        }
        else
        {
            Debug.LogWarning("TimeSeriesGraphRenderer reference is missing. Please assign it in the Inspector.");
        }
    }

    void ShowNodeInfo(NodeInfo nodeInfo)
    {
        System.Text.StringBuilder info = new System.Text.StringBuilder();
        info.AppendLine($"ID: {nodeInfo.nodeID}");
        info.AppendLine($"Type: {nodeInfo.nodeType}");
        info.AppendLine($"Name: {nodeInfo.nodeName}");

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
