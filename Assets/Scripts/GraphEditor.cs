using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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

    // Start is called before the first frame update
    void Start()
    {
        Id = Random.Range(1, 1000);
    }

    public Graph InitGraph()
    {
        Graph.IsTwoGraphSameMatrix(new Graph(), new Graph());

        connectionLines = new List<ConnectionLine>();
        graph = new Graph();
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                Graph.AddNode(graph, Instantiate(NodePrefab, transform.position + new Vector3(-0.6f + i * 1.2f / 6, -0.6f + j * 1.2f / 6), Quaternion.identity, transform));
            }
        }
        Graph.SetNodeScale(graph, Scale);
        graph.OnGraphChange += Graph_OnGraphChange;

        InputController.instance.OnMouseDown += OnMouseDown;
        InputController.instance.OnMouseMove += OnMouseMove;
        InputController.instance.OnMouseUp += OnMouseUp;

        return graph;
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
        if (touchStartNode)
        {
            currentLine.SetPositions(touchStartNode.transform.position, touchPos);

            if (hitTransform)
            {
                if (hitTransform.tag == "Node")
                {
                    var node2 = hitTransform.GetComponent<Node>();
                    if (touchStartNode.Id != node2.Id && node2.GraphId == graph.Id)
                    {
                        if (!touchStartNode.Connections.Exists(n => n.Id == node2.Id))
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
                    Graph.RemoveConnection(graph, line.Node1.Id, line.Node2.Id);
                    connectionLines.Remove(line);
                    Destroy(line.gameObject);
                }
            }
            else if (hitTransform.tag == "Node" && hitTransform.GetComponent<Node>().GraphId == graph.Id)
            {
                var node2 = hitTransform.GetComponent<Node>();
                if (touchStartNode && touchStartNode.Id != node2.Id && !touchStartNode.Connections.Exists(n => n.Id == node2.Id))
                {
                    var node1 = touchStartNode;
                    currentLine.SetConnectedNodes(node1, node2);
                    currentLine.SetPositions(touchStartNode.transform.position, hitTransform.position);
                    connectionLines.Add(currentLine);
                    currentLine = null;
                    Graph.AddConnection(graph, node1.Id, node2.Id);
                    
                }
            }
        }
        touchStartNode = null;
        if (currentLine)
            Destroy(currentLine.gameObject); //TODO: instead, hide it to use later
    }
}
