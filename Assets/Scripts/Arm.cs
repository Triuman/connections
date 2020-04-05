using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    private Vector2 targetPosition;
    List<float> armLenghts = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = new Vector2();
        armLenghts.Add(1);
        armLenghts.Add(0.5f);
    }

    public void SetTargetPosition(Vector2 pos)
    {
        targetPosition = pos;
        var currentPos = transform.position;

        for (int i = armLenghts.Count - 1; i >= 1; i--)
        {
            var armLength1 = armLenghts[i - 1];
            var armLength2 = armLenghts[i];


        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
