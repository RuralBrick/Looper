using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;

    float beat = 0;
    int beatsPerBar = 0;
    float tempo = 0;

    AudioSource song;
    Coroutine songCoroutine;
    LoopDisplayManager ldm;

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayManager>();
    }

    float CurrentBarPercent()
    {
        return (beat % beatsPerBar) / beatsPerBar;
    }

    public void LoadSong(AudioClip file, int beatsPerBar, float tempo)
    {
        song = gameObject.AddComponent<AudioSource>();
        song.clip = file;

        this.beatsPerBar = beatsPerBar;
        this.tempo = tempo;

        StartSong();
    }

    public void StartSong()
    {
        if (song != null && beatsPerBar != 0 && tempo != 0)
        {
            songCoroutine = StartCoroutine(PlaySong());
        }
    }

    IEnumerator PlaySong()
    {
        beat = 0;
        song.Play();
        while (true)
        {
            beat += Time.deltaTime * tempo / SECONDS_PER_MINUTE;
            ldm.SetMetronome(CurrentBarPercent());
            yield return null;
        }
    }
}
