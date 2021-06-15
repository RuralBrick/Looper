using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopDisplayManager : MonoBehaviour
{
    const float RADIUS = 4f;

    LineRenderer metronomeLine;

    void Awake()
    {
        metronomeLine = transform.Find("Metronome Line").GetComponent<LineRenderer>();
    }

    public void SetMetronome(float barPercent)
    {
        float angle = (barPercent + 0.25f) * 2 * Mathf.PI;
        float x = RADIUS * Mathf.Cos(angle);
        float y = RADIUS * Mathf.Sin(angle);
        metronomeLine.SetPosition(1, new Vector3(x, y, 0));
    }
}
