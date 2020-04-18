using UnityEngine;

public class ConnectionLine : MonoBehaviour
{

    public int GraphId;

    public int Node1Index;
    public int Node2Index;

    public float Scale
    {
        set => lineRenderer.startWidth = value * 0.04f;
    }

    private LineRenderer lineRenderer;
    private MeshCollider meshCollider;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.enabled = false;
    }
    public void SetPositions(Vector2 pos1, Vector2 pos2)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, pos2);
    }

    public void SetConnectedNodes(int node1Index, Vector2 node1Position, int node2Index, Vector2 node2Position)
    {
        Node1Index = node1Index;
        Node2Index = node2Index;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, node1Position);
        lineRenderer.SetPosition(1, node2Position);

        Destroy(meshCollider.sharedMesh);
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, false);
        meshCollider.sharedMesh = mesh;
        meshCollider.enabled = true;
    }

}
