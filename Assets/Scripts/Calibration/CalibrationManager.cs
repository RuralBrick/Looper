using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationManager : MonoBehaviour
{
    const float BEATS_PER_SECOND = 2f;
    const int BEATS_PER_BAR = 4;
    const float SECONDS_PER_BEAT = 0.5f;

    LoopDisplayHandler ldm;

    float currentTime;
    float syncOffset;
    float hitOffset;
    List<float> offsetData;

    bool syncConfirmed = false;

    Text instructionsText;
    Slider syncSlider;

    string[] instructions =
    {
        "Adjust the slider and refine with the arrow buttons until the metronome line lines up with the downbeat line when the loudest beat plays.",
        "Tap any of the lane keys to the beat."
    };

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayHandler>();
        instructionsText = GameObject.Find("Instructions Text").transform.GetComponent<Text>();
        syncSlider = GameObject.Find("Sync Slider").transform.GetComponent<Slider>();
    }

    void Start()
    {
        syncOffset = GlobalManager.instance.syncOffset;
        hitOffset = GlobalManager.instance.hitOffset;
        syncConfirmed = false;

        ldm.Initialize(4);
        instructionsText.text = instructions[0];

        GlobalManager.instance.StartMetronome();
        StartCoroutine(KeepTime());
    }

    float CurrentBeatPos()
    {
        float adjustedTime = currentTime - syncOffset;
        float currentBeat = adjustedTime * BEATS_PER_SECOND;
        return currentBeat % BEATS_PER_BAR;
    }

    IEnumerator KeepTime()
    {
        currentTime = 0f;
        ldm.SetMetronome(CurrentBeatPos());

        yield return new WaitForEndOfFrame();

        while (true)
        {
            currentTime += Time.deltaTime;

            ldm.SetMetronome(CurrentBeatPos());

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
        Debug.Log("Sync: " + syncOffset);
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

        GlobalManager.instance.syncOffset = syncOffset;
        GlobalManager.instance.hitOffset = hitOffset;

        Debug.Log("Calibration finished");

        // TODO: Switch scene
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
            float adjustedTime = currentTime - syncOffset;
            float plusAdjusted = adjustedTime + halfBeat;
            float modAjusted = plusAdjusted % SECONDS_PER_BEAT;
            float offset = modAjusted - halfBeat;

            offsetData.Add(offset);
        }
    }
}
