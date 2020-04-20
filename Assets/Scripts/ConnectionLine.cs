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

    public void UpdateNodePosition(int nodeIndex, Vector3 pos)
    {
        if (Node1Index == nodeIndex)
        {
            SetConnectedNodes(Node1Index, pos, Node2Index, lineRenderer.GetPosition(lineRenderer.positionCount - 1));
        }else if (Node2Index == nodeIndex)
        {
            SetConnectedNodes(Node1Index, lineRenderer.GetPosition(0), Node2Index, pos);
        }
    }
    
    public void SetPositions(Vector2 pos1, Vector2 pos2, Color color1, Color color2)
    {
        lineRenderer.positionCount = 4;
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, Vector3.Lerp(pos1, pos2, 0.4f));
        lineRenderer.SetPosition(2, Vector3.Lerp(pos1, pos2, 0.6f));
        lineRenderer.SetPosition(3, pos2);

        SetGradient(color1, color2);
    }

    private void SetConnectedNodes(int node1Index, Vector2 node1Position, int node2Index, Vector2 node2Position)
    {
        Node1Index = node1Index;
        Node2Index = node2Index;

        //To make collider smaller and stay in the middle of two circles.
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, Vector3.Lerp(node1Position, node2Position, 0.35f));
        lineRenderer.SetPosition(1, Vector3.Lerp(node1Position, node2Position, 0.65f));

        Destroy(meshCollider.sharedMesh);
        var width = lineRenderer.startWidth;
        //To make collider area bigger than visual, we increase the width and decrease back again after baking the mesh for collider.
        lineRenderer.startWidth = width * 4f;
        lineRenderer.endWidth = width * 4f;
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, false);
        meshCollider.sharedMesh = mesh;
        meshCollider.enabled = true;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        //Real setup after baking collider mesh
        lineRenderer.positionCount = 4;
        lineRenderer.SetPosition(0, node1Position);
        lineRenderer.SetPosition(1, Vector3.Lerp(node1Position, node2Position, 0.4f));
        lineRenderer.SetPosition(2, Vector3.Lerp(node1Position, node2Position, 0.6f));
        lineRenderer.SetPosition(3, node2Position);
    }


    public void SetConnectedNodes(int node1Index, Vector2 node1Position, int node2Index, Vector2 node2Position, Color node1Color, Color node2Color)
    {
        SetConnectedNodes(node1Index, node1Position, node2Index, node2Position);

        SetGradient(node1Color, node2Color);
    }

    private void SetGradient(Color color1, Color color2)
    {
        var gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        var colorKey = new GradientColorKey[4];
        colorKey[0].color = color1;
        colorKey[0].time = 0.0f;
        colorKey[1].color = color1;
        colorKey[1].time = 0.49f;
        colorKey[2].color = color2;
        colorKey[2].time = 0.51f;
        colorKey[3].color = color2;
        colorKey[3].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        var alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 0.0f;

        gradient.SetKeys(colorKey, alphaKey);
        lineRenderer.colorGradient = gradient;
    }

}
