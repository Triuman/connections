using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Graph
{
    public int Id { get; private set; }
    public event Action OnGraphChange;
    private readonly List<Node> nodes;

    public Graph()
    {
        nodes = new List<Node>();
        SetRandomId(this);
    }
    public static void SetRandomId(Graph graph) => graph.Id = Random.Range(1, 10000000);
    public static void SetId(Graph graph, int id) => graph.Id = id;

    public static void SetNodeScale(Graph graph, float scale)
    {
        foreach (var node in graph.nodes)
        {
            node.Scale = scale;
        }
    }

    public static void AddNode(Graph graph, Node node)
    {
        node.GraphId = graph.Id;
        graph.nodes.Add(node);
        graph.OnGraphChange?.Invoke();
    }

    public static void AddConnection(Graph graph, int nodeId1, int nodeId2)
    {
        var node1 = graph.nodes.FirstOrDefault(n => n.Id == nodeId1);
        var node2 = graph.nodes.FirstOrDefault(n => n.Id == nodeId2);
        Node.ConnectNodes(node1, node2);
        graph.OnGraphChange?.Invoke();
    }
    public static void AddConnection(Graph graph, List<int> nodeIds)
    {
        var nodeList = new List<Node>();
        foreach (var nodeId in nodeIds)
        {
            var node = graph.nodes.FirstOrDefault(n => n.Id == nodeId);
            if(node == null)
                throw new NullReferenceException("Node with id " + nodeId + " not found in Graph with id " + graph.Id);
            nodeList.Add(node);
        }
        Node.ConnectNodes(nodeList);
        graph.OnGraphChange?.Invoke();
    }

    public static void RemoveConnection(Graph graph, int nodeId1, int nodeId2)
    {
        var node1 = graph.nodes.FirstOrDefault(n => n.Id == nodeId1);
        var node2 = graph.nodes.FirstOrDefault(n => n.Id == nodeId2);
        Node.UnConnectNodes(node1, node2);
        graph.OnGraphChange?.Invoke();
    }
    internal static void RemoveConnection(Graph graph, List<int> nodeIds)
    {
        var nodeList = new List<Node>();
        foreach (var nodeId in nodeIds)
        {
            var node = graph.nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
                throw new NullReferenceException("Node with id " + nodeId + " not found in Graph with id " + graph.Id);
            nodeList.Remove(node);
        }
        Node.ConnectNodes(nodeList);
        graph.OnGraphChange?.Invoke();
    }
}


public static class StaticValues
{
    public static Color[] ColorByIndex = new Color[]
    {
        new Color(0,0,0,1),
        new Color(1,0,0,1), 
        new Color(0,1,0,1), 
        new Color(0,0,1,1), 
        new Color(1,1,0,1), 
        new Color(1,0,1,1), 
        new Color(0,1,1,1), 
    };

}
