using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMarkHandler : MonoBehaviour
{
    public static float HIT_NOTE_LENGTH = 0.1f;

    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetColor(Color color)
    {
        lineRenderer.startColor = lineRenderer.endColor = color;
    }

    public void SetPositions(float left, float displayWidth)
    {
        float right = left + HIT_NOTE_LENGTH;
        if (right > displayWidth)
        {
            right = displayWidth;
        }

        lineRenderer.SetPositions(new Vector3[] { Vector3.right * left, Vector3.right * right });
    }
}
