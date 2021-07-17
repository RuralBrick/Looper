using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongDisplayManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const float RIGHT = 5f;
    const float LEFT = -5f;
    const float TOP = 0.75f;
    const float BOTTOM = -0.75f;
    const int NUM_BARS = 5;
    const float DISPLAY_WIDTH = RIGHT - LEFT;
    const float BAR_WIDTH = DISPLAY_WIDTH / NUM_BARS;
    const float DISPLAY_HEIGHT = TOP - BOTTOM;
    const float HALF_HEIGHT = DISPLAY_HEIGHT / 2f;
    const float TARGET_SAMPLES_PER_BAR = 4801f;
    const float NOTE_LINE_WIDTH = 0.05f;
    const float HIT_NOTE_LENGTH = 0.1f;

    int beatsPerBar;
    float[] samples;
    float samplesPerBar;
    float samplesPerSecond;
    float offsetSamples;

    public Color[] laneColors = new Color[LoopDisplayHandler.LANE_COUNT];
    public Material lineMaterial;

    LineRenderer waveform;
    public UnityEngine.UI.Text[] beatNumbers = new UnityEngine.UI.Text[6];
    
    struct NoteLine
    {
        public LineRenderer duration;
        public List<LineRenderer> hits;
    }
    List<NoteLine> noteLines = new List<NoteLine>();

    void Awake()
    {
        waveform = transform.Find("Song Waveform").GetComponent<LineRenderer>();
    }

    void Start()
    {
        waveform.enabled = false;
    }

    public void Initialize(TestSong s)
    {
        beatsPerBar = s.beatsPerBar;

        float[] allSamples = new float[s.file.samples * s.file.channels];
        s.file.GetData(allSamples, 0);

        float fullSamplesPerBar = s.beatsPerBar * s.file.frequency * s.file.channels * SECONDS_PER_MINUTE / s.tempo;
        int divisor = (int)(fullSamplesPerBar / TARGET_SAMPLES_PER_BAR);

        List<float> selectedSamples = new List<float>();
        for (int i = 0; i < allSamples.Length; i += divisor)
            selectedSamples.Add(allSamples[i]);

        samples = selectedSamples.ToArray();
        samplesPerBar = fullSamplesPerBar / divisor;
        samplesPerSecond = s.file.frequency * s.file.channels / divisor;
    }

    public void Initialize(Song s)
    {
        beatsPerBar = s.beatsPerBar;

        AudioClip file = s.Clip;

        float[] allSamples = new float[file.samples * file.channels];
        s.Clip.GetData(allSamples, 0);

        float fullSamplesPerBar = s.beatsPerBar * file.frequency * file.channels * SECONDS_PER_MINUTE / s.tempo;
        int divisor = (int)(fullSamplesPerBar / TARGET_SAMPLES_PER_BAR);

        List<float> selectedSamples = new List<float>();
        for (int i = 0; i < allSamples.Length; i += divisor)
            selectedSamples.Add(allSamples[i]);

        samples = selectedSamples.ToArray();
        samplesPerBar = fullSamplesPerBar / divisor;
        samplesPerSecond = file.frequency * file.channels / divisor;
    }

    public void DisplayBar(List<Ref<Note>> notes)
    {
        UpdateBeatNumbers();
        UpdateWaveform();
        SpawnNoteLines(notes);
    }

    public void WaveformActive(bool state)
    {
        waveform.enabled = state;
    }

    public void SetOffset(float newOffset)
    {
        offsetSamples = newOffset * samplesPerSecond;
        UpdateWaveform();
    }

    int LeftBar { get => EditorManager.currentBar - 2; }
    int RightBar { get => EditorManager.currentBar + 3; }

    void UpdateBeatNumbers()
    {
        for (int i = 0; i < beatNumbers.Length; i++)
        {
            int bar = LeftBar + i;
            int beat = bar * beatsPerBar;
            beatNumbers[i].text = beat.ToString();
        }
    }

    void UpdateWaveform()
    {
        float[] displaySamples = new float[(int)(NUM_BARS * samplesPerBar)];

        int start = (int)(LeftBar * samplesPerBar - offsetSamples);
        int end = (int)(RightBar * samplesPerBar - offsetSamples) - 1;
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

        Vector3[] positions = new Vector3[displaySamples.Length];

        float unitLength = DISPLAY_WIDTH / displaySamples.Length;

        for (i = 0; i < displaySamples.Length; i++)
        {
            float x = LEFT + i * unitLength;
            float y = displaySamples[i] * HALF_HEIGHT;
            positions[i] = new Vector3(x, y, 0);
        }
        
        waveform.positionCount = displaySamples.Length;
        waveform.SetPositions(positions);
    }

    bool NoteInPhrase(float start, float stop)
    {
        float phraseStart = LeftBar * beatsPerBar;
        float phraseEnd = RightBar * beatsPerBar;
        return phraseStart <= stop && start < phraseEnd;
    }

    float BeatToX { get => DISPLAY_WIDTH / (beatsPerBar * NUM_BARS); }
    float BeatPosToX { get => BAR_WIDTH / beatsPerBar; }
    float LaneToY { get => DISPLAY_HEIGHT / (LoopDisplayHandler.LANE_COUNT + 1); }
    float CalcLaneY(int lane)
    {
        float disp = (lane + 1) * LaneToY;
        return BOTTOM + disp;
    }

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
                durationObj.transform.localPosition = new Vector3(LEFT, CalcLaneY(n.Value.lane));

                newNoteLine.duration = durationObj.AddComponent<LineRenderer>();
                GlobalManager.FormatLine(ref newNoteLine.duration, laneColors[n.Value.lane], lineMaterial, "Track", 150, NOTE_LINE_WIDTH);

                float leftBeat = LeftBar * beatsPerBar;
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
                    firstHit += BAR_WIDTH;
                for (float x = firstHit; x <= durationStop && x < DISPLAY_WIDTH; x += BAR_WIDTH)
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
