using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController instance;

    public event System.Action<Transform> OnMouseDown;
    public event System.Action<Transform> OnMouseUp;
    public event System.Action<Transform> OnMouseMove;
    
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //We transform the touch position into word space from screen space and store it.
        Vector3 touchPosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);
        //We now raycast with this information. If we have hit something we can process it.
        RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, Camera.main.transform.forward);

        if (Input.GetMouseButtonDown(0))
            OnMouseDown?.Invoke(hitInformation.collider != null ? hitInformation.transform : null);

        if (Input.GetMouseButton(0))
            OnMouseMove?.Invoke(hitInformation.collider != null ? hitInformation.transform : null);

        if (Input.GetMouseButtonUp(0))
            OnMouseUp?.Invoke(hitInformation.collider != null ? hitInformation.transform : null);
    }
}
