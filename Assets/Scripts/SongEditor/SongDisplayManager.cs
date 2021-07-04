using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongDisplayManager : MonoBehaviour
{
    const float DISPLAY_WIDTH = 10f;
    const float DISPLAY_HEIGHT = 1.5f;

    LineRenderer waveform;

    void Awake()
    {
        waveform = transform.Find("Song Waveform").GetComponent<LineRenderer>();
    }

    public void UpdateWaveform(float[] samples)
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
}
