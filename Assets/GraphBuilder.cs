using UnityEngine;
using System;
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

    // This method parses the JSON and builds the graph.
    public Graph BuildGraphFromJson()
    {
        // Parse the JSON file.
        GraphData graphData = JsonUtility.FromJson<GraphData>(jsonGraphData.text);

        // Graph graph = new Graph();
        // A dictionary for quick lookup of nodes by their id.
        Dictionary<string, GraphNode> nodesById = new Dictionary<string, GraphNode>();

        // Create a graph
        Graph graph = new Graph();

        // Create a node for each vertex.
        foreach (Vertex vertex in graphData.vertices)
        {
            // Create a GraphNode using the vertex information.
            GraphNode node = new GraphNode(vertex.id, vertex.name, vertex.type);
            // Add attributes if they exist
            if (vertex.attributes != null)
            {
                foreach (Attribute attr in vertex.attributes)
                {
                    node.attributes.Add(attr.name, attr.value);
                }
            }
            // Add it to the graph.
            graph.AddNode(node);
            nodesById.Add(vertex.id, node);
        }

        // Now, go through each relationship and set up the parent-child relationships.
        foreach (Relationship rel in graphData.relationships)
        {
            if (rel.type == "HAS_CHILD")
            {
                // Ensure both vertices exist.
                if (nodesById.ContainsKey(rel.fromVertexId) && nodesById.ContainsKey(rel.toVertexId))
                {
                    GraphNode parent = nodesById[rel.fromVertexId];
                    GraphNode child = nodesById[rel.toVertexId];
                    // Set the relationship (assuming each child has only one parent).
                    child.parent = parent;
                    parent.children.Add(child);
                }
                else
                {
                    Debug.LogWarning($"Relationship {rel.type} skipped because one of the vertices was not found: {rel.fromVertexId} or {rel.toVertexId}");
                }
            }
        }

        return graph;
    }
}
