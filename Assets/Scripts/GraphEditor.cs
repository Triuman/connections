using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphEditor : MonoBehaviour
{
    public GameObject NodePrefab;
    public ConnectionLine ConnectionLinePrefab;

    private Graph graph;

    private int Id;

    // Start is called before the first frame update
    void Start()
    {
        Id = Random.Range(1, 1000);

        graph = new Graph();
        Graph.AddNode(graph, Instantiate(NodePrefab, new Vector2(0, 1), Quaternion.identity, transform).GetComponent<Node>());
        Graph.AddNode(graph, Instantiate(NodePrefab, new Vector2(1, 0), Quaternion.identity, transform).GetComponent<Node>());
        Graph.AddNode(graph, Instantiate(NodePrefab, new Vector2(0, 0), Quaternion.identity, transform).GetComponent<Node>());

        InputController.instance.OnMouseDown += OnMouseDown;
        InputController.instance.OnMouseMove += OnMouseMove;
        InputController.instance.OnMouseUp += OnMouseUp;
    }

    //When mouse down or touch on a Node, we will set this to that Node so that we can draw a line from the Node to latest mouse/touch position.
    private Node touchStartNode;
    private ConnectionLine currentLine;

    private void OnMouseDown(Vector2 touchPos, Transform hitTransform)
    {
        if (hitTransform && hitTransform.tag == "Node")
        {
            touchStartNode = hitTransform.GetComponent<Node>();
            currentLine = Instantiate(ConnectionLinePrefab, new Vector2(0, 0), Quaternion.identity, transform);
            currentLine.SetPositions(touchStartNode.transform.position, touchPos);
        }
    }

    private void OnMouseMove(Vector2 touchPos, Transform hitTransform)
    {
        if (touchStartNode)
        {
            currentLine.SetPositions(touchStartNode.transform.position, touchPos);

            if (hitTransform)
            {
                if (hitTransform.tag == "Node")
                {
                    var node2 = hitTransform.GetComponent<Node>();
                    if (touchStartNode.Id != node2.Id)
                    {
                        if (!Node.GetConnections(touchStartNode)
                            .Exists(n => n.Id == hitTransform.GetComponent<Node>().Id))
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
        if (hitTransform)
        {
            if (hitTransform.tag == "Line")
            {
                var line = hitTransform.GetComponent<ConnectionLine>();
                Graph.RemoveConnection(graph, line.Node1.Id, line.Node2.Id);
                Destroy(line.gameObject);
            }
            else if (hitTransform.tag == "Node")
            {
                var node2 = hitTransform.GetComponent<Node>();
                if (touchStartNode && touchStartNode.Id != node2.Id)
                {
                    var node1 = touchStartNode;
                    currentLine.ConnectNodes(node1, node2);
                    currentLine.SetPositions(touchStartNode.transform.position, hitTransform.position);
                    currentLine = null; //TODO: instead of removing the reference, put it into a list
                    Graph.AddConnection(graph, node1.Id, node2.Id);
                }
            }
        }
        touchStartNode = null;
        if (currentLine)
            Destroy(currentLine.gameObject); //TODO: instead, hide it to use later
    }
}
