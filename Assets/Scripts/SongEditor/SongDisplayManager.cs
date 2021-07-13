using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongDisplayManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const float DISPLAY_WIDTH = 10f;
    const float DISPLAY_HEIGHT = 1.5f;
    const int NUM_BUFFER_BARS = 2;
    const float BAR_LINE_WIDTH = 0.025f;
    const float NOTE_LINE_WIDTH = 0.05f;
    const float HIT_NOTE_LENGTH = 0.1f;

    int beatsPerBar;
    float[] samples;
    float samplesPerBar;
    float samplesPerSecond;
    float offsetSamples;

    public Color lineColor;
    public Material lineMaterial;

    LineRenderer waveform;
    GameObject window;

    public Color[] laneColors = new Color[LoopDisplayHandler.LANE_COUNT];

    struct NoteLine
    {
        public LineRenderer duration;
        public List<LineRenderer> hits;
    }
    List<NoteLine> noteLines = new List<NoteLine>();

    void Awake()
    {
        waveform = transform.Find("Song Waveform").GetComponent<LineRenderer>();
        window = transform.Find("Window Mask").gameObject;
    }

    void Start()
    {
        float winWidth = DISPLAY_WIDTH / BarsDisplayed();
        float winHeight = DISPLAY_HEIGHT;
        window.transform.localScale = new Vector3(winWidth, winHeight, 1);

        MakeBarLines();
    }

    public static int BarsDisplayed()
    {
        return 2 * NUM_BUFFER_BARS + 1;
    }

    float Right { get => DISPLAY_WIDTH / 2f; }
    float Left { get => -(DISPLAY_WIDTH / 2f); }
    float Top { get => DISPLAY_HEIGHT / 2f; }
    float Bottom { get => -(DISPLAY_HEIGHT / 2f); }
    float BarWidth { get => DISPLAY_WIDTH / BarsDisplayed(); }
    
    void MakeBarLines()
    {
        for (int i = 1; i < BarsDisplayed(); i++)
        {
            GameObject line = new GameObject("Bar Line " + i);
            line.transform.parent = transform;
            line.transform.localPosition = Vector3.zero;
            line.transform.localScale = Vector3.one;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            GlobalManager.FormatLine(ref lr, lineColor, lineMaterial, "Track", 105, BAR_LINE_WIDTH);

            float x = Left + i * BarWidth;
            lr.SetPositions(new Vector3[] { new Vector3(x, Top, 0), new Vector3(x, Bottom, 0) });
        }
    }

    public void Initialize(Song s)
    {
        beatsPerBar = s.beatsPerBar;

        samples = new float[s.file.samples * s.file.channels];
        s.file.GetData(samples, 0);

        samplesPerBar = s.beatsPerBar * s.file.frequency * SECONDS_PER_MINUTE / s.tempo;
        samplesPerSecond = s.file.frequency;
    }

    float HalfHeight { get => DISPLAY_HEIGHT / 2f; }
    
    void UpdateWaveform(float[] samples)
    {
        float unitLength = DISPLAY_WIDTH / samples.Length;

        Vector3[] positions = new Vector3[samples.Length];

        for (int i = 0; i < samples.Length; i++)
        {
            float x = Left + i * unitLength;
            float y = samples[i] * HalfHeight;
            positions[i] = new Vector3(x, y, 0);
        }
        
        waveform.positionCount = samples.Length;
        waveform.SetPositions(positions);
    }

    public void DisplayBar()
    {
        float[] displaySamples = new float[(int)(BarsDisplayed() * samplesPerBar)];

        int startBar = EditorManager.currentBar - NUM_BUFFER_BARS;
        int start = (int)(startBar * samplesPerBar - offsetSamples);
        int endBar = EditorManager.currentBar + NUM_BUFFER_BARS + 1;
        int end = (int)(endBar * samplesPerBar - offsetSamples) - 1;
        if (end > samples.Length) end = samples.Length;

        int i = start;
        int j = 0;
        for (; i < 0; i++)
        {
            displaySamples[j] = 0f;
            j++;
        }
        for (; i < end; i++)
        {
            displaySamples[j] = samples[i];
            j++;
        }
        for (; j < displaySamples.Length; j++)
        {
            displaySamples[j] = 0f;
        }

        UpdateWaveform(displaySamples);
    }

    public void SetOffset(float newOffset)
    {
        offsetSamples = newOffset * samplesPerSecond;
        DisplayBar();
    }

    bool NoteInPhrase(float start, float stop)
    {
        float phraseStart = (EditorManager.currentBar - NUM_BUFFER_BARS) * beatsPerBar;
        float phraseEnd = (EditorManager.currentBar + NUM_BUFFER_BARS + 1) * beatsPerBar;
        return phraseStart <= stop && start < phraseEnd;
    }

    float LaneToY { get => DISPLAY_HEIGHT / (LoopDisplayHandler.LANE_COUNT + 1); }
    float CalcLaneY(int lane)
    {
        float disp = (lane + 1) * LaneToY;
        return Bottom + disp;
    }

    float BeatToX { get => DISPLAY_WIDTH / (beatsPerBar * BarsDisplayed()); }
    float BeatPosToX { get => BarWidth / beatsPerBar; }

    public void SpawnNoteLines(List<Ref<Note>> notes)
    {
        for (int i = noteLines.Count - 1; i >= 0; i--)
        {
            Destroy(noteLines[i].duration.gameObject);
            noteLines.RemoveAt(i);
        }

        foreach (Ref<Note> n in notes)
        {
            if (NoteInPhrase(n.Value.start, n.Value.stop))
            {
                NoteLine newNoteLine = new NoteLine();

                GameObject durationObj = new GameObject("Note Line");
                durationObj.transform.parent = transform;
                durationObj.transform.localPosition = new Vector3(Left, CalcLaneY(n.Value.lane));

                newNoteLine.duration = durationObj.AddComponent<LineRenderer>();
                GlobalManager.FormatLine(ref newNoteLine.duration, laneColors[n.Value.lane], lineMaterial, "Track", 150, NOTE_LINE_WIDTH);

                int leftBar = EditorManager.currentBar - NUM_BUFFER_BARS;
                float leftBeat = leftBar * beatsPerBar;
                float startBeatOffsetFromLeft = n.Value.start - leftBeat;
                float startOffsetFromLeft = startBeatOffsetFromLeft * BeatToX;
                float durationStart = startOffsetFromLeft < 0f ? 0f : startOffsetFromLeft;
                Vector3 durationLeft = new Vector3(durationStart, 0, 0);

                float stopBeatOffsetFromLeft = n.Value.stop - leftBeat;
                float stopOffsetFromLeft = stopBeatOffsetFromLeft * BeatToX;
                float durationStop = stopOffsetFromLeft > DISPLAY_WIDTH ? DISPLAY_WIDTH : stopOffsetFromLeft;
                Vector3 durationRight = new Vector3(durationStop, 0, 0);

                newNoteLine.duration.SetPositions(new Vector3[] { durationLeft, durationRight });


                newNoteLine.hits = new List<LineRenderer>();

                Color hitColor = Color.HSVToRGB(n.Value.beatPos / beatsPerBar, 1f, 1f);

                float firstHit = n.Value.beatPos * BeatPosToX;

                while (firstHit < DISPLAY_WIDTH && firstHit < durationStart)
                    firstHit += BarWidth;
                for (float x = firstHit; x <= durationStop && x < DISPLAY_WIDTH; x += BarWidth)
                {
                    GameObject hitObj = new GameObject("Hit Mark");
                    hitObj.transform.parent = durationObj.transform;
                    hitObj.transform.localPosition = Vector3.zero;

                    LineRenderer newHit = hitObj.AddComponent<LineRenderer>();
                    GlobalManager.FormatLine(ref newHit, hitColor, lineMaterial, "Track", 155, NOTE_LINE_WIDTH);

                    Vector3 hitLeft = new Vector3(x, 0, 0);

                    float right = x + HIT_NOTE_LENGTH;
                    if (right > DISPLAY_WIDTH) right = DISPLAY_WIDTH;
                    Vector3 hitRight = new Vector3(right, 0, 0);

                    newHit.SetPositions(new Vector3[] { hitLeft, hitRight });

                    newNoteLine.hits.Add(newHit);
                }


                noteLines.Add(newNoteLine);
            }
        }
    }
}
