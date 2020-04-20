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

    private void OnDestroy()
    {
        DestroyObjects();
        InputController.instance.OnMouseDown -= OnMouseDown;
        InputController.instance.OnMouseMove -= OnMouseMove;
        InputController.instance.OnMouseUp -= OnMouseUp;
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

    public Graph InitGraph(int[] nodeColorIds, int[] connections)
    {
        DestroyObjects();

        nodes = new List<Node>();
        connectionLines = new List<ConnectionLine>();
        graph = new Graph(nodeColorIds, connections);

        //To mix things more
        var startAngle = Random.value * Mathf.PI * 2;

        int matrixSize = nodeColorIds.Length;
        var angle = Mathf.PI * 2 / matrixSize;
        var radius = matrixSize / 15f + 0.2f;
        for (int i = 0; i < matrixSize; i++)
        {
            var node = Instantiate(NodePrefab, transform.position + new Vector3(Mathf.Cos(angle * i + startAngle) * radius, Mathf.Sin(angle * i + startAngle) * radius) * Scale, Quaternion.identity, transform);
            node.Index = i;
            node.GraphId = graph.Id;
            node.ColorId = nodeColorIds[i];
            nodes.Add(node);
        }

        for (int c = 0; c < connections.Length; c+=2)
        {
            var connectionNode1 = connections[c];
            var connectionNode2 = connections[c + 1];
            if(connectionNode1 == connectionNode2)
                continue;
            if (connectionLines.Exists(cn => cn.Node1Index == connectionNode1 && cn.Node2Index == connectionNode2 || cn.Node1Index == connectionNode2 && cn.Node2Index == connectionNode1))
                continue;

            //TODO: make line's color gradient between node colors.
            var newLine = Instantiate(ConnectionLinePrefab, new Vector2(0, 0), Quaternion.identity, transform);
            newLine.Scale = Scale;
            newLine.GraphId = graph.Id;
            newLine.SetConnectedNodes(connectionNode1, nodes[connectionNode1].transform.position, connectionNode2, nodes[connectionNode2].transform.position, StaticValues.ColorByIndex[nodeColorIds[connectionNode1] - 1], StaticValues.ColorByIndex[nodeColorIds[connectionNode2] - 1]);
            connectionLines.Add(newLine);
            nodes[connectionNode1].lines.Add(newLine);
            nodes[connectionNode2].lines.Add(newLine);
        }
        SetNodeScale(Scale / (matrixSize / 80f + 0.6f));
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SwapNodes();
        }
    }

    void SwapNodes()
    {
        var node1Index = Random.Range(0, nodes.Count);
        var node2Index = Random.Range(0, nodes.Count);

        if (node1Index == node2Index)
        {
            SwapNodes();
            return;
        }

        var node1Pos = nodes[node1Index].Position;
        var node2Pos = nodes[node2Index].Position;
        nodes[node1Index].MoveTo(node2Pos, 0.2f);
        nodes[node2Index].MoveTo(node1Pos, 0.2f);
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
            currentLine.SetPositions(touchStartNode.transform.position, touchPos, StaticValues.ColorByIndex[touchStartNode.ColorId-1], StaticValues.ColorByIndex[touchStartNode.ColorId - 1]);
        }
    }

    private void OnMouseMove(Vector2 touchPos, Transform hitTransform)
    {
        if (!ProcessUserInput)
            return;
        if (touchStartNode && currentLine)
        {
            currentLine.SetPositions(touchStartNode.transform.position, touchPos, StaticValues.ColorByIndex[touchStartNode.ColorId - 1], StaticValues.ColorByIndex[touchStartNode.ColorId - 1]);

            if (hitTransform)
            {
                if (hitTransform.tag == "Node")
                {
                    var node2 = hitTransform.GetComponent<Node>();
                    if (touchStartNode.Id != node2.Id && node2.GraphId == graph.Id)
                    {
                        if (!Graph.IsNodesConnectedMatrix(graph, touchStartNode.Index, node2.Index))
                        {
                            currentLine.SetPositions(touchStartNode.transform.position, hitTransform.position, StaticValues.ColorByIndex[touchStartNode.ColorId - 1], StaticValues.ColorByIndex[node2.ColorId - 1]);
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
                    nodes[line.Node1Index].lines.Remove(line);
                    nodes[line.Node2Index].lines.Remove(line);
                    Destroy(line.gameObject);
                }
            }
            else if (hitTransform.tag == "Node" && hitTransform.GetComponent<Node>().GraphId == graph.Id)
            {
                var node2 = hitTransform.GetComponent<Node>();
                if (touchStartNode && touchStartNode.Id != node2.Id && !Graph.IsNodesConnectedMatrix(graph, touchStartNode.Index, node2.Index))
                {
                    var node1 = touchStartNode;
                    currentLine.SetConnectedNodes(node1.Index, node1.transform.position, node2.Index, node2.transform.position, StaticValues.ColorByIndex[node1.ColorId - 1], StaticValues.ColorByIndex[node2.ColorId - 1]);
                    connectionLines.Add(currentLine);
                    node1.lines.Add(currentLine);
                    node2.lines.Add(currentLine);
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
