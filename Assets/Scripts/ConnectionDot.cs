using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionDot : MonoBehaviour
{
    public GameObject DotSprite;
    public void SetDotScale(float scale)
    {
        DotSprite.transform.localScale = new Vector3(scale, scale, 1);
    }

    public static string CreateDotTag(int editorId, int x, int y)
    {
        return "dot|" + editorId + "|" + x + "|" + y;
    }
    public static DotInfo GetDotInfoFromTag(string tag)
    {
        var split = tag.Split('|');
        if (split.Length != 4 || split[0] != "dot")
            return null;
        return new DotInfo()
        {
            GraphId = Int32.Parse(split[1]),
            X = Int32.Parse(split[2]),
            Y = Int32.Parse(split[3])
        };
    }

}

public class DotInfo
{
    public int GraphId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}
