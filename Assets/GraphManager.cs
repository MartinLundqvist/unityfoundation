using UnityEngine;

public class GraphManager : MonoBehaviour
{
    // The in‑memory graph instance.
    public Graph graph;

    // Prefab for a node (e.g., a sphere). If not set, a primitive sphere will be created.
    public GameObject nodePrefab;

    // Material for rendering edges.
    public Material lineMaterial;

    // This method will be called when the scene starts.
    void Start()
    {
        // Initialize the graph.
        graph = new Graph();

        // Create some sample nodes with parent-child relationships.
        // Here, node "A" will be a root node.
        graph.AddNode("A"); // root

        // Add child nodes. Each child has only one parent.
        graph.AddNode("B", "A");
        graph.AddNode("C", "A");
        graph.AddNode("D", "B");
        graph.AddNode("E", "B");
        graph.AddNode("F", "C");

        // (Optional) Demonstrate CRUD operations:
        // For example, update node id "F" to "F1":
        // graph.UpdateNode("F", "F1");

        // Now, render the graph.
        RenderGraph();
    }

    // Render the graph. In this version, we compute positions dynamically.
    // Here we simply lay out nodes in a circle for demonstration purposes.
    // In your production code, replace this with your actual layout algorithm.
    void RenderGraph()
    {
        // Get all nodes from the graph.
        var nodes = graph.GetAllNodes();

        // For simplicity, we compute positions in a circle around the origin.
        // In a real application, you might use a force-directed or hierarchical layout.
        int nodeCount = nodes.Count;
        float radius = 5f;
        for (int i = 0; i < nodeCount; i++)
        {
            float angle = (i / (float)nodeCount) * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            // Create or update the node GameObject.
            GameObject nodeObj = (nodePrefab != null)
                ? Instantiate(nodePrefab, pos, Quaternion.identity)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            nodeObj.transform.position = pos;
            nodeObj.name = "Node " + nodes[i].id;

            // Optionally adjust the scale.
            nodeObj.transform.localScale = Vector3.one * 0.5f;
        }

        // Render edges as lines between parent and child nodes.
        foreach (GraphNode node in nodes)
        {
            if (node.parent != null)
            {
                CreateEdge(node.parent, node);
            }
        }
    }

    // Creates an edge (LineRenderer) between a parent and child node.
    void CreateEdge(GraphNode parent, GraphNode child)
    {
        // Find the corresponding GameObjects by name.
        // In a more robust solution, you would store a mapping between GraphNodes and their GameObjects.
        GameObject parentObj = GameObject.Find("Node " + parent.id);
        GameObject childObj = GameObject.Find("Node " + child.id);

        if (parentObj == null || childObj == null)
        {
            return;
        }

        // Create a new GameObject to hold the LineRenderer.
        GameObject edgeObject = new GameObject("Edge " + parent.id + "-" + child.id);
        LineRenderer lr = edgeObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, parentObj.transform.position);
        lr.SetPosition(1, childObj.transform.position);
        lr.material = lineMaterial;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.startColor = Color.white;
        lr.endColor = Color.white;
    }
}
