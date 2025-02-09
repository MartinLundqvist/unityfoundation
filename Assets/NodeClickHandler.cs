using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using Newtonsoft.Json;  // Make sure Newtonsoft.Json is installed

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
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Highlight the node if it has a NodeHighlightHandler.
                NodeHighlightHandler highlight = hit.collider.GetComponent<NodeHighlightHandler>();
                if (highlight != null)
                {
                    highlight.StartHighlight();
                }

                // Check if the hit object has a component that contains node info.
                NodeInfo nodeInfo = hit.collider.GetComponent<NodeInfo>();
                if (nodeInfo != null)
                {
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
        if (graphRenderer != null)
        {
            graphRenderer.gameObject.SetActive(true);

            // Load data points using Newtonsoft.Json.
            List<Vector2> dataPoints = LoadSensorData("89afbe56-2109-5e9d-b8d5-640975813b0b");
            graphRenderer.SetData(dataPoints);
            graphRenderer.UpdateValueText(nodeInfo.nodeName);
        }
        else
        {
            Debug.LogWarning("TimeSeriesGraphRenderer reference is missing. Please assign it in the Inspector.");
        }
    }

    private List<Vector2> LoadSensorData(string sensorId)
    {
        // Load the JSON file from the Resources folder.
        TextAsset jsonFile = Resources.Load<TextAsset>("sensor_data");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load sensor_data.json");
            return new List<Vector2>();
        }

        try
        {
            // Trim whitespace and check if the JSON starts with '['.
            string jsonText = jsonFile.text.Trim();
            if (jsonText.StartsWith("["))
            {
                // Wrap the array in an object with an "entries" field.
                jsonText = "{\"entries\":" + jsonText + "}";
            }

            // (Optional) Log the first entry's structure for debugging.
            int firstBracket = jsonText.IndexOf("{", 1);
            int matchingBracket = FindMatchingBracket(jsonText, firstBracket);
            if (firstBracket >= 0 && matchingBracket > firstBracket)
            {
                string firstEntry = jsonText.Substring(firstBracket, matchingBracket - firstBracket + 1);
                Debug.Log("First entry structure: " + firstEntry);
            }

            // Deserialize the JSON using Newtonsoft.Json.
            SensorDataWrapper wrapper = JsonConvert.DeserializeObject<SensorDataWrapper>(jsonText);

            if (wrapper.entries != null && wrapper.entries.Count > 0)
            {
                var firstSensor = wrapper.entries[0];
                Debug.Log($"First sensor ID: {firstSensor.sensorId}");
                Debug.Log($"Raw data dictionary entries: {firstSensor.data?.Count ?? 0}");
                if (firstSensor.data != null && firstSensor.data.Count > 0)
                {
                    var firstKey = firstSensor.data.Keys.FirstOrDefault();
                    if (firstKey != null)
                    {
                        Debug.Log($"First data point - Time: {firstKey}, Value: {firstSensor.data[firstKey]}");
                    }
                }
            }
            else
            {
                Debug.Log("No entries found in wrapper");
            }

            // Find the sensor entry with the matching ID.
            SensorEntry sensorEntry = wrapper.entries.Find(s => s.sensorId == sensorId);
            if (sensorEntry == null)
            {
                Debug.LogWarning("Sensor entry with ID " + sensorId + " not found.");
                return new List<Vector2>();
            }

            if (sensorEntry.data == null || sensorEntry.data.Count == 0)
            {
                Debug.LogWarning("No data found for sensor entry with ID " + sensorId);
                return new List<Vector2>();
            }

            List<Vector2> dataPoints = new List<Vector2>();
            long earliest = sensorEntry.GetEarliestTimestamp();

            // Convert each key/value pair into a Vector2.
            foreach (var kvp in sensorEntry.data)
            {
                if (long.TryParse(kvp.Key, out long timestamp))
                {
                    float value = kvp.Value;
                    float timeValue = (timestamp - earliest) / 1000f;
                    dataPoints.Add(new Vector2(timeValue, value));
                }
                else
                {
                    Debug.LogWarning("Failed to parse timestamp key: " + kvp.Key);
                }
            }
            dataPoints.Sort((a, b) => a.x.CompareTo(b.x));
            return dataPoints;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing sensor data: {e.Message}");
        }

        return new List<Vector2>();
    }

    private int FindMatchingBracket(string text, int openBracketIndex)
    {
        int count = 1;
        for (int i = openBracketIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '{') count++;
            else if (text[i] == '}') count--;

            if (count == 0)
                return i;
        }
        return -1;
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

    // Classes for JSON deserialization using Newtonsoft.Json.
    private class SensorDataWrapper
    {
        [JsonProperty("entries")]
        public List<SensorEntry> entries { get; set; }
    }

    private class SensorEntry
    {
        [JsonProperty("instructions")]
        public Instructions instructions { get; set; }

        [JsonProperty("query")]
        public string query { get; set; }

        [JsonProperty("sensorId")]
        public string sensorId { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, float> data { get; set; }

        public long GetEarliestTimestamp()
        {
            if (data == null || data.Count == 0)
                return 0;
            return data.Keys.Select(k => long.Parse(k)).Min();
        }
    }

    private class Instructions
    {
        [JsonProperty("period")]
        public string period { get; set; }
    }
}