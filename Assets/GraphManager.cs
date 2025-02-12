using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public class GraphManager : MonoBehaviour
{
    // In‑memory graph instance (assume this uses your previously defined Graph class)
    public Graph graph;

    // Prefab for rendering a node (e.g., a sphere). If not assigned, a primitive sphere will be created.
    public GameObject nodePrefab;

    // Material for rendering nodes.
    public Material nodeMaterial;

    // Prefabs for different node types.
    public GameObject rootPrefab;
    public GameObject sensorPrefab;
    public GameObject assetPrefab;
    public GameObject computePrefab;

    // Materials for different node types.
    public Material rootMaterial;
    public Material sensorMaterial;
    public Material assetMaterial;
    public Material computeMaterial;


    // Material for rendering edges.
    public Material lineMaterial;

    // Vertical spacing between layers (Y axis).
    public float layerSpacing = 3f;

    // Base spacing between nodes in the XZ plane.
    public float gridSpacing = 2f;

    // Multiplier to compress spacing between siblings (children of the same parent).
    public float siblingSpacingMultiplier = 0.5f;

    // Minimum gap (in X) to enforce between distinct sibling groups.
    public float groupMinGap = 0.5f;

    // Increase this value to spread out nodes further along Z.
    public float zSpacingMultiplier = 1.5f;

    // Adjust as needed
    public float tubeThickness = 0.1f;

    private GameObject fallbackNodePrefab;

    // Dictionary to store computed positions for nodes (for rendering purposes).
    private Dictionary<GraphNode, Vector3> computedPositions = new Dictionary<GraphNode, Vector3>();

    [SerializeField]
    private TextAsset graphDefinition; // Assign in inspector or load from Resources

    [System.Serializable]
    private class NodeDefinition
    {
        public string id;
        public string type;
        public string parent;
    }

    [System.Serializable]
    private class GraphDefinition
    {
        public NodeDefinition[] nodes;
    }

    // A helper class to hold a parent's children group layout.
    private class SiblingGroup
    {
        public string parentId;
        public Vector3 parentPos;      // The parent's computed XZ position.
        public float childY;           // The Y coordinate for the children (determined by layer).
        public List<GraphNode> children;
        public List<Vector3> desiredPositions; // The desired positions for each child (before group shifting).
        public float minX;             // Bounding box in X (minimum X among children positions).
        public float maxX;             // Bounding box in X (maximum X among children positions).
    }

    void Start()
    {
        // Create and cache the fallback prefab only once
        fallbackNodePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fallbackNodePrefab.SetActive(false);

        // Build the graph 
        GraphBuilder builder = GetComponent<GraphBuilder>();

        if (builder.useLocal)
        {
            graph = builder.BuildGraphFromJson();
            GetComponent<ForceDirectedGraphManager>().InitializeGraph(graph);
        }
        else
        {
            StartCoroutine(builder.BuildGraphFromJsonAsync(graph =>
            {
                this.graph = graph;
                GetComponent<ForceDirectedGraphManager>().InitializeGraph(graph);
            }));
        }
    }

    // New method to encapsulate the graph visualization logic
    private void BuildGraphVisualization()
    {
        Dictionary<int, List<GraphNode>> layers = ComputeLayers();
        RefineLayerOrdering(layers);
        UpdateParentPositions(layers);
        RenderGraph();
    }

    #region Layer Computation

    /// <summary>
    /// Groups all nodes by layer.
    /// A node's layer is defined as 0 if it has no parent; otherwise, it is parent's layer + 1.
    /// </summary>
    Dictionary<int, List<GraphNode>> ComputeLayers()
    {
        Dictionary<int, List<GraphNode>> layers = new Dictionary<int, List<GraphNode>>();
        foreach (GraphNode node in graph.GetAllNodes())
        {
            int layer = ComputeNodeLayer(node);
            if (!layers.ContainsKey(layer))
            {
                layers[layer] = new List<GraphNode>();
            }
            layers[layer].Add(node);
        }
        return layers;
    }

    /// <summary>
    /// Recursively computes the layer of a node.
    /// </summary>
    int ComputeNodeLayer(GraphNode node)
    {
        if (node.parent == null)
            return 0;
        return ComputeNodeLayer(node.parent) + 1;
    }

    #endregion

    #region Layout Refinement: Sibling Grouping & Non-Overlap Adjustment

    /// <summary>
    /// Assign positions for nodes in each layer.
    /// - For layer 0, arrange nodes in a grid in the XZ plane.
    /// - For subsequent layers, group nodes by their parent's id.
    ///   For each group, compute a desired layout (grid centered on the parent's position).
    ///   Then, adjust entire groups along X to ensure groups do not overlap.
    /// </summary>
    void RefineLayerOrdering(Dictionary<int, List<GraphNode>> layers)
    {
        // --- Layer 0: Arrange root nodes in a grid in the XZ plane.
        if (layers.ContainsKey(0))
        {
            List<GraphNode> topLayer = layers[0];
            int count = topLayer.Count;
            int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            int rows = Mathf.CeilToInt(count / (float)columns);

            float totalWidth = (columns - 1) * gridSpacing;
            float totalDepth = (rows - 1) * gridSpacing;
            float xOffset = -totalWidth / 2;
            float zOffset = -totalDepth / 2;

            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                float x = xOffset + col * gridSpacing;
                float z = zOffset + row * gridSpacing * zSpacingMultiplier;
                computedPositions[topLayer[i]] = new Vector3(x, 0, z);
            }
        }

        // --- For each subsequent layer (layer > 0):
        foreach (var kvp in layers)
        {
            int layerIndex = kvp.Key;
            if (layerIndex == 0)
                continue;

            List<GraphNode> layerNodes = kvp.Value;

            // Group nodes by their parent's id so that siblings are clustered together.
            Dictionary<string, List<GraphNode>> siblingGroups = new Dictionary<string, List<GraphNode>>();
            foreach (GraphNode node in layerNodes)
            {
                string parentId = node.parent.id; // assume parent's id is set
                if (!siblingGroups.ContainsKey(parentId))
                    siblingGroups[parentId] = new List<GraphNode>();
                siblingGroups[parentId].Add(node);
            }

            // Helper: a temporary structure to hold each group's desired layout.
            List<SiblingGroup> groups = new List<SiblingGroup>();
            float effectiveSpacing = gridSpacing * siblingSpacingMultiplier;
            // Define a minimum group width (you can adjust this value).
            float minGroupWidth = effectiveSpacing;

            foreach (var groupKvp in siblingGroups)
            {
                SiblingGroup group = new SiblingGroup();
                group.parentId = groupKvp.Key;
                group.children = groupKvp.Value;

                // Ensure the parent's position exists in computedPositions
                GraphNode parentNode = group.children[0].parent;
                if (!computedPositions.ContainsKey(parentNode))
                {
                    Debug.LogWarning($"Parent position not found for node {parentNode.id}. Using default position.");
                    computedPositions[parentNode] = Vector3.zero;
                }

                group.parentPos = computedPositions[parentNode];
                group.childY = layerIndex * layerSpacing;

                // Compute desired positions for this group.
                if (group.children.Count == 1)
                {
                    // For a single child, position it at the parent's XZ position.
                    Vector3 desired = new Vector3(group.parentPos.x, group.childY, group.parentPos.z);
                    group.desiredPositions = new List<Vector3>() { desired };

                    // Force a minimum bounding width.
                    group.minX = desired.x - minGroupWidth / 2f;
                    group.maxX = desired.x + minGroupWidth / 2f;
                }
                else
                {
                    int sibCount = group.children.Count;
                    int columns = Mathf.CeilToInt(Mathf.Sqrt(sibCount));
                    int rows = Mathf.CeilToInt(sibCount / (float)columns);
                    float totalWidth = (columns - 1) * effectiveSpacing;
                    float totalDepth = (rows - 1) * effectiveSpacing;
                    float offsetX = -totalWidth / 2f;
                    float offsetZ = -totalDepth / 2f;
                    List<Vector3> posList = new List<Vector3>();
                    for (int i = 0; i < sibCount; i++)
                    {
                        int col = i % columns;
                        int row = i / columns;
                        float x = group.parentPos.x + offsetX + col * effectiveSpacing;
                        float z = group.parentPos.z + offsetZ + row * effectiveSpacing * zSpacingMultiplier;
                        posList.Add(new Vector3(x, group.childY, z));
                    }
                    group.desiredPositions = posList;

                    // Compute the bounding box in X for this group.
                    float minX = float.MaxValue, maxX = float.MinValue;
                    foreach (Vector3 pos in posList)
                    {
                        if (pos.x < minX) minX = pos.x;
                        if (pos.x > maxX) maxX = pos.x;
                    }
                    // Ensure the group has at least the minimum width.
                    float width = maxX - minX;
                    if (width < minGroupWidth)
                    {
                        float adjustment = (minGroupWidth - width) / 2f;
                        minX -= adjustment;
                        maxX += adjustment;
                    }
                    group.minX = minX;
                    group.maxX = maxX;
                }

                groups.Add(group);
            }

            // Adjust groups along the X axis so that there is at least groupMinGap between adjacent groups.
            groups.Sort((a, b) => a.minX.CompareTo(b.minX));
            for (int i = 1; i < groups.Count; i++)
            {
                SiblingGroup prev = groups[i - 1];
                SiblingGroup curr = groups[i];
                if (curr.minX < prev.maxX + groupMinGap)
                {
                    float shift = (prev.maxX + groupMinGap) - curr.minX;
                    // Shift the entire current group.
                    for (int j = 0; j < curr.desiredPositions.Count; j++)
                    {
                        Vector3 p = curr.desiredPositions[j];
                        curr.desiredPositions[j] = new Vector3(p.x + shift, p.y, p.z);
                    }
                    curr.minX += shift;
                    curr.maxX += shift;
                }
            }

            // Finally, assign these positions to the computedPositions for each child.
            foreach (SiblingGroup group in groups)
            {
                for (int i = 0; i < group.children.Count; i++)
                {
                    computedPositions[group.children[i]] = group.desiredPositions[i];
                }
            }
        }

    }

    #endregion

    #region Bottom-Up Centering: Updating Parent Positions

    /// <summary>
    /// Recalculate parent positions so that each parent is centered underneath its children.
    /// This is done in a bottom-up pass from the deepest layer to layer 0.
    /// </summary>
    void UpdateParentPositions(Dictionary<int, List<GraphNode>> layers)
    {
        // Get all layer indices and sort them in descending order.
        List<int> layerIndices = new List<int>(layers.Keys);
        layerIndices.Sort();
        for (int i = layerIndices.Count - 1; i >= 0; i--)
        {
            int layer = layerIndices[i];
            // Process only nodes that have children.
            foreach (GraphNode node in layers[layer])
            {
                if (node.children != null && node.children.Count > 0)
                {
                    float minX = float.MaxValue, maxX = float.MinValue;
                    float minZ = float.MaxValue, maxZ = float.MinValue;
                    foreach (GraphNode child in node.children)
                    {
                        // Ensure the child has been assigned a position.
                        if (computedPositions.ContainsKey(child))
                        {
                            Vector3 pos = computedPositions[child];
                            if (pos.x < minX) minX = pos.x;
                            if (pos.x > maxX) maxX = pos.x;
                            if (pos.z < minZ) minZ = pos.z;
                            if (pos.z > maxZ) maxZ = pos.z;
                        }
                    }
                    // If at least one child has a valid position, update the parent's XZ position.
                    if (minX != float.MaxValue && maxX != float.MinValue)
                    {
                        float centerX = (minX + maxX) / 2f;
                        float centerZ = (minZ + maxZ) / 2f;
                        // Parent's Y remains its current value.
                        Vector3 oldPos = computedPositions[node];
                        computedPositions[node] = new Vector3(centerX, oldPos.y, centerZ);
                    }
                }
            }
        }
    }

    #endregion

    #region Rendering

    /// <summary>
    /// Instantiate node GameObjects at the computed positions and draw edges (via LineRenderer)
    /// between each node and its parent.
    /// </summary>
    void RenderGraph()
    {
        // Instantiate node GameObjects.
        foreach (var kv in computedPositions)
        {
            GraphNode node = kv.Key;
            Vector3 pos = kv.Value;
            GameObject nodeObj = null;
            string type = node.nodeType.ToLower();

            // Choose the prefab based on the node type.
            GameObject prefabToUse = null;
            switch (type)
            {
                case "root":
                    if (rootPrefab != null)
                        prefabToUse = rootPrefab;
                    break;
                case "sensor":
                    if (sensorPrefab != null)
                        prefabToUse = sensorPrefab;
                    break;
                case "asset":
                    if (assetPrefab != null)
                        prefabToUse = assetPrefab;
                    break;
                case "compute":
                    if (computePrefab != null)
                        prefabToUse = computePrefab;
                    break;
            }
            // Fallback if no prefab is assigned.
            if (prefabToUse == null)
            {
                prefabToUse = nodePrefab != null ? nodePrefab : fallbackNodePrefab;
            }

            nodeObj = Instantiate(prefabToUse, pos, Quaternion.identity, this.transform);
            nodeObj.SetActive(true); // Ensure it becomes visible.

            nodeObj.name = "Node " + node.id + " (" + node.nodeType + ")";

            // Set the node name text if there's a TextMeshPro component in children
            TMPro.TextMeshPro textComponent = nodeObj.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = node.name;
            }

            // Optionally adjust the scale.
            nodeObj.transform.localScale = Vector3.one * 0.5f;

            // Assign the appropriate material.
            Renderer rend = nodeObj.GetComponent<Renderer>();
            if (rend != null)
            {
                Material materialToUse = nodeMaterial;

                switch (type)
                {
                    case "root":
                        if (rootMaterial != null)
                            materialToUse = rootMaterial;
                        break;
                    case "sensor":
                        if (sensorMaterial != null)
                            materialToUse = sensorMaterial;
                        break;
                    case "asset":
                        if (assetMaterial != null)
                            materialToUse = assetMaterial;
                        break;
                    case "compute":
                        if (computeMaterial != null)
                            materialToUse = computeMaterial;
                        break;
                }

                rend.material = materialToUse;
            }
            // Update the NodeInfo component with the node's information
            NodeInfo nodeInfo = nodeObj.GetComponent<NodeInfo>();
            if (nodeInfo != null)
            {
                nodeInfo.nodeType = type;
                nodeInfo.nodeID = node.id;
                nodeInfo.nodeName = node.name;

                // Copy attributes
                if (node.attributes != null)
                {
                    foreach (var attr in node.attributes)
                    {
                        nodeInfo.attributes[attr.Key] = attr.Value;
                    }
                }
            }
        }

        // Create edges connecting nodes to their parents.
        foreach (GraphNode node in graph.GetAllNodes())
        {
            if (node.parent != null)
            {
                CreateEdge(computedPositions[node.parent], computedPositions[node]);
            }
        }
    }

    /// <summary>
    /// Creates an edge between two positions using a Cylinder.
    /// </summary>
    void CreateEdge(Vector3 startPos, Vector3 endPos)
    {
        // Compute the vector from start to end.
        Vector3 offset = endPos - startPos;
        float length = offset.magnitude;

        // Create a cylinder primitive.
        GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tube.transform.SetParent(this.transform);
        // Position the tube at the midpoint.
        tube.transform.position = (startPos + endPos) / 2f;

        // By default, a Unity cylinder's height is along the Y axis (and its default height is 2).
        // Scale the cylinder:
        // - The Y-scale is set to half the edge length (because 2 * (length/2) = length).
        // - The X and Z scales set the tube's thickness.
        tube.transform.localScale = new Vector3(tubeThickness, length / 2f, tubeThickness);

        // Rotate the cylinder so its Y-axis aligns with the edge.
        // One way is to use FromToRotation, which creates a rotation that rotates Vector3.up into the desired direction.
        tube.transform.rotation = Quaternion.FromToRotation(Vector3.up, offset.normalized);

        // Optionally, assign a material to the tube.
        if (lineMaterial != null)
        {
            Renderer rend = tube.GetComponent<Renderer>();
            rend.material = lineMaterial;
        }
    }

    #endregion

    /// <summary>
    /// Clears and rebuilds the entire graph visualization.
    /// </summary>
    [SerializeField]
    public void RebuildGraph()
    {
        // Clear existing visualization
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        computedPositions.Clear();

        // Rebuild the graph
        GraphBuilder builder = GetComponent<GraphBuilder>();
        if (builder.useLocal)
        {
            graph = builder.BuildGraphFromJson();
            GetComponent<ForceDirectedGraphManager>().InitializeGraph(graph);
        }
        else
        {
            StartCoroutine(builder.BuildGraphFromJsonAsync(graph =>
            {
                this.graph = graph;
                GetComponent<ForceDirectedGraphManager>().InitializeGraph(graph);
            }));
        }
    }
}
