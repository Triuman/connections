using System.Collections;
using System.Collections.Generic;
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

    private Vector3 _position;
    public Vector3 Position
    {
        get => new Vector3(_position.x, _position.y, _position.z);
        set
        {
            _position = new Vector3(value.x, value.y, value.z);
            transform.localPosition = _position;
        }
    }

    public List<ConnectionLine> lines;

    public float Scale
    {
        set => transform.localScale = new Vector3(value, value, value);
    }

    void Awake()
    {
        _position = transform.localPosition;
        SetRandomId(this);
        lines = new List<ConnectionLine>(StaticValues.MaxNodeCountPerGraph);
    }


    /// <summary>
    /// In case there is a node with same Id in a graph, we can call this function from outside and add the node safely.
    /// </summary>
    public static void SetRandomId(Node node) => node.Id = Random.Range(1, 10000000);


    public void MoveTo(Vector3 targetPosition, float duration)
    {
        _position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        StopCoroutine(nameof(MoveToCoroutine));
        StartCoroutine(MoveToCoroutine(duration));
    }

    private IEnumerator MoveToCoroutine(float duration)
    {
        while (Vector3.Distance(transform.localPosition, _position) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _position, 1f / duration * Time.deltaTime);
            for (int cl = 0; cl < lines.Count; cl++)
            {
                lines[cl].UpdateNodePosition(Index, transform.position);
            }
            yield return null;
        }
    }
}

