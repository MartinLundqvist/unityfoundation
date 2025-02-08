using UnityEngine;
using System.Collections.Generic;
public class GraphNode
{
    // A unique identifier for the node.
    public string id;

    // Type ("root", "sensor", "asset", "compute")
    public string nodeType;

    // Name of the node
    public string name;

    // Reference to the parent node (null if this is a root node).
    public GraphNode parent;

    // List of child nodes.
    public List<GraphNode> children;

    // (Optional) Additional metadata can be added here.
    public Dictionary<string, string> attributes;  // Store attributes as name-value pairs

    public GraphNode(string id, string name, string nodeType)
    {
        this.id = id;
        this.nodeType = nodeType;
        this.name = name;
        parent = null;
        children = new List<GraphNode>();
        attributes = new Dictionary<string, string>();
    }
}

public class Graph
{
    // Use a dictionary for quick look-up by node id.
    private Dictionary<string, GraphNode> nodes;

    public Graph()
    {
        nodes = new Dictionary<string, GraphNode>();
    }

    // Create and add a new node to the graph.
    // If a parentId is provided, the new node becomes a child of the parent.
    // Returns true if the node was successfully added.
    public bool AddNode(GraphNode node)
    {
        if (nodes.ContainsKey(node.id))
        {
            Debug.LogWarning("AddNode failed: A node with id '" + node.id + "' already exists.");
            return false;
        }

        nodes.Add(node.id, node);

        // If a parent is specified, assign the parent–child relationship.
        if (node.parent != null)
        {
            if (nodes.TryGetValue(node.parent.id, out GraphNode parentNode))
            {
                node.parent = parentNode;
                parentNode.children.Add(node);
            }
            else
            {
                Debug.LogWarning("AddNode warning: Parent with id '" + node.parent.id + "' not found. New node will be a root node.");
            }
        }

        return true;
    }

    // Retrieve a node by its id.
    // Returns null if the node is not found.
    public GraphNode GetNode(string id)
    {
        nodes.TryGetValue(id, out GraphNode node);
        return node;
    }

    // Update the id (or other properties) of a node.
    // Here, we update the node's id while ensuring the new id is unique.
    // Returns true if update succeeds.
    public bool UpdateNode(string oldId, string newId, string newNodeType = null)
    {
        if (!nodes.ContainsKey(oldId))
        {
            Debug.LogWarning("UpdateNode failed: Node with id '" + oldId + "' does not exist.");
            return false;
        }

        if (nodes.ContainsKey(newId))
        {
            Debug.LogWarning("UpdateNode failed: A node with id '" + newId + "' already exists.");
            return false;
        }

        // Get the node, remove it from the dictionary, update the id, then re-add it.
        GraphNode node = nodes[oldId];
        nodes.Remove(oldId);
        node.id = newId;
        if (newNodeType != null)
        {
            node.nodeType = newNodeType;
        }
        nodes.Add(newId, node);

        return true;
    }

    // Delete a node (and optionally its entire subtree).
    // Returns true if deletion succeeds.
    public bool DeleteNode(string id, bool deleteSubtree = true)
    {
        if (!nodes.TryGetValue(id, out GraphNode node))
        {
            Debug.LogWarning("DeleteNode failed: Node with id '" + id + "' not found.");
            return false;
        }

        // If the node has a parent, remove it from the parent's children list.
        if (node.parent != null)
        {
            node.parent.children.Remove(node);
        }

        // Optionally, recursively delete all children.
        if (deleteSubtree)
        {
            DeleteSubtree(node);
        }
        else
        {
            // If not deleting the subtree, reassign children as root nodes.
            foreach (GraphNode child in node.children)
            {
                child.parent = null;
            }
            nodes.Remove(id);
        }

        return true;
    }

    // Helper method to recursively remove a node and all its descendants.
    private void DeleteSubtree(GraphNode node)
    {
        // Copy list since we'll modify the original while iterating.
        List<GraphNode> childrenCopy = new List<GraphNode>(node.children);
        foreach (GraphNode child in childrenCopy)
        {
            DeleteSubtree(child);
        }

        nodes.Remove(node.id);
    }

    // Returns a list of all nodes in the graph.
    public List<GraphNode> GetAllNodes()
    {
        return new List<GraphNode>(nodes.Values);
    }
}