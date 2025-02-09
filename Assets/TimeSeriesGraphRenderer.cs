using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class TimeSeriesGraphRenderer : MonoBehaviour
{
    [SerializeField]
    private RectTransform graphContainer;
    [SerializeField]
    private TextMeshProUGUI chartTitle;
    public Color lineColor = Color.green;
    public float lineWidth = 2f;
    public RectTransform xAxisContainer;
    public TextMeshProUGUI xLabelPrefab;


    public int xLabelCount = 5;
    public int yLabelCount = 5;
    public TextMeshProUGUI yLabelPrefab;
    public RectTransform yAxisContainer;

    private List<Vector2> timeSeriesData = new List<Vector2>();

    private void Awake()
    {
        if (graphContainer == null)
        {
            Debug.LogError("Graph Container RectTransform not assigned!");
        }
        if (chartTitle == null)
        {
            Debug.LogError("Chart Title TextMeshProUGUI not assigned!");
        }
        if (xAxisContainer == null)
        {
            Debug.LogError("X Axis Container not assigned!");
        }
        if (xLabelPrefab == null)
        {
            Debug.LogError("X Label Prefab not assigned!");
        }
    }

    public void SetData(List<Vector2> dataPoints)
    {
        timeSeriesData = dataPoints;
        DrawGraph();
    }

    private void DrawGraph()
    {
        // Clear previous graph elements
        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }

        if (timeSeriesData.Count < 2) return;

        // Determine the data range
        float xMin = timeSeriesData.Min(point => point.x);
        float xMax = timeSeriesData.Max(point => point.x);
        float yMin = timeSeriesData.Min(point => point.y);
        float yMax = timeSeriesData.Max(point => point.y);

        // Draw X-axis, Y-axis labels
        DrawXAxis(xMin, xMax);
        DrawYAxis(yMin, yMax);

        // Get the graph dimensions
        Rect graphRect = graphContainer.rect;
        float graphWidth = graphRect.width;
        float graphHeight = graphRect.height;

        // Convert and draw points
        for (int i = 0; i < timeSeriesData.Count - 1; i++)
        {
            Vector2 pointA = NormalizeAndConvert(timeSeriesData[i], xMin, xMax, yMin, yMax, graphWidth, graphHeight);
            Vector2 pointB = NormalizeAndConvert(timeSeriesData[i + 1], xMin, xMax, yMin, yMax, graphWidth, graphHeight);

            DrawLine(pointA, pointB);
        }
    }

    private Vector2 NormalizeAndConvert(Vector2 point, float xMin, float xMax, float yMin, float yMax, float graphWidth, float graphHeight)
    {
        float normalizedX = (point.x - xMin) / (xMax - xMin);
        float normalizedY = (point.y - yMin) / (yMax - yMin);

        // Add padding to prevent points from being drawn at the very edge
        float padding = 10f;
        float usableWidth = graphWidth - (padding * 2);
        float usableHeight = graphHeight - (padding * 2);

        return new Vector2(
            padding + (normalizedX * usableWidth),
            graphHeight - (padding + (normalizedY * usableHeight))
        );
    }

    private void DrawLine(Vector2 pointA, Vector2 pointB)
    {
        GameObject lineObject = new GameObject("Line", typeof(Image));
        lineObject.transform.SetParent(graphContainer, false);

        RectTransform rectTransform = lineObject.GetComponent<RectTransform>();
        Image lineImage = lineObject.GetComponent<Image>();
        lineImage.color = lineColor;

        Vector2 direction = (pointB - pointA).normalized;
        float distance = Vector2.Distance(pointA, pointB);

        // Set anchor points to top-left corner
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;

        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        rectTransform.anchoredPosition = pointA;
        rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public void UpdateValueText(string text)
    {
        if (chartTitle != null)
        {
            chartTitle.text = text;
        }
    }
    private void DrawXAxis(float xMin, float xMax)
    {
        // Clear previous X-axis labels
        foreach (Transform child in xAxisContainer)
        {
            Destroy(child.gameObject);
        }

        // Get graph dimensions
        float graphWidth = graphContainer.rect.width;

        // Generate X-axis labels
        for (int i = 0; i <= xLabelCount; i++)
        {
            float normalizedValue = i / (float)xLabelCount;
            float xValue = Mathf.Lerp(xMin, xMax, normalizedValue);

            // Convert epoch to DateTime
            System.DateTime dateTime = System.DateTimeOffset.FromUnixTimeSeconds((long)xValue).DateTime;

            // Create label
            TextMeshProUGUI label = Instantiate(xLabelPrefab, xAxisContainer);
            label.text = dateTime.ToString("MM/dd HH:mm"); // Format as "Month/Day Hour:Minute"

            // Get the label's RectTransform
            RectTransform labelRect = label.rectTransform;

            // Set a consistent width for each label container
            float labelWidth = graphWidth / (xLabelCount + 1);
            labelRect.sizeDelta = new Vector2(labelWidth, labelRect.sizeDelta.y);
        }
    }
    private void DrawYAxis(float yMin, float yMax)
    {
        // Clear previous Y-axis labels
        foreach (Transform child in yAxisContainer)
        {
            Destroy(child.gameObject);
        }

        // Get graph dimensions
        float graphHeight = graphContainer.rect.height;

        // Generate Y-axis labels
        for (int i = 0; i <= yLabelCount; i++)
        {
            float normalizedValue = i / (float)yLabelCount;
            float yValue = Mathf.Lerp(yMin, yMax, normalizedValue);

            // Create label
            TextMeshProUGUI label = Instantiate(yLabelPrefab, yAxisContainer);
            label.text = yValue.ToString("F2");  // Format to two decimal places

            // Get the label's RectTransform
            RectTransform labelRect = label.rectTransform;

            // Set a consistent height for each label container
            float labelHeight = graphHeight / (yLabelCount + 1);
            labelRect.sizeDelta = new Vector2(labelRect.sizeDelta.x, labelHeight);
        }
    }

}
