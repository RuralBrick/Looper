using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneLineHandler : MonoBehaviour
{
    public static int LANE_LINE_SUBDIVISIONS = 360;

    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetPositions(float radius)
    {
        Vector3[] pos = new Vector3[LANE_LINE_SUBDIVISIONS];
        for (int angle = 0; angle < LANE_LINE_SUBDIVISIONS; angle++)
        {
            float a = angle * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(a);
            float y = radius * Mathf.Sin(a);
            pos[angle] = new Vector3(x, y, 0);
        }
        lineRenderer.SetPositions(pos);
    }
}
