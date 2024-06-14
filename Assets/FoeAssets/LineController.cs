using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    private LineRenderer lr;
    private Transform[] points;
    
    private float age;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null)
        {
            Debug.Log("LineRenderer not found");
        }
    }

    private void Start()
    {
        age = 0;
    }

    public void SetUpLine(Vector3 from, Vector3 to)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
    }

    [System.Obsolete] // SetWidth is obsolete
    private void Update()
    {
        age+= Time.deltaTime;

        lr.SetWidth(0.2f * age, 0.1f * age);
    }
}
