using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GraphEditor graphEditorTarget;
    public GraphEditor graphEditorPlayer;
    
    private Graph graphPlayer = null;
    private Graph graphTarget = null;

    private int currentLevelIndex;

    // Start is called before the first frame update
    void Start()
    {
        currentLevelIndex = -1;
        graphEditorTarget.OnGraphChange += GraphEditorTarget_OnGraphChange;
        graphEditorPlayer.OnGraphChange += GraphEditorPlayer_OnGraphChange;
        LoadNextLevel();
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
        var isSame = Graph.IsTwoGraphSameMatrix(graphPlayer, graphTarget);
        if(isSame)
            LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        LoadLevel(++currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= LevelInfo.Levels.Length)
            levelIndex = LevelInfo.Levels.Length - 1;
        else if (levelIndex < 0)
            levelIndex = 0;
        var level = LevelInfo.Levels[levelIndex];

        graphTarget = graphEditorTarget.InitGraph(level.TargetGraphNodeColorIds, level.TargetGraphConnections);
        graphPlayer = graphEditorPlayer.InitGraph(level.PlayerGraphNodeColorIds, level.PlayerGraphConnections);
    }
}

[System.Serializable]
public class Level
{
    public int[] TargetGraphNodeColorIds;
    public Tuple<int, int>[] TargetGraphConnections;
    public int[] PlayerGraphNodeColorIds;
    public Tuple<int, int>[] PlayerGraphConnections;
}

/// <summary>
///
///     Level Parameters
///         - Target Node Count Scale 0-1 (min 2, max 6)
///         - Target Color Count Scale 0-1 (min 1, max Node Count)
///         - Target Connection Count Scale 0-1 (min 1, max Node Count factorial)
/// 
///         - Player Node Count Scale 0-1 (min Target Connected Node Count, max 6)
///         - Player Color Count Scale 0-1 (min Target Connected Different Node Color Count, max Node Count)
///         - Player Connection Count Scale 0-1 (min 1, max Node Count factorial)
///
/// 
/// </summary>



public static class LevelInfo
{
    public static Level[] Levels = new[]
    {
        new Level()
        {
            TargetGraphNodeColorIds = new []
            {
                1,2,3,4
            },
            TargetGraphConnections = new []
            {
                new Tuple<int, int>(0,2),
                new Tuple<int, int>(0,3),
                new Tuple<int, int>(1,2),

            },
            PlayerGraphNodeColorIds = new []
            {
                1,2,3,4,2
            },
            PlayerGraphConnections = new []
            {
                new Tuple<int, int>(0,2),
                new Tuple<int, int>(0,3),
                new Tuple<int, int>(1,2),

            },
        },
        new Level()
        {
            TargetGraphNodeColorIds = new []
            {
                1,
                2,
                4,
                4
            },
            TargetGraphConnections = new []
            {
                new Tuple<int, int>(0,2),
                new Tuple<int, int>(1,0),
                new Tuple<int, int>(1,3),
                new Tuple<int, int>(3,2),
            },
            PlayerGraphNodeColorIds = new []
            {
                1,
                4,
                2,
                1,
                4,
                2
            },
            PlayerGraphConnections = new Tuple<int, int>[0]
        },
        new Level()
        {
            TargetGraphNodeColorIds = new []
            {
                2,
                1,
                2,
                3,
                4,
                4,
                4,
                4,
                4,
                4,
                4,
            },
            TargetGraphConnections = new []
            {
                new Tuple<int, int>(0,2),
                new Tuple<int, int>(2,3),
                new Tuple<int, int>(4,0),
                new Tuple<int, int>(1,0),
            },
            PlayerGraphNodeColorIds = new []
            {
                3,
                4,
                2,
                2,
                1
            },
            PlayerGraphConnections = new Tuple<int, int>[0]
        },
    };
}