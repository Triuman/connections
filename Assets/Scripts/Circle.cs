using UnityEngine;

public class Circle : MonoBehaviour
{
    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}
