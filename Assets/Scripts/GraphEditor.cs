using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphEditor : MonoBehaviour
{
    public float Scale = 1;
    public bool ProcessUserInput;
    public GameObject NodePrefab;
    public ConnectionLine ConnectionLinePrefab;


    private Graph graph;
    private List<ConnectionLine> connectionLines;

    private int Id;

    // Start is called before the first frame update
    void Start()
    {
        Id = Random.Range(1, 1000);
        connectionLines = new List<ConnectionLine>();

        graph = new Graph();
        Graph.AddNode(graph, Instantiate(NodePrefab, transform.position + new Vector3(-0.5f, 0.5f), Quaternion.identity, transform).GetComponent<Node>());
        Graph.AddNode(graph, Instantiate(NodePrefab, transform.position + new Vector3(0.5f, 0.5f), Quaternion.identity, transform).GetComponent<Node>());
        Graph.AddNode(graph, Instantiate(NodePrefab, transform.position + new Vector3(-0.5f, -0.5f), Quaternion.identity, transform).GetComponent<Node>());
        Graph.AddNode(graph, Instantiate(NodePrefab, transform.position + new Vector3(0.5f, -0.5f), Quaternion.identity, transform).GetComponent<Node>());
        Graph.SetNodeScale(graph, Scale);

        InputController.instance.OnMouseDown += OnMouseDown;
        InputController.instance.OnMouseMove += OnMouseMove;
        InputController.instance.OnMouseUp += OnMouseUp;
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
                        if (!Node.GetConnections(touchStartNode).Exists(n => n.Id == node2.Id))
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
                if (touchStartNode && touchStartNode.Id != node2.Id && !Node.GetConnections(touchStartNode).Exists(n => n.Id == node2.Id))
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
