using UnityEngine;

public static class StaticValues
{
    public static int MaxNodeCountPerGraph = 8;

    public static Color32[] ColorByIndex = new Color32[]
    {
        new Color32(77, 77, 77, 255),
        new Color32(225, 88, 58, 255),
        new Color32(104, 189, 233, 255),
        new Color32(254, 187, 48, 255),
        new Color32(155, 110, 209, 255)
    };

    public static int[] MaxConnectionCountByNodeCount = new[]
    {
        0,
        0,
        1,
        3,
        6,
        10,
        15,
        21,
        28,
        36,
        45,
        55,
        61,
        73,
        86,
        100
    };

}