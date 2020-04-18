using UnityEngine;
using Random = UnityEngine.Random;

public class Node : MonoBehaviour
{
    public Circle Circle;
    public int GraphId;
    public int Id { get; private set; }
    public int Index { get; set; }
    [SerializeField]
    private int colorId;
    public int ColorId
    {
        get => colorId;
        set
        {
            colorId = value;
            Circle.SetColor(StaticValues.ColorByIndex[ColorId - 1]);
        }
    }

    public float Scale
    {
        set => transform.localScale = new Vector3(value, value, value);
    }

    void Start()
    {
        SetRandomId(this);
        ColorId = colorId;
    }


    /// <summary>
    /// In case there is a node with same Id in a graph, we can call this function from outside and add the node safely.
    /// </summary>
    public static void SetRandomId(Node node) => node.Id = Random.Range(1, 10000000);
}

