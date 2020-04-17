using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GraphEditor : MonoBehaviour
{
    public float Scale = 1;
    public bool ProcessUserInput;
    public Node NodePrefab;
    public ConnectionLine ConnectionLinePrefab;


    public event Action<Graph> OnGraphChange;

    private Graph graph;
    private List<ConnectionLine> connectionLines;

    private int Id;


    private List<Node> nodes;

    // Start is called before the first frame update
    void Start()
    {
        Id = Random.Range(1, 1000);
    }

    public Graph InitGraph()
    {
        nodes = new List<Node>();
        connectionLines = new List<ConnectionLine>();
        graph = new Graph();

        int columnCount = 3;
        int rowCount = 3;
        float width = 1.2f;
        float height = 1.2f;
        for (int i = 0; i < columnCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                var node = Instantiate(NodePrefab,
                    transform.position + new Vector3(-width / 2 + i * width / (columnCount - 1), -height / 2 + j * height / (rowCount - 1)), Quaternion.identity,
                    transform);
                node.Index = (i * columnCount) + j;
               nodes.Add(node);
               node.GraphId = graph.Id;
               node.ColorId = (short)Random.Range(1, StaticValues.ColorByIndex.Length);
               Graph.SetNodeColor(graph, nodes.Count - 1, node.ColorId);
            }
        }
        SetNodeScale(Scale);
        graph.OnGraphChange += Graph_OnGraphChange;

        InputController.instance.OnMouseDown += OnMouseDown;
        InputController.instance.OnMouseMove += OnMouseMove;
        InputController.instance.OnMouseUp += OnMouseUp;

        return graph;
    }

    public void SetNodeScale(float scale)
    {
        foreach (var node in nodes)
        {
            node.Scale = scale;
        }
    }

    private void Graph_OnGraphChange()
    {
        OnGraphChange?.Invoke(graph);
    }

    //When mouse down or touch on a Node, we will set this to that Node so that we can draw a line from the Node to latest mouse/touch position.
    private Node touchStartNode;
    private ConnectionLine currentLine;

    private void OnMouseDown(Vector2 touchPos, Transform hitTransform)
    {
        if (!ProcessUserInput)
            return;
        if (hitTransform && hitTransform.tag == "Node" && hitTransform.GetComponent<Node>().GraphId == graph.Id)
        {
            touchStartNode = hitTransform.GetComponent<Node>();
            currentLine = Instantiate(ConnectionLinePrefab, new Vector2(0, 0), Quaternion.identity, transform);
            currentLine.Scale = Scale;
            currentLine.GraphId = graph.Id;
            currentLine.SetPositions(touchStartNode.transform.position, touchPos);
        }
    }

    private void OnMouseMove(Vector2 touchPos, Transform hitTransform)
    {
        if (!ProcessUserInput)
            return;
        if (touchStartNode && currentLine)
        {
            currentLine.SetPositions(touchStartNode.transform.position, touchPos);

            if (hitTransform)
            {
                if (hitTransform.tag == "Node")
                {
                    var node2 = hitTransform.GetComponent<Node>();
                    if (touchStartNode.Id != node2.Id && node2.GraphId == graph.Id)
                    {
                        if (!Graph.IsNodesConnectedMatrix(graph, touchStartNode.Index, node2.Index))
                        {
                            currentLine.SetPositions(touchStartNode.transform.position, hitTransform.position);
                        }
                    }
                }
            }
        }
    }

    private void OnMouseUp(Vector2 touchPos, Transform hitTransform)
    {
        if (!ProcessUserInput)
            return;
        if (hitTransform)
        {
            if (hitTransform.tag == "Line" && !touchStartNode)
            {
                var line = hitTransform.GetComponent<ConnectionLine>();
                if (line.GraphId == graph.Id)
                {
                    Graph.RemoveConnectionMatrix(graph, line.Node1.Index, line.Node2.Index);
                    connectionLines.Remove(line);
                    Destroy(line.gameObject);
                }
            }
            else if (hitTransform.tag == "Node" && hitTransform.GetComponent<Node>().GraphId == graph.Id)
            {
                var node2 = hitTransform.GetComponent<Node>();
                if (touchStartNode && touchStartNode.Id != node2.Id && !Graph.IsNodesConnectedMatrix(graph, touchStartNode.Index, node2.Index))
                {
                    var node1 = touchStartNode;
                    currentLine.SetConnectedNodes(node1, node2);
                    currentLine.SetPositions(touchStartNode.transform.position, hitTransform.position);
                    connectionLines.Add(currentLine);
                    currentLine = null;
                    Graph.AddConnectionMatrix(graph, node1.Index, node2.Index);

                }
            }
        }
        touchStartNode = null;
        if (currentLine)
            Destroy(currentLine.gameObject); //TODO: instead, hide it to use later
    }
}
