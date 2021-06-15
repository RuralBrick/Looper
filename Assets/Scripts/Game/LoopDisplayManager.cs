using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopDisplayManager : MonoBehaviour
{
    const float RADIUS = 4f;
    const float SUB_BEAT_LINE_WIDTH = 0.025f;

    public Material lineMaterial;

    LineRenderer metronomeLine;
    
    void Awake()
    {
        metronomeLine = transform.Find("Metronome Line").GetComponent<LineRenderer>();
    }

    Vector3 CalcLinePosition(float barPercent)
    {
        float angle = (barPercent + 0.25f) * 2 * Mathf.PI;
        float x = RADIUS * Mathf.Cos(angle);
        float y = RADIUS * Mathf.Sin(angle);
        return new Vector3(x, y, 0);
    }

    public void SetMetronome(float barPercent)
    {
        metronomeLine.SetPosition(1, CalcLinePosition(barPercent));
    }

    public void MakeBeatLines(int beatsPerBar)
    {
        for (int i = 1; i < beatsPerBar; i++)
        {
            GameObject line = new GameObject("Beat " + (i + 1) + " Line");
            line.transform.parent = transform;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.sortingLayerName = "Track";
            lr.sortingOrder = 100;
            lr.startWidth = lr.endWidth = SUB_BEAT_LINE_WIDTH;
            lr.SetPosition(1, CalcLinePosition((float)i / beatsPerBar));
        }
    }
}
