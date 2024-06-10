using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    private LineRenderer lr;
    private Transform[] points;
    
    private float age = 0f;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null)
        {
            Debug.Log("LineRenderer not found");
        }
    }

    public void SetUpLine(Transform[] points)
    {
        lr.positionCount = points.Length;
        this.points = points;
    }

    [System.Obsolete]
    private void Update()
    {
        age+= Time.deltaTime;

        if (age > 1f)
        {
            Destroy(gameObject);
        }

        if (points == null)
        {
            return;
        }
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
            {
                Destroy(gameObject);
                return;
            }
            lr.SetPosition(i, points[i].position);
        }

        lr.SetWidth(0.2f * age, 0.1f *age);
    }
}
