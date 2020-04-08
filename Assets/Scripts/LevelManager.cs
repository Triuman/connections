using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GraphEditor graphEditorTarget;
    public GraphEditor graphEditorPlayer;

    public Circle CorrectnessCircle; //This is for development only

    private Graph graphPlayer = null;
    private Graph graphTarget = null;

    // Start is called before the first frame update
    void Start()
    {
        graphTarget = graphEditorTarget.InitGraph();
        graphPlayer = graphEditorPlayer.InitGraph();

        graphEditorTarget.OnGraphChange += GraphEditorTarget_OnGraphChange;
        graphEditorPlayer.OnGraphChange += GraphEditorPlayer_OnGraphChange;
    }

    private void GraphEditorPlayer_OnGraphChange(Graph graphP)
    {
        graphPlayer = graphP;
        if(graphTarget == null)
            return;
        CompareGraphs();
    }

    private void GraphEditorTarget_OnGraphChange(Graph graphT)
    {
        graphTarget = graphT;
        if (graphPlayer == null)
            return;
        CompareGraphs();
    }

    void CompareGraphs()
    {
        CorrectnessCircle.SetColor(Graph.IsTwoGraphSameMatrix(graphPlayer, graphTarget)
            ? new Color(0.2f, 0.8f, 0.2f)
            : new Color(0.8f, 0.2f, 0.2f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
