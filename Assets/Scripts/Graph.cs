using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Graph
{
    public int Id { get; private set; }
    public event Action OnGraphChange;

    private readonly int[,] graphMatrix;
    private const int defaultMatrixSize = 25; //TODO: make it 25

    public Graph(int[,] matrix)
    {
        graphMatrix = matrix ?? new int[defaultMatrixSize, defaultMatrixSize];

        SetRandomId(this);
    }
    public Graph(int[] nodeColorIds, int[] connections)
    {
        graphMatrix = new int[nodeColorIds.Length, nodeColorIds.Length];
        for (int i = 0; i < nodeColorIds.Length; i++)
        {
            graphMatrix[i, i] = nodeColorIds[i];
        }

        for (int c = 0; c < connections.Length; c += 2)
        {
            var connectionNode1 = connections[c];
            var connectionNode2 = connections[c + 1];
            if (connectionNode1 == connectionNode2)
                continue;
            graphMatrix[connectionNode1, connectionNode2] = nodeColorIds[connectionNode2];
            graphMatrix[connectionNode2, connectionNode1] = nodeColorIds[connectionNode1];
        }

        SetRandomId(this);
    }
    public static void SetRandomId(Graph graph) => graph.Id = Random.Range(1, 10000000);
    public static void SetId(Graph graph, int id) => graph.Id = id;

    public static void SetNodeColor(Graph graph, int index, int colorId = 1)
    {
        //We keep colorId of each node on the diagonal of the matrix.
        //And we put the colorId of connected node to say it is connected to that node.
        //So, the matrix is not symetrical anymore.

        //   0 1 2 3 4
        // 0 2 3 0 0 2
        // 1 2 3 0 2 0
        // 2 0 0 5 0 0
        // 3 0 3 0 2 2
        // 4 2 0 0 2 2
        graph.graphMatrix[index, index] = colorId;
        graph.OnGraphChange?.Invoke();
    }


    public static void AddConnectionMatrix(Graph graph, int node1Index, int node2Index)
    {
        var node1Number = graph.graphMatrix[node1Index, node1Index];
        var node2Number = graph.graphMatrix[node2Index, node2Index];
        graph.graphMatrix[node1Index, node2Index] = node2Number;
        graph.graphMatrix[node2Index, node1Index] = node1Number;
        graph.OnGraphChange?.Invoke();
    }
    public static void RemoveConnectionMatrix(Graph graph, int node1Index, int node2Index)
    {
        graph.graphMatrix[node1Index, node2Index] = 0;
        graph.graphMatrix[node2Index, node1Index] = 0;
        graph.OnGraphChange?.Invoke();
    }
    public static bool IsNodesConnectedMatrix(Graph graph, int node1Index, int node2Index)
    {
        var node1Number = graph.graphMatrix[node1Index, node1Index];
        var node2Number = graph.graphMatrix[node2Index, node2Index];
        return graph.graphMatrix[node1Index, node2Index] == node2Number && graph.graphMatrix[node2Index, node1Index] == node1Number;
    }

    internal static bool IsTwoGraphSameMatrix(Graph graph1, Graph graph2)
    {
        if (graph1.graphMatrix.Rank != 2 && graph1.graphMatrix.Rank != graph2.graphMatrix.Rank ||
            graph1.graphMatrix.GetLength(0) != graph1.graphMatrix.GetLength(1) ||
            graph2.graphMatrix.GetLength(0) != graph2.graphMatrix.GetLength(1))
            return false;

        var graph1Size = graph1.graphMatrix.GetLength(0);
        var graph2Size = graph2.graphMatrix.GetLength(0);

        var graph1MatrixTemp = graph1.graphMatrix.Clone() as int[,];
        var graph2MatrixTemp = graph2.graphMatrix.Clone() as int[,];

        //Process matrix and put every node index into dictionary list by their connection count.
        //So, <5, [0,3,4]> means nodes 0,3 and 4 have 5 connections each.
        //After grouping them by their connection number, we will sort the matrix and permutate them.
        var graph1IndexListPerCountDic = new SortedDictionary<int, List<int>>(); //<count>, <list of indices that has same number of connections>
        for (int i = 0; i < graph1Size; i++)
        {
            var countOfIndex = 0;
            for (int j = 0; j < graph1Size; j++)
            {
                if (i == j)
                    continue;
                if (graph1MatrixTemp[i, j] > 0)
                    countOfIndex++;
            }

            if (countOfIndex <= 0)
                continue;

            if (!graph1IndexListPerCountDic.ContainsKey(countOfIndex))
                graph1IndexListPerCountDic[countOfIndex] = new List<int>();
            graph1IndexListPerCountDic[countOfIndex].Add(i);
        }

        if (graph1IndexListPerCountDic.Count == 0)
        {
            return false;
        }


        var graph2IndexListPerCountDic = new SortedDictionary<int, List<int>>(); //<count>, <list of indices that has same number of connections>
        for (int i = 0; i < graph2Size; i++)
        {
            var countOfIndex = 0;
            for (int j = 0; j < graph2Size; j++)
            {
                if (i == j)
                    continue;
                if (graph2MatrixTemp[i, j] > 0)
                    countOfIndex++;
            }

            if (countOfIndex <= 0)
                continue;

            if (!graph2IndexListPerCountDic.ContainsKey(countOfIndex))
                graph2IndexListPerCountDic[countOfIndex] = new List<int>();
            graph2IndexListPerCountDic[countOfIndex].Add(i);
        }

        if (graph2IndexListPerCountDic.Count == 0)
        {
            return false;
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
        //     2 3 5 4 1 0
        //
        // 2   0 0 0 1 1 0
        // 3   0 0 0 1 1 0
        // 5   0 0 0 1 1 1
        // 4   1 1 1 0 0 1
        // 1   1 1 1 0 0 1
        // 0   0 0 1 1 1 0
        // </sorted>
        // 

        var graph1CountDicKeys = graph1IndexListPerCountDic.Keys.ToArray();

        var graph2CountDicKeys = graph2IndexListPerCountDic.Keys.ToArray();
        var combinationGraph2 = new List<int>();
        foreach (int graph2CountDicKey in graph2CountDicKeys)
        {
            combinationGraph2.AddRange(graph2IndexListPerCountDic[graph2CountDicKey]);
        }

        graph2MatrixTemp = MultiArrayHelper<int>.OrderArray(graph2MatrixTemp, combinationGraph2);


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
            var graph1MatrixTempCombination = MultiArrayHelper<int>.OrderArray(graph1MatrixTemp, combination);
            bool isCombinationCorrect = true;
            for (int i = 0; i < combination.Count; i++)
            {
                var rowGraph1 = MultiArrayHelper<int>.GetRow(graph1MatrixTempCombination, i);
                var rowGraph2 = MultiArrayHelper<int>.GetRow(graph2MatrixTemp, i);

                if (!MultiArrayHelper<int>.CompareRows(rowGraph1, rowGraph2))
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


    private static class MultiArrayHelper<T>
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
        private static T[,] SwapColumns(T[,] matrix, int index1, int index2)
        {
            var resultMatrix = matrix.Clone() as T[,];
            var column1 = GetColumn(matrix, index1);
            var column2 = GetColumn(matrix, index2);

            resultMatrix = SetColumn(resultMatrix, index1, column2);
            resultMatrix = SetColumn(resultMatrix, index2, column1);

            return resultMatrix;
        }
        private static T[,] SwapRows(T[,] matrix, int index1, int index2)
        {
            var resultMatrix = matrix.Clone() as T[,];
            var row1 = GetRow(matrix, index1);
            var row2 = GetRow(matrix, index2);

            resultMatrix = SetRow(resultMatrix, index1, row2);
            resultMatrix = SetRow(resultMatrix, index2, row1);

            return resultMatrix;
        }
        private static T[,] SetColumn(T[,] matrix, int columnIndex, T[] column)
        {
            var resultMatrix = matrix.Clone() as T[,];
            for (int r = 0; r < resultMatrix.GetLength(0); r++)
            {
                resultMatrix[r, columnIndex] = column[r];
            }
            return resultMatrix;
        }
        private static T[,] SetRow(T[,] matrix, int rowIndex, T[] row)
        {
            var resultMatrix = matrix.Clone() as T[,];
            for (int c = 0; c < resultMatrix.GetLength(1); c++)
            {
                resultMatrix[rowIndex, c] = row[c];
            }
            return resultMatrix;
        }
        private static T[] GetColumn(T[,] matrix, int columnIndex)
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
            var smallSize = Mathf.Min(row1.Length, row2.Length);
            var biggerArray = row1.Length == smallSize ? row2 : row1;
            for (int i = 0; i < smallSize; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(row1[i], row2[i]))
                    return false;
            }

            //We need to make sure that rest of the bigger array has only default value. eg. 0 for int, false for bool, null for an object
            for (int i = smallSize; i < biggerArray.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(biggerArray[i], default(T)))
                    return false;
            }

            return true;
        }
    }

}


