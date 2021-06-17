using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const int LANE_COUNT = 4;
    const float LANE_WIDTH = 0.75f;
    const float NOTE_WIDTH = 0.65f;

    float beat = 0;
    int beatsPerBar = 0;
    float tempo = 0;

    List<Note> notes = new List<Note>();
    List<NoteHandler>[] activeNotes = new List<NoteHandler>[LANE_COUNT];

    AudioSource song;
    Coroutine songCoroutine;
    LoopDisplayManager ldm;

    public GameObject[] notePrefabs = new GameObject[LANE_COUNT];

    public SongManager()
    {
        for (int i = 0; i < LANE_COUNT; i++)
            activeNotes[i] = new List<NoteHandler>();
    }

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayManager>();
    }

    public void LoadSong(Song s)
    {
        song = gameObject.AddComponent<AudioSource>();
        song.clip = s.file;

        beatsPerBar = s.beatsPerBar;
        tempo = s.tempo;

        ldm.Initialize(beatsPerBar, LANE_COUNT, LANE_WIDTH);

        Note testNote = new Note();
        testNote.lane = 0;
        testNote.beatPos = 0f;
        testNote.start = 8f;
        testNote.stop = 12f;

        notes.Add(testNote);

        StartSong();
    }

    public void StartSong()
    {
        if (song != null && beatsPerBar != 0 && tempo != 0)
        {
            songCoroutine = StartCoroutine(PlaySong());
        }
    }
    
    void SpawnNotes()
    {
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            Note note = notes[i];
            if ((note.start - beat) < beatsPerBar)
            {
                GameObject notePrefab = notePrefabs[note.lane];
                
                GameObject activeNote = Instantiate(notePrefab);
                Vector3 pos = ldm.CalcNotePosition(note.lane, note.beatPos);
                activeNote.transform.position = pos;
                activeNote.transform.localScale = new Vector3(NOTE_WIDTH, NOTE_WIDTH, 1f);

                NoteHandler nh = activeNote.GetComponent<NoteHandler>();
                nh.beatPos = note.beatPos;
                nh.end = note.stop;

                activeNotes[note.lane].Add(nh);
                notes.RemoveAt(i);
            }
        }
    }
    
    IEnumerator PlaySong()
    {
        beat = GlobalManager.instance.calibration * tempo / SECONDS_PER_MINUTE;
        song.Play();
        while (true)
        {
            beat += Time.deltaTime * tempo / SECONDS_PER_MINUTE;

            ldm.SetMetronome(beat % beatsPerBar);
            SpawnNotes();

            yield return null;
        }
    }
}
