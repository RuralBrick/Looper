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

    int beatsPerBar;
    float[] samples;
    float samplesPerBar;
    float samplesPerSecond;
    float offsetSamples;

    public Color[] laneColors = new Color[LoopDisplayHandler.LANE_COUNT];
    public GameObject noteLinePrefab;

    LineRenderer waveform;
    public UnityEngine.UI.Text[] beatNumbers = new UnityEngine.UI.Text[6];

    void Awake()
    {
        waveform = transform.Find("Song Waveform").GetComponent<LineRenderer>();
    }

    void Start()
    {
        waveform.enabled = false;
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
    float CalcDurationPosition(float beat)
    {
        float leftBeat = LeftBar * beatsPerBar;
        float beatOffsetFromLeft = beat - leftBeat;
        float offsetFromLeft = beatOffsetFromLeft * BeatToX;
        return Mathf.Clamp(offsetFromLeft, 0f, DISPLAY_WIDTH);
    }

    public void SpawnNoteLines(List<Ref<Note>> notes)
    {
        NoteLineHandler.DestroyNoteLines();

        foreach (Ref<Note> n in notes)
        {
            if (NoteInPhrase(n.Value.start, n.Value.stop))
            {
                float durationStart = CalcDurationPosition(n.Value.start);
                float durationStop = CalcDurationPosition(n.Value.stop);

                Color hitColor = Color.HSVToRGB(n.Value.beatPos / beatsPerBar, 1f, 1f);
                
                float firstHit = n.Value.beatPos * BeatPosToX;
                while (firstHit < DISPLAY_WIDTH && firstHit < durationStart)
                    firstHit += BAR_WIDTH;

                NoteLineHandler noteLine = Instantiate(noteLinePrefab, transform.position + new Vector3(LEFT, CalcLaneY(n.Value.lane)), Quaternion.identity, transform).GetComponent<NoteLineHandler>();
                noteLine.SetColor(laneColors[n.Value.lane]);
                noteLine.SetPositions(durationStart, durationStop);
                noteLine.SpawnHitMarks(hitColor, firstHit, durationStop, DISPLAY_WIDTH, BAR_WIDTH);
            }
        }
    }
}
