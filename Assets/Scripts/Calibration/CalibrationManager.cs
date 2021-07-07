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
        syncOffset = 0f;
        hitOffset = 0f;
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

    public void ConfirmSync()
    {
        Debug.Log(syncOffset);
        GameObject.Find("Sync Controls").SetActive(false);

        instructionsText.text = instructions[1];

        syncConfirmed = true;
    }

    public void Hit(int lane)
    {
        const float halfBeat = SECONDS_PER_BEAT / 2f;

        if (syncConfirmed)
        {
            float adjustedTime = currentTime - syncOffset;

            float plusCurrent = currentTime + halfBeat;
            float plusAdjusted = adjustedTime + halfBeat;
            float modCurrent = plusCurrent % SECONDS_PER_BEAT;
            float modAjusted = plusAdjusted % SECONDS_PER_BEAT;
            float offset1 = modCurrent - halfBeat;
            float offset2 = modAjusted - halfBeat;

            Debug.LogFormat($"Offset 1: {offset1}\n" +
                $"Offset 2: {offset2}");
        }
    }
}
