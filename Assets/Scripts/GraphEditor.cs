using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphEditor : MonoBehaviour
{
    public float Width;
    public float Height;

    public GameObject DotPrefab;
    public GraphLine GraphLinePrefab;

    private Graph graph;

    private readonly int Id = Random.Range(1, 1000);

    // Start is called before the first frame update
    void Start()
    {
        //Place small dots by size of the graph
        float spaceX = Width / Graph.Size;
        float spaceY = Height / Graph.Size;
        float startPositionX = transform.position.x - Width / 2;
        float startPositionY = transform.position.y - Height / 2;
        DotPrefab.GetComponent<ConnectionDot>().SetDotScale(0.3f);
        for (int x = 0; x < Graph.Size; x++)
        {
            for (int y = 0; y < Graph.Size; y++)
            {
                DotPrefab.tag = "dot|" + Id + "|" + x + "|" + y;
                Instantiate(DotPrefab, new Vector2(x * spaceX + startPositionX, y * spaceY + startPositionY), Quaternion.identity, transform);
            }
        }

        InputController.instance.OnMouseDown += OnMouseDown;
        InputController.instance.OnMouseMove += OnMouseMove;
        InputController.instance.OnMouseUp += OnMouseUp;
    }

    private void OnMouseDown(Transform obj)
    {
        throw new System.NotImplementedException();
    }

    private void OnMouseMove(Transform obj)
    {
        throw new System.NotImplementedException();
    }

    private void OnMouseUp(Transform obj)
    {
        throw new System.NotImplementedException();
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
