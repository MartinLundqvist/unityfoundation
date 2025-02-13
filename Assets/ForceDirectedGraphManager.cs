using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ForceDirectedGraphManager : MonoBehaviour
{
    // Existing prefab and material references from GraphManager
    public GameObject nodePrefab;
    public Material nodeMaterial;
    public GameObject rootPrefab;
    public GameObject sensorPrefab;
    public GameObject assetPrefab;
    public GameObject computePrefab;
    public Material rootMaterial;
    public Material sensorMaterial;
    public Material assetMaterial;
    public Material computeMaterial;
    public Material lineMaterial;

    // Force-directed graph parameters
    public float repulsionForce = 50f;
    public float springForce = 0.5f;
    public float springLength = 5f;
    public float damping = 0.8f;
    public float timeStep = 0.1f;
    public int maxIterations = 1000;
    public float convergenceThreshold = 0.01f;

    private Graph graph;
    private Dictionary<GraphNode, NodeData> nodeData = new Dictionary<GraphNode, NodeData>();
    private bool isSimulating = false;

    private class NodeData
    {
        public Vector3 position;
        public Vector3 velocity;
        public GameObject gameObject;

        public NodeData(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
        }
    }

    public void InitializeGraph(Graph inputGraph)
    {
        // if (isSimulating)
        // {
        //     Debug.LogWarning("Cannot initialize new graph while simulation is running");
        //     return;
        // }

        Debug.Log("Initializing graph with " + inputGraph.GetAllNodes().Count + " nodes");
        graph = inputGraph;
        ClearExistingVisualization();
        InitializeNodePositions();
        StartSimulation();
    }

    public void StopSimulation()
    {
        if (isSimulating)
        {
            isSimulating = false;
            StopAllCoroutines();
        }
    }

    private void ClearExistingVisualization()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        nodeData.Clear();
    }

    private void InitializeNodePositions()
    {
        // Initialize nodes in a spherical distribution
        float radius = 10f;
        int nodeCount = graph.GetAllNodes().Count;
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        int i = 0;
        foreach (GraphNode node in graph.GetAllNodes())
        {
            float t = (float)i / nodeCount;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = radius * Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = radius * Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = radius * Mathf.Cos(inclination);

            nodeData[node] = new NodeData(new Vector3(x, y, z));
            i++;
        }
    }

    private void StartSimulation()
    {
        Debug.Log("Starting force-directed simulation");
        // Create all node objects before starting simulation
        foreach (GraphNode node in graph.GetAllNodes())
        {
            CreateNodeObject(node);
        }

        // Create initial edges
        foreach (GraphNode node in graph.GetAllNodes())
        {
            if (node.parent != null)
            {
                CreateEdge(nodeData[node.parent].position, nodeData[node].position);
            }
        }

        isSimulating = true;
        StartCoroutine(SimulateGraph());
    }

    private System.Collections.IEnumerator SimulateGraph()
    {
        int iteration = 0;
        float totalMovement = float.MaxValue;

        while (/*isSimulating &&*/ iteration < maxIterations && totalMovement > convergenceThreshold)
        {
            totalMovement = UpdateNodePositions();
            UpdateVisualization();
            iteration++;
            yield return new WaitForSeconds(timeStep);
        }

        isSimulating = false;
    }

    private float UpdateNodePositions()
    {
        Dictionary<GraphNode, Vector3> forces = new Dictionary<GraphNode, Vector3>();
        float totalMovement = 0f;

        // Initialize forces
        foreach (GraphNode node in graph.GetAllNodes())
        {
            forces[node] = Vector3.zero;
        }

        // Calculate repulsion forces between all nodes
        var nodes = graph.GetAllNodes();
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                GraphNode node1 = nodes[i];
                GraphNode node2 = nodes[j];
                Vector3 delta = nodeData[node2].position - nodeData[node1].position;
                float distance = delta.magnitude;

                if (distance > 0.1f)
                {
                    Vector3 repulsion = delta.normalized * repulsionForce / (distance * distance);
                    forces[node1] -= repulsion;
                    forces[node2] += repulsion;
                }
            }
        }

        // Calculate spring forces for connected nodes
        foreach (GraphNode node in graph.GetAllNodes())
        {
            if (node.parent != null)
            {
                Vector3 delta = nodeData[node].position - nodeData[node.parent].position;
                float distance = delta.magnitude;
                Vector3 spring = delta.normalized * springForce * (distance - springLength);

                forces[node] -= spring;
                forces[node.parent] += spring;
            }
        }

        // Update positions and velocities
        foreach (GraphNode node in graph.GetAllNodes())
        {
            NodeData data = nodeData[node];
            data.velocity = (data.velocity + forces[node] * timeStep) * damping;
            Vector3 movement = data.velocity * timeStep;
            data.position += movement;
            totalMovement += movement.magnitude;
        }

        return totalMovement;
    }

    private void UpdateVisualization()
    {
        foreach (var kvp in nodeData)
        {
            if (kvp.Value.gameObject != null)
            {
                kvp.Value.gameObject.transform.position = kvp.Value.position;
            }
        }

        // Update edge positions
        int edgeIndex = 0;
        foreach (GraphNode node in graph.GetAllNodes())
        {
            if (node.parent != null)
            {
                Transform edgeTransform = transform.GetChild(graph.GetAllNodes().Count + edgeIndex);
                Vector3 startPos = nodeData[node.parent].position;
                Vector3 endPos = nodeData[node].position;

                Vector3 offset = endPos - startPos;
                float length = offset.magnitude;

                edgeTransform.position = (startPos + endPos) / 2f;
                edgeTransform.localScale = new Vector3(0.1f, length / 2f, 0.1f);
                edgeTransform.rotation = Quaternion.FromToRotation(Vector3.up, offset.normalized);

                edgeIndex++;
            }
        }
    }

    private void CreateNodeObject(GraphNode node)
    {
        GameObject prefabToUse = nodePrefab;
        Material materialToUse = nodeMaterial;
        string type = node.nodeType.ToLower();

        switch (type)
        {
            case "root":
                prefabToUse = rootPrefab != null ? rootPrefab : nodePrefab;
                materialToUse = rootMaterial != null ? rootMaterial : nodeMaterial;
                break;
            case "sensor":
                prefabToUse = sensorPrefab != null ? sensorPrefab : nodePrefab;
                materialToUse = sensorMaterial != null ? sensorMaterial : nodeMaterial;
                break;
            case "asset":
                prefabToUse = assetPrefab != null ? assetPrefab : nodePrefab;
                materialToUse = assetMaterial != null ? assetMaterial : nodeMaterial;
                break;
            case "compute":
                prefabToUse = computePrefab != null ? computePrefab : nodePrefab;
                materialToUse = computeMaterial != null ? computeMaterial : nodeMaterial;
                break;
        }

        // if (prefabToUse == null)
        // {
        //     prefabToUse = nodePrefab != null ? nodePrefab : GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // }

        GameObject nodeObj = Instantiate(prefabToUse, nodeData[node].position, Quaternion.identity, transform);
        nodeData[node].gameObject = nodeObj;

        nodeObj.name = $"Node {node.id} ({node.nodeType})";

        // Only adjust cube height for asset nodes
        if (type == "asset")
        {
            // Get the LabelManager component
            LabelManager labelManager = nodeObj.GetComponentInChildren<LabelManager>();
            if (labelManager != null)
            {
                labelManager.SetLabel(node.name);
            }
            else
            {
                Debug.LogWarning($"LabelManager component missing on prefab for node {node.id}");
            }
            // // Get the TextMeshPro component
            // TextMeshPro textMesh = nodeObj.GetComponentInChildren<TextMeshPro>();
            // if (textMesh != null)
            // {
            //     textMesh.text = node.name;
            //     textMesh.ForceMeshUpdate();

            //     // Force layout update
            //     var layoutGroup = textMesh.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            //     if (layoutGroup != null)
            //     {
            //         Canvas.ForceUpdateCanvases();
            //         layoutGroup.SetLayoutHorizontal();
            //         layoutGroup.SetLayoutVertical();
            //     }
            // }
        }

        Renderer renderer = nodeObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = materialToUse;
        }

        // Get NodeInfo component (should already be on the prefab)
        NodeInfo nodeInfo = nodeObj.GetComponent<NodeInfo>();
        if (nodeInfo != null)
        {
            nodeInfo.nodeType = node.nodeType.ToLower();
            nodeInfo.nodeID = node.id;
            nodeInfo.nodeName = node.name;

            if (node.attributes != null)
            {
                foreach (var attr in node.attributes)
                {
                    nodeInfo.attributes[attr.Key] = attr.Value;
                }
            }
        }
        else
        {
            Debug.LogWarning($"NodeInfo component missing on prefab for node {node.id}");
        }
    }

    private void CreateEdge(Vector3 startPos, Vector3 endPos)
    {
        // Use the existing edge creation logic from GraphManager
        Vector3 offset = endPos - startPos;
        float length = offset.magnitude;

        GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tube.transform.SetParent(transform);
        tube.transform.position = (startPos + endPos) / 2f;
        tube.transform.localScale = new Vector3(0.1f, length / 2f, 0.1f);
        tube.transform.rotation = Quaternion.FromToRotation(Vector3.up, offset.normalized);

        if (lineMaterial != null)
        {
            Renderer rend = tube.GetComponent<Renderer>();
            rend.material = lineMaterial;
        }
    }
}