using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationManager : MonoBehaviour
{
    const float BEATS_PER_SECOND = 1f;
    const int BEATS_PER_BAR = 4;
    const float SECONDS_PER_BEAT = 1f / BEATS_PER_SECOND;

    LoopDisplayHandler loopDisplayHandler;

    float startTime;
    float syncOffset = 0;
    float hitOffset = 0;
    List<float> offsetData;

    float SyncOffset
    {
        get => LoopDisplayHandler.clockwiseMetronome ? -1 * syncOffset : syncOffset;
    }

    bool syncConfirmed = true;//false;

    Text instructionsText;
    Slider syncSlider;

    string[] instructions =
    {
        "Adjust the slider and refine with the arrow buttons until the metronome line lines up with the downbeat line when the loudest beat plays.",
        "Tap any of the lane keys to the beat."
    };

    void Awake()
    {
        loopDisplayHandler = FindObjectOfType<LoopDisplayHandler>();
        instructionsText = GameObject.Find("Instructions Text").transform.GetComponent<Text>();
        //syncSlider = GameObject.Find("Sync Slider").transform.GetComponent<Slider>();
    }

    // TODO: Add option to use separate sync and hit offsets

    // TODO: Add manual calibration

    void Start()
    {
        //syncConfirmed = false;

        loopDisplayHandler.Initialize(4);
        instructionsText.text = instructions[1];//[0];

        offsetData = new List<float>();

        StartCoroutine(KeepTime());
    }

    float CurrentTime
    {
        get => Time.time - startTime;
    }

    float MetBeatPos()
    {
        float adjustedTime = CurrentTime - SyncOffset;
        float currentBeat = adjustedTime * BEATS_PER_SECOND;
        return currentBeat % BEATS_PER_BAR;
    }

    IEnumerator KeepTime()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        startTime = Time.time;
        loopDisplayHandler.SetMetronome(MetBeatPos());
        GlobalManager.instance.StartMetronome();

        yield return new WaitForEndOfFrame();

        while (true)
        {
            loopDisplayHandler.SetMetronome(MetBeatPos());

            yield return null;
        }
    }

    public void SetSync(float offset)
    {
        syncOffset = offset;
    }

    public void IncSync()
    {
        syncOffset += 0.01f;
        syncSlider.value = syncOffset;
    }

    public void DecSync()
    {
        syncOffset -= 0.01f;
        syncSlider.value = syncOffset;
    }

    void ConfirmSync()
    {
        Debug.Log("Sync: " + SyncOffset);
        GameObject.Find("Sync Controls").SetActive(false);

        instructionsText.text = instructions[1];

        offsetData = new List<float>();
        syncConfirmed = true;
    }

    float CalcHitOffset()
    {
        if (offsetData == null)
            return 0f;

        float sum = 0f;
        foreach (float o in offsetData)
            sum += o;
        return sum / offsetData.Count;
    }

    void ConfirmCalibration()
    {
        hitOffset = CalcHitOffset();

        Debug.Log("Hits: " + hitOffset);

        //GlobalManager.SetSyncOffset(SyncOffset);
        GlobalManager.SetHitOffset(hitOffset);

        Debug.Log("Calibration finished");

        GlobalManager.instance.StopMetronome();
        GlobalManager.ChangeScene("TitleScene");
    }

    public void Confirm()
    {
        if (!syncConfirmed)
            ConfirmSync();
        else
            ConfirmCalibration();
    }

    public void Hit(int lane)
    {
        const float halfBeat = SECONDS_PER_BEAT / 2f;

        if (syncConfirmed)
        {
            float halfForwardShift = CurrentTime + halfBeat;
            float placeInNoteRange = halfForwardShift % SECONDS_PER_BEAT;
            float offset = placeInNoteRange - halfBeat;

            Debug.Log($"Offset: {offset}");

            offsetData.Add(offset);
        }
    }
}
