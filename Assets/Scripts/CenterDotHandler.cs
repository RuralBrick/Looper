using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterDotHandler : MonoBehaviour
{
    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
}
