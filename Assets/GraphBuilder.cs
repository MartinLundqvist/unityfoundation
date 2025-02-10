using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Attribute
{
    public string name;
    public string value;
}

[Serializable]
public class Vertex
{
    public string id;
    public string name;
    public string type; // For example, "root", "sensor", "asset", "compute"
    public Attribute[] attributes;
    // You could add more fields (like attributes) if needed.

}

[Serializable]
public class Relationship
{
    public string type;
    public string fromVertexId;
    public string toVertexId;
    // You can include other fields like id, attributes, etc., if needed.
}

[Serializable]
public class GraphData
{
    public Vertex[] vertices;
    public Relationship[] relationships;
}

public class GraphBuilder : MonoBehaviour
{
    // Reference to a TextAsset containing your JSON (assign in Inspector)
    public TextAsset jsonGraphData;
    public bool useLocal = true;
    private DataLoader dataLoader;
    private string apiUrl;
    private string bearerToken;
    private string rootAssetID;

    void Awake()
    {
        dataLoader = new DataLoader();

        apiUrl = "https://dev.domain.foundation.arundo.com/domain/" + AdminManager.Instance.DomainID + "/graph";// f9a6e31b-c309-49b2-a81f-46c26f50dcc3
        bearerToken = AdminManager.Instance.BearerToken;
        rootAssetID = AdminManager.Instance.RootAssetID;
    }

    // Synchronous method for local data
    public Graph BuildGraphFromJson()
    {
        if (!useLocal)
        {
            Debug.LogWarning("BuildGraphFromJson called with useLocal=false. Use BuildGraphFromJsonAsync instead.");
            return new Graph();
        }

        return BuildGraphFromJsonText(jsonGraphData.text);
    }

    // Asynchronous method for API data
    public IEnumerator BuildGraphFromJsonAsync(System.Action<Graph> onGraphBuilt)
    {
        if (useLocal)
        {
            Debug.Log("Using local graph data");
            onGraphBuilt?.Invoke(BuildGraphFromJson());
            yield break;
        }

        Debug.Log("Fetching graph data from API...");
        Debug.Log($"API URL: {apiUrl}");

        // Reset these in case they were changed by the AdminManager
        apiUrl = "https://dev.domain.foundation.arundo.com/domain/" + AdminManager.Instance.DomainID + "/graph";// f9a6e31b-c309-49b2-a81f-46c26f50dcc3
        bearerToken = AdminManager.Instance.BearerToken;
        rootAssetID = AdminManager.Instance.RootAssetID;

        string requestBody = @"{
            ""entrypoints"": [{
                ""id"": """ + rootAssetID + @"""
            }],
            ""vertices"": [""Asset"", ""Sensor""],
            ""returnVertices"": [""Asset"", ""Sensor""],
            ""relationships"": [{
                ""type"": ""HAS_CHILD"",
                ""direction"": ""both""
            }, {
                ""type"": ""HAS_POINTER"",
                ""direction"": ""outgoing""
            }, {
                ""type"": ""HAS_SOURCE"",
                ""direction"": ""outgoing""
            }, {
                ""type"": ""HAS_INPUT"",
                ""direction"": ""both""
            }, {
                ""type"": ""HAS_OUTPUT"",
                ""direction"": ""both""
            }, {
                ""type"": ""USES_TEMPLATE"",
                ""direction"": ""outgoing""
            }],
            ""depth"": 10,
            ""identifiablePaths"": [{
                ""type"": ""HAS_CHILD"",
                ""direction"": ""incoming""
            }, {
                ""type"": ""HAS_POINTER"",
                ""direction"": ""incoming""
            }, {
                ""type"": ""EXTENDS_TEMPLATE"",
                ""direction"": ""outgoing""
            }, {
                ""type"": ""EXTENDS_ROOT_TEMPLATE"",
                ""direction"": ""outgoing""
            }]
        }";

        Debug.Log($"Request Body: {requestBody}");

        yield return dataLoader.FetchData(
            apiUrl,
            bearerToken,
            requestBody,
            (jsonResponse) =>
            {
                try
                {
                    Debug.Log($"Received API response: {jsonResponse.Substring(0, Mathf.Min(500, jsonResponse.Length))}...");
                    Graph graph = BuildGraphFromJsonText(jsonResponse);
                    Debug.Log($"Successfully built graph with {graph.GetAllNodes().Count} nodes");
                    onGraphBuilt?.Invoke(graph);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing graph data: {e.Message}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                    onGraphBuilt?.Invoke(new Graph());
                }
            },
            (error) =>
            {
                Debug.LogError($"Error fetching graph data: {error}");
                onGraphBuilt?.Invoke(new Graph());
            }
        );
    }

    private Graph BuildGraphFromJsonText(string jsonText)
    {
        try
        {
            // Parse the JSON file
            GraphData graphData = JsonUtility.FromJson<GraphData>(jsonText);
            Debug.Log($"Parsed JSON data: {graphData.vertices?.Length ?? 0} vertices, {graphData.relationships?.Length ?? 0} relationships");

            Dictionary<string, GraphNode> nodesById = new Dictionary<string, GraphNode>();
            Graph graph = new Graph();

            // Create nodes
            if (graphData.vertices != null)
            {
                foreach (Vertex vertex in graphData.vertices)
                {
                    GraphNode node = new GraphNode(vertex.id, vertex.name, vertex.type);
                    if (vertex.attributes != null)
                    {
                        foreach (Attribute attr in vertex.attributes)
                        {
                            node.attributes.Add(attr.name, attr.value);
                        }
                    }
                    graph.AddNode(node);
                    nodesById.Add(vertex.id, node);
                }
                Debug.Log($"Created {nodesById.Count} nodes");
            }
            else
            {
                Debug.LogWarning("No vertices found in graph data");
            }

            // Set up relationships
            if (graphData.relationships != null)
            {
                int relationshipCount = 0;
                foreach (Relationship rel in graphData.relationships)
                {
                    if (rel.type == "HAS_CHILD")
                    {
                        if (nodesById.ContainsKey(rel.fromVertexId) && nodesById.ContainsKey(rel.toVertexId))
                        {
                            GraphNode parent = nodesById[rel.fromVertexId];
                            GraphNode child = nodesById[rel.toVertexId];
                            child.parent = parent;
                            parent.children.Add(child);
                            relationshipCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"Relationship {rel.type} skipped because one of the vertices was not found: {rel.fromVertexId} or {rel.toVertexId}");
                        }
                    }
                }
                Debug.Log($"Processed {relationshipCount} parent-child relationships");
            }
            else
            {
                Debug.LogWarning("No relationships found in graph data");
            }

            return graph;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in BuildGraphFromJsonText: {e.Message}");
            Debug.LogError($"JSON text: {jsonText}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            throw;
        }
    }
}
