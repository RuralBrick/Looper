using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const int LANE_COUNT = 4;
    const float LANE_WIDTH = 0.75f;
    const int NUM_BUFFER_BARS = 2;

    float[] samples;
    float samplesPerBar;

    int currentBar;

    LoopDisplayHandler ldm;
    SongDisplayManager sdm;

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayHandler>();
        sdm = FindObjectOfType<SongDisplayManager>();
    }

    void Start()
    {
        ldm.HideMetronome();
    }

    public void LoadSong(Song s)
    {
        samples = new float[s.file.samples * s.file.channels];
        s.file.GetData(samples, 0);

        samplesPerBar = s.beatsPerBar * s.file.frequency * SECONDS_PER_MINUTE / s.tempo;

        ldm.Initialize(s.beatsPerBar, LANE_COUNT, LANE_WIDTH);

        currentBar = 0;
        DisplayCurrentBar();
    }

    int PhraseBarCount()
    {
        return 2 * NUM_BUFFER_BARS + 1;
    }

    void DisplayCurrentBar()
    {
        float[] displaySamples = new float[(int)(PhraseBarCount() * samplesPerBar)];

        int startBar = currentBar - NUM_BUFFER_BARS;
        int start = (int)(startBar * samplesPerBar);
        int endBar = currentBar + NUM_BUFFER_BARS + 1;
        int end = (int)(endBar * samplesPerBar) - 1;
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

        sdm.UpdateWaveform(displaySamples);
    }

    public void DisplayNextBar()
    {
        currentBar++;
        DisplayCurrentBar();
    }

    public void DisplayPrevBar()
    {
        currentBar--;
        if (currentBar < 0) currentBar = 0;
        DisplayCurrentBar();
    }

    public void DisplayNextPhrase()
    {
        currentBar += PhraseBarCount();
        DisplayCurrentBar();
    }

    public void DisplayPrevPhrase()
    {
        currentBar -= PhraseBarCount();
        if (currentBar < 0) currentBar = 0;
        DisplayCurrentBar();
    }
}
