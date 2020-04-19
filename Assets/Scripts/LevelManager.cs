using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        if (graphTarget == null)
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
        if (isSame)
            LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        LoadLevel(++currentLevelIndex);
    }

    public void LoadLevel(int levelNo)
    {
        if (levelNo > LevelInfo.Levels.Length)
            levelNo = LevelInfo.Levels.Length;
        else if (levelNo < 1)
            levelNo = 1;
        var level = GetLevel(levelNo); // LevelInfo.Levels[levelIndex];

        graphTarget = graphEditorTarget.InitGraph(level.TargetGraphNodeColorIds, level.TargetGraphConnections);
        graphPlayer = graphEditorPlayer.InitGraph(level.PlayerGraphNodeColorIds, level.PlayerGraphConnections);
    }


    private Level GetLevel(int levelNo)
    {
        //TODO: All Random numbers will be coming from a procedural function that takes levelNo and returns a value.

        const int maxNodeCount = 6;
        const int minNodeCount = 2;

        //Target Graph Setup
        float targetNodeCountScale = Random.Range(0f, 1f);
        float targetColorCountScale = Random.Range(0f, 1f);
        float targetConnectionCountScale = Random.Range(0f, 1f);

        int targetNodeCount = Mathf.CeilToInt(targetNodeCountScale * (maxNodeCount - minNodeCount)) + minNodeCount;
        int targetColorCount = Mathf.CeilToInt((Mathf.Min(targetNodeCount, StaticValues.ColorByIndex.Length) - 1) * targetColorCountScale) + 1;
        int targetConnectionCount = Mathf.CeilToInt((StaticValues.MaxConnectionCountByNodeCount[targetNodeCount] - 1) * targetConnectionCountScale) + 1;


        //Player Graph Setup
        float playerNodeCountScale = Random.Range(0f, 1f);
        float playerColorCountScale = Random.Range(0f, 1f);
        float playerConnectionCountScale = Random.Range(0f, 1f);

        int playerNodeCount = Mathf.CeilToInt(playerNodeCountScale * (maxNodeCount - targetNodeCount)) + targetNodeCount;


        var level = new Level();

        //Create Target Node Array
        level.TargetGraphNodeColorIds = new int[targetNodeCount];

        //Init Target Node Array with ColorIds
        for (int i = 0; i < targetNodeCount; i++)
        {
            if (i < targetColorCount)
                level.TargetGraphNodeColorIds[i] = i + 1;
            else
                level.TargetGraphNodeColorIds[i] = Random.Range(1, targetColorCount + 1);
        }

        //Shuffle Target Node Array
        level.TargetGraphNodeColorIds.Shuffle();

        //Create Connections for Target Node Array
        level.TargetGraphConnections = new int[targetConnectionCount * 2];
        var connectionDic = new Dictionary<int, List<int>>(); //Just to see if we connected same pairs before
        for (int cn = 0; cn < targetConnectionCount; cn++)
        {
            var index1 = cn * 2;
            var index2 = index1 + 1;
            while (true)
            {
                var node1 = Random.Range(0, targetNodeCount);
                var node2 = Random.Range(0, targetNodeCount);
                if (node1 == node2)
                    continue;
                if (connectionDic.ContainsKey(node1) && connectionDic.ContainsKey(node2))
                {
                    if (connectionDic[node1].Exists(c => c == node2) || connectionDic[node2].Exists(c => c == node1))
                    {
                        continue;
                    }
                }

                if (!connectionDic.ContainsKey(node1))
                {
                    connectionDic[node1] = new List<int>(targetConnectionCount) {node2};
                }
                if (!connectionDic.ContainsKey(node2))
                {
                    connectionDic[node2] = new List<int>(targetConnectionCount) {node1};
                }

                level.TargetGraphConnections[index1] = node1;
                level.TargetGraphConnections[index2] = node2;
                break;
            }
        }


        //TODO: Remove the rest
        level.PlayerGraphNodeColorIds = level.TargetGraphNodeColorIds.Clone() as int[];
        level.PlayerGraphNodeColorIds.Shuffle();
        level.PlayerGraphConnections = new int[0];

        return level;
    }


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
/// 
[System.Serializable]
public class Level
{
    public int[] TargetGraphNodeColorIds;
    public int[] TargetGraphConnections;
    public int[] PlayerGraphNodeColorIds;
    public int[] PlayerGraphConnections;
}



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
                0,2,
                0,3,
                1,2,
            },
            PlayerGraphNodeColorIds = new []
            {
                1,2,3,4,2
            },
            PlayerGraphConnections = new int[0],
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
                0,2,
                1,0,
                1,3,
                3,2,
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
            PlayerGraphConnections = new int[0]
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
                0,2,
                2,3,
                4,0,
                1,0,
            },
            PlayerGraphNodeColorIds = new []
            {
                3,
                4,
                2,
                2,
                1
            },
            PlayerGraphConnections = new int[0]
        },
    };
}