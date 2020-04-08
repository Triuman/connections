using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Node : MonoBehaviour
{
    public Circle Circle;
    public int GraphId;
    public int Index;
    public int Id { get; private set; }
    [SerializeField]
    private int colorId;
    public int ColorId
    {
        get => colorId;
        set
        {
            colorId = value;
            Circle.SetColor(StaticValues.ColorByIndex[ColorId]);
        }
}

    public float Scale
    {
        set => transform.localScale = new Vector3(value, value, value);
    }

    public List<Node> Connections { get; set; }

    void Start()
    {
        SetRandomId(this);
        ColorId = colorId;
        Connections = new List<Node>();
    }


    /// <summary>
    /// In case there is a node with same Id in a graph, we can call this function from outside and add the node safely.
    /// </summary>
    public static void SetRandomId(Node node) => node.Id = Random.Range(1, 10000000);
    public static void ConnectNodes(Node node1, Node node2) => ConnectNodes(new List<Node>() { node1, node2 });
    public static void ConnectNodes(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (!nodes[i].Connections.Exists(n => n.Id == nodes[j].Id))
                    nodes[i].Connections.Add(nodes[j]);
                if (!nodes[j].Connections.Exists(n => n.Id == nodes[i].Id))
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

