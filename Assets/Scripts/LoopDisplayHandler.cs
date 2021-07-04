using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopDisplayHandler : MonoBehaviour
{
    const float RADIUS = 4f;
    const float SUB_BEAT_LINE_WIDTH = 0.025f;
    const float LANE_LINE_WIDTH = 0.025f;
    const int LANE_LINE_SUBDIVISIONS = 360;

    bool initialized = false;
    int beatsPerBar = 0;
    int laneCount = 0;
    float laneWidth = 0f;
    float trackWidth = 0f;
    float innerGap = 0f;

    public Color lineColor;
    public Sprite circleSprite;
    public Material lineMaterial;
    public Material spriteMaterial;

    LineRenderer metronomeLine;
    
    void Awake()
    {
        metronomeLine = transform.Find("Metronome Line").GetComponent<LineRenderer>();
    }

    Vector3 CalcLinePosition(float beatPos)
    {
        float barPercent = beatPos / beatsPerBar;
        float angle = (barPercent + 0.25f) * 2f * Mathf.PI;
        float x = RADIUS * Mathf.Cos(angle);
        float y = RADIUS * Mathf.Sin(angle);
        return new Vector3(x, y, 0);
    }
    
    public Vector3 CalcNotePosition(int lane, float beatPos)
    {
        Debug.Assert(initialized && lane <= laneCount && beatPos < beatsPerBar);

        float lanePercent = (float)lane / laneCount;
        float trackR = lanePercent * trackWidth;
        float r = trackR + innerGap;
        r += laneWidth / 2;

        float barPercent = beatPos / beatsPerBar;
        float angle = (barPercent + 0.25f) * 2f * Mathf.PI;
        float x = r * Mathf.Cos(angle);
        float y = r * Mathf.Sin(angle);

        return new Vector3(x, y, 0);
    }
    
    public void SetMetronome(float beatPos)
    {
        Debug.Assert(initialized && beatPos < beatsPerBar);

        metronomeLine.SetPosition(1, CalcLinePosition(beatPos));
    }

    public void HideMetronome()
    {
        metronomeLine.enabled = false;
    }

    public void Initialize(int beatsPerBar, int laneCount, float laneWidth)
    {
        this.beatsPerBar = beatsPerBar;
        this.laneCount = laneCount;
        this.laneWidth = laneWidth;

        trackWidth = laneCount * laneWidth;
        Debug.Assert(trackWidth <= RADIUS);
        innerGap = RADIUS - trackWidth;

        MakeBeatLines();
        MakeLaneLines();

        initialized = true;
    }

    void MakeBeatLines()
    {
        for (int i = 1; i < beatsPerBar; i++)
        {
            GameObject line = new GameObject("Beat " + (i + 1) + " Line");
            line.transform.parent = transform;
            line.transform.localPosition = Vector3.zero;
            line.transform.localScale = Vector3.one;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.startColor = lr.endColor = lineColor;
            lr.material = lineMaterial;
            lr.sortingLayerName = "Track";
            lr.sortingOrder = 100;
            lr.startWidth = lr.endWidth = SUB_BEAT_LINE_WIDTH;
            lr.useWorldSpace = false;
            lr.SetPosition(1, CalcLinePosition(i));
        }
    }

    void MakeLaneLines()
    {
        for (int i = laneCount; i > 0; i--)
        {
            GameObject line = new GameObject("Lane Line " + i);
            line.transform.parent = transform;
            line.transform.localPosition = Vector3.zero;
            line.transform.localScale = Vector3.one;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.startColor = lr.endColor = lineColor;
            lr.material = lineMaterial;
            lr.sortingLayerName = "Track";
            lr.sortingOrder = 100;
            lr.loop = true;
            lr.startWidth = lr.endWidth = LANE_LINE_WIDTH;
            lr.useWorldSpace = false;
            lr.positionCount = LANE_LINE_SUBDIVISIONS;

            float lanePercent = (float)i / laneCount;
            float trackR = lanePercent * trackWidth;
            float r = trackR + innerGap;
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
            dot.transform.localPosition = Vector3.zero;
            dot.transform.localScale = new Vector3(LANE_LINE_WIDTH, LANE_LINE_WIDTH, 1f);

            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.color = lineColor;
            sr.material = spriteMaterial;
            sr.sprite = circleSprite;
            sr.sortingLayerName = "Track";
            sr.sortingOrder = 100;
        }
        else
        {
            GameObject line = new GameObject("Lane Line 0");
            line.transform.parent = transform;
            line.transform.localPosition = Vector3.zero;
            line.transform.localScale = Vector3.one;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.startColor = lr.endColor = lineColor;
            lr.material = lineMaterial;
            lr.sortingLayerName = "Track";
            lr.sortingOrder = 100;
            lr.loop = true;
            lr.startWidth = lr.endWidth = LANE_LINE_WIDTH;
            lr.useWorldSpace = false;
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
