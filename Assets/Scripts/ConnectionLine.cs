using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    public Node Node1;
    public Node Node2;
    

    private LineRenderer lineRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.enabled = false;
    }
    public void SetPositions(Vector2 pos1, Vector2 pos2)
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, pos2);
    }

    public void ConnectNodes(Node node1, Node node2)
    {
        Node1 = node1;
        Node2 = node2;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, node1.transform.position);
        lineRenderer.SetPosition(1, node2.transform.position);

        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshCollider.sharedMesh = mesh;
        meshCollider.enabled = true;
    }

}
