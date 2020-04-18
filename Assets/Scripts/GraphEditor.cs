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
        InputController.instance.OnMouseDown += OnMouseDown;
        InputController.instance.OnMouseMove += OnMouseMove;
        InputController.instance.OnMouseUp += OnMouseUp;
    }

    public Graph InitGraph()
    {
        DestroyObjects();

        nodes = new List<Node>();
        connectionLines = new List<ConnectionLine>();
        graph = new Graph(null);

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
                node.Index = i * columnCount + j;
                nodes.Add(node);
                node.GraphId = graph.Id;
                node.ColorId = Random.Range(1, StaticValues.ColorByIndex.Length);
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

    public Graph InitGraph(int[] nodeColorIds, Tuple<int, int>[] connections)
    {
        DestroyObjects();

        nodes = new List<Node>();
        connectionLines = new List<ConnectionLine>();
        graph = new Graph(nodeColorIds, connections);

        int matrixSize = nodeColorIds.Length;
        var angle = Mathf.PI * 2 / matrixSize;
        var radius = matrixSize / 15f + 0.2f;
        for (int i = 0; i < matrixSize; i++)
        {
            var node = Instantiate(NodePrefab, transform.position + new Vector3(Mathf.Cos(angle * i + Mathf.PI) * radius, Mathf.Sin(angle * i + Mathf.PI) * radius) * Scale, Quaternion.identity, transform);
            node.Index = i;
            node.GraphId = graph.Id;
            node.ColorId = nodeColorIds[i];
            nodes.Add(node);
        }

        for (int c = 0; c < connections.Length; c++)
        {
            var connection = connections[c];
            if(connection.Item1 == connection.Item2)
                continue;
            if (connectionLines.Exists(cn => cn.Node1Index == connection.Item1 && cn.Node2Index == connection.Item2 || cn.Node1Index == connection.Item2 && cn.Node2Index == connection.Item1))
                continue;

            //TODO: make line's color gradient between node colors.
            var newLine = Instantiate(ConnectionLinePrefab, new Vector2(0, 0), Quaternion.identity, transform);
            newLine.Scale = Scale;
            newLine.GraphId = graph.Id;
            newLine.SetConnectedNodes(connection.Item1, nodes[connection.Item1].transform.position, connection.Item2, nodes[connection.Item2].transform.position);
            connectionLines.Add(newLine);

        }
        SetNodeScale(Scale);
        graph.OnGraphChange += Graph_OnGraphChange;

        return graph;
    }

    private void DestroyObjects()
    {

        if (nodes != null && nodes.Count > 0)
        {
            for (int n = 0; n < nodes.Count; n++)
            {
                //TODO: use object pooling
                Destroy(nodes[n].gameObject);
            }
            nodes.Clear();
        }
        if (connectionLines != null && connectionLines.Count > 0)
        {
            for (int c = 0; c < connectionLines.Count; c++)
            {
                //TODO: use object pooling
                Destroy(connectionLines[c].gameObject);
            }
            connectionLines.Clear();
        }
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
                    Graph.RemoveConnectionMatrix(graph, line.Node1Index, line.Node2Index);
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
                    currentLine.SetConnectedNodes(node1.Index, node1.transform.position, node2.Index, node2.transform.position);
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
