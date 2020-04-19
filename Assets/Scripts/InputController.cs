using UnityEngine;

public class InputController : MonoBehaviour
{
    public Camera MainCamera;
    public LevelManager LevelManager;

    public static InputController instance;

    public event System.Action<Vector2, Transform> OnMouseDown;
    public event System.Action<Vector2, Transform> OnMouseUp;
    public event System.Action<Vector2, Transform> OnMouseMove;
    
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //We transform the touch position into word space from screen space and store it.
        Vector3 touchPosWorld = MainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);
        Transform hitTransform = null;
        //We now raycast with this information. If we have hit something we can process it.
        RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, MainCamera.transform.forward);
        if (hitInformation.transform)
            hitTransform = hitInformation.transform;
        else if(Physics.Raycast(touchPosWorld, Vector3.forward, out var hitInformation3D))
        {
            hitTransform = hitInformation3D.transform;
        }
        if (Input.GetMouseButtonDown(0))
            OnMouseDown?.Invoke(touchPosWorld2D, hitTransform);

        if (Input.GetMouseButton(0))
            OnMouseMove?.Invoke(touchPosWorld2D, hitTransform);

        if (Input.GetMouseButtonUp(0))
            OnMouseUp?.Invoke(touchPosWorld2D, hitTransform);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LevelManager.LoadLevel(0);
        }
    }
}
