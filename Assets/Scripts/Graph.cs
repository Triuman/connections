using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Graph
{
    public int Id { get; private set; }
    public event Action OnGraphChange;
    private readonly List<Node> nodes;

    private bool[,] graphMatrix;
    private const int GraphMatrixSize = 9; //TODO: make it 25

    public Graph()
    {
        graphMatrix = new bool[GraphMatrixSize, GraphMatrixSize];


        nodes = new List<Node>();
        SetRandomId(this);
    }
    public static void SetRandomId(Graph graph) => graph.Id = Random.Range(1, 10000000);
    public static void SetId(Graph graph, int id) => graph.Id = id;

    public static void SetNodeScale(Graph graph, float scale)
    {
        foreach (var node in graph.nodes)
        {
            node.Scale = scale;
        }
    }

    public static void AddNode(Graph graph, Node node, int colorId = 0)
    {
        node.GraphId = graph.Id;
        graph.nodes.Add(node);
        node.ColorId = colorId;
        graph.OnGraphChange?.Invoke();
    }


    public static void AddConnectionMatrix(Graph graph, int node1Index, int node2Index)
    {
        graph.graphMatrix[node1Index, node2Index] = true;
        graph.graphMatrix[node2Index, node1Index] = true;
        graph.OnGraphChange?.Invoke();
    }
    public static void RemoveConnectionMatrix(Graph graph, int node1Index, int node2Index)
    {
        graph.graphMatrix[node1Index, node2Index] = false;
        graph.graphMatrix[node2Index, node1Index] = false;
        graph.OnGraphChange?.Invoke();
    }
    public static bool IsNodesConnectedMatrix(Graph graph, int node1Index, int node2Index)
    {
        return graph.graphMatrix[node1Index, node2Index] && graph.graphMatrix[node2Index, node1Index];
    }

    internal static bool IsTwoGraphSameMatrix(Graph graph1, Graph graph2)
    {
        var graph1MatrixTemp = graph1.graphMatrix.Clone() as bool[,];
        var graph2MatrixTemp = graph2.graphMatrix.Clone() as bool[,];

        //Process matrix and put every node index into dictionary list by their connection count.
        //So, <5, [0,3,4]> means nodes 0,3 and 4 have 5 connections each.
        //After grouping them by their connection number, we will sort the matrix and permutate them.
        var graph1IndexListPerCountDic = new SortedDictionary<int, List<int>>(); //<count>, <list of indices that has same number of connections>
        for (int i = 0; i < GraphMatrixSize; i++)
        {
            var countOfIndex = 0;
            for (int j = 0; j < GraphMatrixSize; j++)
            {
                if (graph1MatrixTemp[i, j])
                    countOfIndex++;
            }

            if (countOfIndex <= 0)
                continue;

            if (!graph1IndexListPerCountDic.ContainsKey(countOfIndex))
                graph1IndexListPerCountDic[countOfIndex] = new List<int>();
            graph1IndexListPerCountDic[countOfIndex].Add(i);
        }


        var graph2IndexListPerCountDic = new SortedDictionary<int, List<int>>(); //<count>, <list of indices that has same number of connections>
        for (int i = 0; i < GraphMatrixSize; i++)
        {
            var countOfIndex = 0;
            for (int j = 0; j < GraphMatrixSize; j++)
            {
                if (graph2MatrixTemp[i, j])
                    countOfIndex++;
            }

            if (countOfIndex <= 0)
                continue;

            if (!graph2IndexListPerCountDic.ContainsKey(countOfIndex))
                graph2IndexListPerCountDic[countOfIndex] = new List<int>();
            graph2IndexListPerCountDic[countOfIndex].Add(i);
        }

        //Check if every count has same number of indices in each graph
        //eg. graph1 has 3 nodes with 2 connection and 2 nodes with 1 connection. graph2 has 2 nodes with 2 connection and 2 nodes with 1 connection. Then they are definitely not equal.
        foreach (KeyValuePair<int, List<int>> graph1IndexListPerCount in graph1IndexListPerCountDic)
        {
            if (!graph2IndexListPerCountDic.ContainsKey(graph1IndexListPerCount.Key) ||
                graph2IndexListPerCountDic[graph1IndexListPerCount.Key].Count != graph1IndexListPerCount.Value.Count)
                return false;
        }


        //Sort matrices so that they have less connections from lower indeces
        //
        // <not sorted>
        //   0 1 2 3 4 5
        // 0 0 1 0 1 1 1
        // 1 1 0 1 0 1 0
        // 2 0 1 0 0 0 0
        // 3 1 0 0 0 1 0
        // 4 1 1 0 1 0 0
        // 5 1 0 0 0 0 0
        // </not sorted>
        //
        // <sorted>
        //     2 5 3 4 1 0
        //
        // 2   0 0 0 1 1 0
        // 5   0 0 0 1 1 1
        // 3   0 0 0 1 1 0
        // 4   1 1 1 0 0 1
        // 1   1 1 1 0 0 1
        // 0   0 1 0 1 1 0
        // </sorted>
        // 
        //
        // 
        var graph1CountDicKeys = graph1IndexListPerCountDic.Keys.ToArray();
        //for (int k = 0; k < graph1CountDicKeys.Length - 1; k++)
        //{
        //    var indexList = graph1IndexListPerCountDic[graph1CountDicKeys[k]];
        //    for (var i = 0; i < indexList.Count; i++)
        //    {
        //        int index = indexList[i];
        //        for (int p = k + 1; p < graph1CountDicKeys.Length; p++)
        //        {
        //            var indexListControl = graph1IndexListPerCountDic[graph1CountDicKeys[k]];
        //            for (var iControl = 0; iControl < indexListControl.Count; iControl++)
        //            {
        //                int indexControl = indexListControl[iControl];
        //                if (index > indexControl)
        //                {
        //                    MultiArrayHelper<bool>.SwapColumns(graph1MatrixTemp, index, indexControl);
        //                    MultiArrayHelper<bool>.SwapRows(graph1MatrixTemp, index, indexControl);
        //                    indexList[i] = indexControl;
        //                    indexListControl[iControl] = index;
        //                }
        //            }
        //        }
        //    }
        //}

        var graph2CountDicKeys = graph2IndexListPerCountDic.Keys.ToArray();
        var combinationGraph2 = new List<int>();
        foreach (int graph2CountDicKey in graph2CountDicKeys)
        {
            combinationGraph2.AddRange(graph2IndexListPerCountDic[graph2CountDicKey]);
        }

        graph2MatrixTemp = MultiArrayHelper<bool>.OrderArray(graph2MatrixTemp, combinationGraph2);
        //for (int k = 0; k < graph2CountDicKeys.Length - 1; k++)
        //{
        //    var indexList = graph2IndexListPerCountDic[graph2CountDicKeys[k]];
        //    for (var i = 0; i < indexList.Count; i++)
        //    {
        //        int index = indexList[i];
        //        for (int p = k + 1; p < graph2CountDicKeys.Length; p++)
        //        {
        //            var indexListControl = graph2IndexListPerCountDic[graph2CountDicKeys[k]];
        //            for (var iControl = 0; iControl < indexListControl.Count; iControl++)
        //            {
        //                int indexControl = indexListControl[iControl];
        //                if (index > indexControl)
        //                {
        //                    MultiArrayHelper<bool>.SwapColumns(graph2MatrixTemp, index, indexControl);
        //                    MultiArrayHelper<bool>.SwapRows(graph2MatrixTemp, index, indexControl);
        //                    indexList[i] = indexControl;
        //                    indexListControl[iControl] = index;
        //                }
        //            }
        //        }
        //    }
        //}


        //Permutate graphMatrix1 and check if it matches with graphMatrix2
        var graph1IndexPermutations = new List<List<int[]>>();
        for (int k = 0; k < graph1CountDicKeys.Length; k++)
        {
            var indexList = graph1IndexListPerCountDic[graph1CountDicKeys[k]];
            var permutations = ListPermutations(indexList.ToArray());
            graph1IndexPermutations.Add(permutations);
        }

        var combinations = GetCombinationsOfIntLists(graph1IndexPermutations);

        foreach (List<int> combination in combinations)
        {
            var graph1MatrixTempCombination = MultiArrayHelper<bool>.OrderArray(graph1MatrixTemp, combination);
            bool isCombinationCorrect = true;
            for (int i = 0; i < GraphMatrixSize; i++)
            {
                var rowGraph1 = MultiArrayHelper<bool>.GetRow(graph1MatrixTempCombination, i);
                var rowGraph2 = MultiArrayHelper<bool>.GetRow(graph2MatrixTemp, i);
                if (!MultiArrayHelper<bool>.CompareRows(rowGraph1, rowGraph2))
                {
                    isCombinationCorrect = false;
                    break;
                }
            }

            if (isCombinationCorrect)
            {
                return true;
            }
        }

        return false;
    }

    private static List<List<int>> GetCombinationsOfIntLists(List<List<int[]>> intLists)
    {
        var result = new List<List<int>>();

        GetCombinationsOfIntListsRecursive(intLists, 0, new List<int>(), result);

        return result;
    }

    private static void GetCombinationsOfIntListsRecursive(List<List<int[]>> intLists, int intListIndex, List<int> currentList, List<List<int>> result)
    {
        foreach (int[] intList in intLists[intListIndex])
        {
            var newList = currentList.ToList();
            newList.AddRange(intList);
            if (intListIndex + 1 >= intLists.Count)
            {
                result.Add(newList);
                continue;
            }
            GetCombinationsOfIntListsRecursive(intLists, intListIndex + 1, newList.ToList(), result);
        }
    }


    private static List<int[]> ListPermutations(int[] a)
    {
        List<int[]> results = new List<int[]>();
        ListPermutations(a, 0, results);
        return results;
    }

    private static void ListPermutations(int[] a, int start, List<int[]> result)
    {
        if (start >= a.Length)
        {
            result.Add((int[])a.Clone());
        }
        else
        {
            for (int i = start; i < a.Length; i++)
            {
                swap(a, start, i);
                ListPermutations(a, start + 1, result);
                swap(a, start, i);
            }
        }
    }

    private static void swap(int[] a, int i, int j)
    {
        int temp = a[i];
        a[i] = a[j];
        a[j] = temp;
    }


    public static class MultiArrayHelper<T>
    {

        public static T[,] OrderArray(T[,] matrix, List<int> indexOrder)
        {
            var resultMatrix = matrix.Clone() as T[,];
            for (int i = 0; i < indexOrder.Count; i++)
            {
                resultMatrix = SwapColumns(resultMatrix, i, indexOrder[i]);
                resultMatrix = SwapRows(resultMatrix, i, indexOrder[i]);
                for (int j = 0; j < indexOrder.Count; j++)
                {
                    if (indexOrder[j] == i)
                    {
                        indexOrder[j] = indexOrder[i];
                    }
                }
            }
            return resultMatrix;
        }

        public static T[,] SwapColumns(T[,] matrix, int index1, int index2)
        {
            var resultMatrix = matrix.Clone() as T[,];
            var column1 = GetColumn(matrix, index1);
            var column2 = GetColumn(matrix, index2);

            resultMatrix = SetColumn(resultMatrix, index1, column2);
            resultMatrix = SetColumn(resultMatrix, index2, column1);

            return resultMatrix;
        }
        public static T[,] SwapRows(T[,] matrix, int index1, int index2)
        {
            var resultMatrix = matrix.Clone() as T[,];
            var row1 = GetRow(matrix, index1);
            var row2 = GetRow(matrix, index2);

            resultMatrix = SetRow(resultMatrix, index1, row2);
            resultMatrix = SetRow(resultMatrix, index2, row1);

            return resultMatrix;
        }
        public static T[,] SetColumn(T[,] matrix, int columnIndex, T[] column)
        {
            var resultMatrix = matrix.Clone() as T[,];
            for (int r = 0; r < resultMatrix.GetLength(0); r++)
            {
                resultMatrix[r, columnIndex] = column[r];
            }
            return resultMatrix;
        }
        public static T[,] SetRow(T[,] matrix, int rowIndex, T[] row)
        {
            var resultMatrix = matrix.Clone() as T[,];
            for (int c = 0; c < resultMatrix.GetLength(1); c++)
            {
                resultMatrix[rowIndex, c] = row[c];
            }
            return resultMatrix;
        }
        public static T[] GetColumn(T[,] matrix, int columnIndex)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, columnIndex])
                .ToArray();
        }

        public static T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowNumber, x])
                .ToArray();
        }

        public static bool CompareRows(T[] row1, T[] row2)
        {
            if (row1.Length != row2.Length)
                return false;
            for (int i = 0; i < row1.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(row1[i], row2[i]))
                    return false;
            }

            return true;
        }
    }


    public static void AddConnection(Graph graph, int nodeId1, int nodeId2)
    {
        var node1 = graph.nodes.FirstOrDefault(n => n.Id == nodeId1);
        var node2 = graph.nodes.FirstOrDefault(n => n.Id == nodeId2);
        Node.ConnectNodes(node1, node2);
        graph.OnGraphChange?.Invoke();
    }
    public static void AddConnection(Graph graph, List<int> nodeIds)
    {
        var nodeList = new List<Node>();
        foreach (var nodeId in nodeIds)
        {
            var node = graph.nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
                throw new NullReferenceException("Node with id " + nodeId + " not found in Graph with id " + graph.Id);
            nodeList.Add(node);
        }
        Node.ConnectNodes(nodeList);
        graph.OnGraphChange?.Invoke();
    }

    public static void RemoveConnection(Graph graph, int nodeId1, int nodeId2)
    {
        var node1 = graph.nodes.FirstOrDefault(n => n.Id == nodeId1);
        var node2 = graph.nodes.FirstOrDefault(n => n.Id == nodeId2);
        Node.UnConnectNodes(node1, node2);
        graph.OnGraphChange?.Invoke();
    }
    internal static void RemoveConnection(Graph graph, List<int> nodeIds)
    {
        var nodeList = new List<Node>();
        foreach (var nodeId in nodeIds)
        {
            var node = graph.nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
                throw new NullReferenceException("Node with id " + nodeId + " not found in Graph with id " + graph.Id);
            nodeList.Remove(node);
        }
        Node.ConnectNodes(nodeList);
        graph.OnGraphChange?.Invoke();
    }
    internal static bool IsTwoGraphSame(Graph graph1, Graph graph2)
    {
        //Compare the count of Nodes that have at least one connected node 
        if (graph1.nodes.Count(n => n.Connections.Any()) != graph2.nodes.Count(n => n.Connections.Any()))
        {
            return false;
        }

        //Compare connection counts
        if (graph1.nodes.Sum(n => n.Connections.Count) != graph2.nodes.Sum(n => n.Connections.Count))
        {
            return false;
        }

        //Compare connections node by node and color by color
        var graph1ConnectionCountPerNode = new List<NodeConnection>();
        foreach (Node graph1Node in graph1.nodes)
        {
            var connectedNodes = graph1Node.Connections;
            if (connectedNodes.Count == 0)
                continue;
            var nodeConnectionDic = new NodeConnection()
            {
                ColorId = graph1Node.ColorId,
                ConnectionColorCountDic = new Dictionary<int, int>()
            };

            foreach (Node connectedNode in connectedNodes)
            {
                if (!nodeConnectionDic.ConnectionColorCountDic.ContainsKey(connectedNode.ColorId))
                    nodeConnectionDic.ConnectionColorCountDic.Add(connectedNode.ColorId, 0);
                nodeConnectionDic.ConnectionColorCountDic[connectedNode.ColorId]++;
            }
            graph1ConnectionCountPerNode.Add(nodeConnectionDic);
        }
        foreach (Node graph2Node in graph2.nodes)
        {
            var connectedNodes = graph2Node.Connections;
            if (connectedNodes.Count == 0)
                continue;
            var nodeConnectionDic = new NodeConnection()
            {
                ColorId = graph2Node.ColorId,
                ConnectionColorCountDic = new Dictionary<int, int>()
            };
            foreach (Node connectedNode in connectedNodes)
            {
                if (!nodeConnectionDic.ConnectionColorCountDic.ContainsKey(connectedNode.ColorId))
                    nodeConnectionDic.ConnectionColorCountDic.Add(connectedNode.ColorId, 0);
                nodeConnectionDic.ConnectionColorCountDic[connectedNode.ColorId]++;
            }

            bool isMatched = false;
            //Compare with the nodes with same colorId on the graph1
            foreach (NodeConnection graph1NodeConnection in graph1ConnectionCountPerNode.Where(nc => nc.ColorId == graph2Node.ColorId).ToList())
            {
                var left = graph1NodeConnection.ConnectionColorCountDic
                    .Where(entry => nodeConnectionDic.ConnectionColorCountDic[entry.Key] != entry.Value)
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
                if (!left.Any())
                {
                    isMatched = true;
                    graph1ConnectionCountPerNode.Remove(graph1NodeConnection);
                    break;
                }
            }
            //if we cannot match this one graph2 node in any of graph1 nodes, we say the graphs are not same
            if (!isMatched)
                return false;
        }


        //Compare lengts

        //if we pass all the comparisons, we can safely say they are equal.
        return true;
    }


    private class NodeConnection
    {
        public int ColorId { get; set; }
        public Dictionary<int, int> ConnectionColorCountDic { get; set; } //<Color of the connected node, count of connected nodes in that Color>
    }
}



public static class StaticValues
{
    public static Color[] ColorByIndex = new Color[]
    {
        new Color(0.3f,0.3f,0.3f),
        new Color(0.5f,0,0),
        new Color(0,0.5f,0),
        new Color(0,0,0.5f),
        new Color(0.5f,0.5f,0),
        new Color(0.5f,0,0.5f),
        new Color(0,0.5f,0.5f),
    };

}
