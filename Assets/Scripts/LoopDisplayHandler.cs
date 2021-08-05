using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopDisplayHandler : MonoBehaviour
{
    public const int LANE_COUNT = 4;

    public static bool clockwiseMetronome = true;
    
    const float RADIUS = 4f;
    const float LANE_WIDTH = 0.75f;
    const float TRACK_WIDTH = LANE_COUNT * LANE_WIDTH; // NOTE: Make sure TRACK_WIDTH <= RADIUS
    const float INNER_GAP = RADIUS - TRACK_WIDTH;
    const float NOTE_WIDTH = 0.7f;
    const float SUB_BEAT_LINE_WIDTH = 0.025f;
    const float LANE_LINE_WIDTH = 0.025f;
    const int LANE_LINE_SUBDIVISIONS = 360;

    int beatsPerBar = 0;
    
    public Color lineColor;
    public Sprite circleSprite;
    public Material lineMaterial;
    public Material spriteMaterial;

    LineRenderer metronomeLine;
    List<LineRenderer> beatLines = new List<LineRenderer>();
    
    void Awake()
    {
        metronomeLine = transform.Find("Metronome Line").GetComponent<LineRenderer>();
    }

    void Start()
    {
        MakeLaneLines();
    }

    public void Initialize(int beatsPerBar)
    {
        this.beatsPerBar = beatsPerBar;
        MakeBeatLines();
    }

    public Vector3 CalcNotePosition(int lane, float beatPos)
    {
        Debug.Assert(beatsPerBar != 0 && beatPos < beatsPerBar);

        float lanePercent = (float)lane / LANE_COUNT;
        float trackR = lanePercent * TRACK_WIDTH;
        float r = trackR + INNER_GAP;
        r += LANE_WIDTH / 2f;
        Debug.Assert(Mathf.Approximately(transform.localScale.x, transform.localScale.y));
        r *= transform.localScale.x;

        float barPercent = beatPos / beatsPerBar;
        float angle = (barPercent + 0.25f) * 2f * Mathf.PI;
        float x = r * Mathf.Cos(angle);
        if (clockwiseMetronome) x *= -1;
        x += transform.position.x;
        float y = r * Mathf.Sin(angle);
        y += transform.position.y;

        return new Vector3(x, y, 0);
    }

    public Vector3 CalcNoteScale()
    {
        Debug.Assert(Mathf.Approximately(transform.localScale.x, transform.localScale.y));
        float scale = NOTE_WIDTH * transform.localScale.x;
        return new Vector3(scale, scale, 1);
    }

    Vector3 CalcLinePosition(float beatPos)
    {
        Debug.Assert(beatsPerBar != 0 && beatPos < beatsPerBar);
        float barPercent = beatPos / beatsPerBar;
        float angle = (barPercent + 0.25f) * 2f * Mathf.PI;
        float x = RADIUS * Mathf.Cos(angle);
        if (clockwiseMetronome) x *= -1;
        float y = RADIUS * Mathf.Sin(angle);
        return new Vector3(x, y, 0);
    }

    public void SetMetronome(float beatPos)
    {
        Debug.Assert(beatsPerBar != 0 && beatPos < beatsPerBar);

        metronomeLine.SetPosition(1, CalcLinePosition(beatPos));
    }

    public void HideMetronome()
    {
        metronomeLine.enabled = false;
    }

    void MakeBeatLines()
    {
        foreach (LineRenderer lr in beatLines)
            Destroy(lr.gameObject);
        beatLines.Clear();

        for (int i = 1; i < beatsPerBar; i++)
        {
            GameObject line = new GameObject("Beat " + (i + 1) + " Line");
            line.transform.parent = transform;
            line.transform.localPosition = Vector3.zero;
            line.transform.localScale = Vector3.one;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            GlobalManager.FormatLine(ref lr, lineColor, lineMaterial, "Track", 100, SUB_BEAT_LINE_WIDTH);
            lr.SetPosition(1, CalcLinePosition(i));

            beatLines.Add(lr);
        }
    }

    void MakeLaneLines()
    {
        for (int i = LANE_COUNT; i > 0; i--)
        {
            GameObject line = new GameObject("Lane Line " + i);
            line.transform.parent = transform;
            line.transform.localPosition = Vector3.zero;
            line.transform.localScale = Vector3.one;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            GlobalManager.FormatLine(ref lr, lineColor, lineMaterial, "Track", 100, LANE_LINE_WIDTH);
            lr.loop = true;
            lr.positionCount = LANE_LINE_SUBDIVISIONS;

            float lanePercent = (float)i / LANE_COUNT;
            float trackR = lanePercent * TRACK_WIDTH;
            float r = trackR + INNER_GAP;
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

        if (Mathf.Approximately(INNER_GAP, 0f))
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
            GlobalManager.FormatLine(ref lr, lineColor, lineMaterial, "Track", 100, LANE_LINE_WIDTH);
            lr.loop = true;
            lr.positionCount = LANE_LINE_SUBDIVISIONS;

            float r = INNER_GAP;
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
