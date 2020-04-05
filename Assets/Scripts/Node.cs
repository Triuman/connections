using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Node : MonoBehaviour
{
    public int Id { get; private set; }
    public int Number { get; private set; }
    private List<Node> Connections { get; set; }

    void Start()
    {
        SetRandomId(this);
        SetNumber(this, 0);
        Connections = new List<Node>();
    }


    public static Color GetColor(Node node) => StaticValues.ColorByIndex[node.Number];
    public static void SetNumber(Node node, int number) => node.Number = number;
    /// <summary>
    /// In case there is a node with same Id in a graph, we can call this function from outside and add the node safely.
    /// </summary>
    public static void SetRandomId(Node node) => node.Id = Random.Range(1, 10000000);
    public static void SetId(Node node, int id) => node.Id = id;
    public static List<Node> SetConnections(Node node, List<Node> connections) => node.Connections = connections.ToList();
    public static List<Node> GetConnections(Node node) => node?.Connections?.ToList();
    public static void ConnectNodes(Node node1, Node node2) => ConnectNodes(new List<Node>() { node1, node2 });
    public static void ConnectNodes(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (nodes[i].Connections.All(n => n.Id == nodes[j].Id))
                    nodes[i].Connections.Add(nodes[j]);
                if (nodes[j].Connections.All(n => n.Id == nodes[i].Id))
                    nodes[j].Connections.Add(nodes[i]);
            }
        }
    }
    public static void UnConnectNodes(Node node1, Node node2) => UnConnectNodes(new List<Node>() { node1, node2 });
    public static void UnConnectNodes(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (nodes[i].Connections.Any(n => n.Id == nodes[j].Id))
                    nodes[i].Connections.Remove(nodes[j]);
                if (nodes[j].Connections.Any(n => n.Id == nodes[i].Id))
                    nodes[j].Connections.Remove(nodes[i]);
            }
        }
    }
}

