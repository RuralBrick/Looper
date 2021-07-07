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

    float[] samples;
    float samplesPerBar;
    float samplesPerSecond;
    float offsetSamples;

    public Color lineColor;
    public Material lineMaterial;

    LineRenderer waveform;
    GameObject window;

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

    void MakeBarLines()
    {
        float left = -(DISPLAY_WIDTH / 2f);
        float barWidth = DISPLAY_WIDTH / BarsDisplayed();

        float top = DISPLAY_HEIGHT / 2f;
        float bottom = -(DISPLAY_HEIGHT / 2f);

        for (int i = 1; i < BarsDisplayed(); i++)
        {
            GameObject line = new GameObject("Bar Line " + i);
            line.transform.parent = transform;
            line.transform.localPosition = Vector3.zero;
            line.transform.localScale = Vector3.one;

            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.startColor = lr.endColor = lineColor;
            lr.material = lineMaterial;
            lr.sortingLayerName = "Track";
            lr.sortingOrder = 105;
            lr.startWidth = lr.endWidth = BAR_LINE_WIDTH;
            lr.useWorldSpace = false;

            float x = left + i * barWidth;
            lr.SetPositions(new Vector3[] { new Vector3(x, top, 0), new Vector3(x, bottom, 0) });
        }
    }

    public void Initialize(Song s)
    {
        samples = new float[s.file.samples * s.file.channels];
        s.file.GetData(samples, 0);

        samplesPerBar = s.beatsPerBar * s.file.frequency * SECONDS_PER_MINUTE / s.tempo;
        samplesPerSecond = s.file.frequency;
    }

    void UpdateWaveform(float[] samples)
    {
        Vector3[] positions = new Vector3[samples.Length];
        
        float left = -(DISPLAY_WIDTH / 2f);
        float halfHeight = DISPLAY_HEIGHT / 2f;
        float unitLength = DISPLAY_WIDTH / samples.Length;
        for (int i = 0; i < samples.Length; i++)
        {
            float x = left + i * unitLength;
            float y = samples[i] * halfHeight;
            positions[i] = new Vector3(x, y, 0);
        }
        
        waveform.positionCount = samples.Length;
        waveform.SetPositions(positions);
    }

    public void DisplayBar(int bar)
    {
        float[] displaySamples = new float[(int)(BarsDisplayed() * samplesPerBar)];

        int startBar = bar - NUM_BUFFER_BARS;
        int start = (int)(startBar * samplesPerBar - offsetSamples);
        int endBar = bar + NUM_BUFFER_BARS + 1;
        int end = (int)(endBar * samplesPerBar - offsetSamples) - 1;
        end = end < samples.Length ? end : samples.Length;

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

    public void SetOffset(int bar, float newOffset)
    {
        offsetSamples = newOffset * samplesPerSecond;
        DisplayBar(bar);
    }
}
