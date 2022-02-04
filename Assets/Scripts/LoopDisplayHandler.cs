using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopDisplayHandler : MonoBehaviour
{
    public const int LANE_COUNT = 4;

    public static bool clockwiseMetronome = true;
    
    const float RADIUS = 4f;
    const float DOWNBEAT_LINE_WIDTH = 0.1f;

    const float NOTE_WIDTH = 0.7f;
    const float LANE_WIDTH = 0.75f;

    const float TRACK_WIDTH = LANE_COUNT * LANE_WIDTH;
    const uint _track_width_check = TRACK_WIDTH <= RADIUS ? 0 : -666;
    const float INNER_GAP = RADIUS - TRACK_WIDTH;

    int beatsPerBar = 0;
    
    public GameObject beatLinePrefab;
    public GameObject laneLinePrefab;

    LineRenderer metronomeLine;
    
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
        for (int i = 1; i < beatsPerBar; i++)
        {
            BeatLineHandler beatLine = Instantiate(beatLinePrefab, transform).GetComponent<BeatLineHandler>();
            beatLine.SetPosition(CalcLinePosition(i));
        }
    }

    void MakeLaneLines()
    {
        for (int i = LANE_COUNT; i > 0; i--)
        {
            float lanePercent = (float)i / LANE_COUNT;
            float trackR = lanePercent * TRACK_WIDTH;
            float r = trackR + INNER_GAP;

            LaneLineHandler laneLine = Instantiate(laneLinePrefab, transform).GetComponent<LaneLineHandler>();
            laneLine.SetPositions(r);
        }

        if (INNER_GAP > DOWNBEAT_LINE_WIDTH)
        {
            LaneLineHandler laneLine = Instantiate(laneLinePrefab, transform).GetComponent<LaneLineHandler>();
            laneLine.SetPositions(INNER_GAP);
        }
    }
}
