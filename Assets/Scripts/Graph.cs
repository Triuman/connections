using System;
using System.Collections.Generic;

public class Graph
{
    public const int Size = 4;
    readonly List<Point>[,] nodeArray = new List<Point>[Size, Size];

    public event Action OnGraphChange;
    public void AddNode(Point node)
    {
        if (nodeArray[node.x, node.y] != null)
            return;
        nodeArray[node.x, node.y] = new List<Point>();
        OnGraphChange?.Invoke();
    }

    public void AddConnection(Point node1, Point node2)
    {
        var node1Connections = nodeArray[node1.x, node1.y];
        var node2Connections = nodeArray[node2.x, node2.y];
        if (node1Connections == null)
        {
            node1Connections = new List<Point>();
            nodeArray[node1.x, node1.y] = node1Connections;
        }

        if (node2Connections == null)
        {
            node2Connections = new List<Point>();
            nodeArray[node2.x, node2.y] = node2Connections;
        }
        //TODO: 'Contains' method might be checking for the exact object instance. So, make sure it is working well with different Point instances.
        if (!node1Connections.Contains(node2))
            node1Connections.Add(node2);
        if (!node2Connections.Contains(node1))
            node2Connections.Add(node1);

        OnGraphChange?.Invoke();
    }
}

public struct Point
{
    public int x, y;
    public Point(int px, int py)
    {
        x = px;
        y = py;
    }
}

