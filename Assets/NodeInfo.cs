using UnityEngine;
using System.Collections.Generic;

public class NodeInfo : MonoBehaviour
{
    // These fields can be set per node (via the Inspector or dynamically).
    public string nodeID;
    public string nodeType;
    public string nodeName;
    public string description;
    public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public List<Vector2> timeSeriesData = new List<Vector2>();
}
