using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;

    float beat = 0;
    int beatsPerBar = 0;
    float tempo = 0;

    List<Note> notes;
    //List<>

    AudioSource song;
    Coroutine songCoroutine;
    LoopDisplayManager ldm;

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayManager>();
    }

    void Start()
    {
        ldm.MakeLaneLines(0.75f);
    }

    float CurrentBarPercent()
    {
        return (beat % beatsPerBar) / beatsPerBar;
    }

    public void LoadSong(Song s)
    {
        song = gameObject.AddComponent<AudioSource>();
        song.clip = s.file;

        beatsPerBar = s.beatsPerBar;
        tempo = s.tempo;

        ldm.MakeBeatLines(beatsPerBar);

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
        beat = GlobalManager.instance.calibration * tempo / SECONDS_PER_MINUTE;
        song.Play();
        while (true)
        {
            beat += Time.deltaTime * tempo / SECONDS_PER_MINUTE;
            ldm.SetMetronome(CurrentBarPercent());
            yield return null;
        }
    }
}
