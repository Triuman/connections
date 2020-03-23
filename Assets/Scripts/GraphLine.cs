using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphLine : MonoBehaviour
{
    private LineRenderer LineRenderer;
    void Start()
    {
        LineRenderer = GetComponent<LineRenderer>();
        LineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        LineRenderer.SetPosition(1, new Vector3(0, 0, 0));
    }

    public void SetPoints(Vector3 point1, Vector3 point2)
    {
        LineRenderer.SetPosition(0, point1);
        LineRenderer.SetPosition(1, point2);
    }
}
