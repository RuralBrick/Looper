using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopDisplayManager : MonoBehaviour
{
    const float RADIUS = 4f;
    const float SUB_BEAT_LINE_WIDTH = 0.025f;
    const float LANE_LINE_WIDTH = 0.05f;
    const int LANE_LINE_SUBDIVISIONS = 360;

    public Material lineMaterial;
    public Material spriteMaterial;
    public Sprite circleSprite;

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

    public void MakeLaneLines(float laneWidth, int laneCount = 4)
    {
        float allLaneWidth = laneWidth * laneCount;

        Debug.Assert(allLaneWidth <= RADIUS);

        float innerGap = RADIUS - allLaneWidth;

        for (int i = laneCount; i > 0; i--)
        {
            GameObject line = new GameObject("Lane Line " + i);
            line.transform.parent = transform;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.sortingLayerName = "Track";
            lr.sortingOrder = 100;
            lr.loop = true;
            lr.startWidth = lr.endWidth = LANE_LINE_WIDTH;
            lr.positionCount = LANE_LINE_SUBDIVISIONS;

            float r = (((float)i / 4) * allLaneWidth) + innerGap;
            Vector3[] pos = new Vector3[LANE_LINE_SUBDIVISIONS];
            for (int angle = 0; angle < LANE_LINE_SUBDIVISIONS; angle++)
            {
                float a = angle * Mathf.Deg2Rad;
                float x = r * Mathf.Cos(a);
                float y = r * Mathf.Sin(a);
                pos[angle] = new Vector3(x, y, 0);
            }
            lr.SetPositions(pos);
        }

        if (innerGap < Mathf.Epsilon)
        {
            GameObject dot = new GameObject("Center Dot");
            dot.transform.parent = transform;
            dot.transform.localScale = new Vector3(LANE_LINE_WIDTH, LANE_LINE_WIDTH, 1f);

            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.material = spriteMaterial;
            sr.sprite = circleSprite;
            sr.sortingLayerName = "Track";
            sr.sortingOrder = 100;
        }
        else
        {
            GameObject line = new GameObject("Lane Line 0");
            line.transform.parent = transform;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.sortingLayerName = "Track";
            lr.sortingOrder = 100;
            lr.loop = true;
            lr.startWidth = lr.endWidth = LANE_LINE_WIDTH;
            lr.positionCount = LANE_LINE_SUBDIVISIONS;

            float r = innerGap;
            Vector3[] pos = new Vector3[LANE_LINE_SUBDIVISIONS];
            for (int angle = 0; angle < LANE_LINE_SUBDIVISIONS; angle++)
            {
                float a = angle * Mathf.Deg2Rad;
                float x = r * Mathf.Cos(a);
                float y = r * Mathf.Sin(a);
                pos[angle] = new Vector3(x, y, 0);
            }
            lr.SetPositions(pos);
        }
    }
}
