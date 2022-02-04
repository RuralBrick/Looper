using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatLineHandler : MonoBehaviour
{
    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetPosition(Vector3 position)
    {
        lineRenderer.SetPosition(1, position);
    }
}
