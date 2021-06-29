using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const int LANE_COUNT = 4;
    const float LANE_WIDTH = 0.75f;
    const float NOTE_WIDTH = 0.65f;
    const float BASE_SONG_WAIT = 3f;
    const float PERFECT_MARGIN = 0.075f;
    const float GREAT_MARGIN = 0.25f;

    float beat = 0;
    int beatsPerBar = 0;
    float tempo = 0;
    float songOffset = 0f;

    List<Note> notes = new List<Note>();
    List<NoteHandler>[] activeNotes = new List<NoteHandler>[LANE_COUNT];

    AudioSource song;
    Coroutine metCoroutine;
    LoopDisplayHandler ldm;

    public GameObject[] notePrefabs = new GameObject[LANE_COUNT];

    public SongManager()
    {
        for (int i = 0; i < LANE_COUNT; i++)
            activeNotes[i] = new List<NoteHandler>();
    }

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayHandler>();
    }

    public void LoadSong(Song s)
    {
        song = gameObject.AddComponent<AudioSource>();
        song.clip = s.file;

        beatsPerBar = s.beatsPerBar;
        tempo = s.tempo;
        songOffset = s.offset;

        ldm.Initialize(beatsPerBar, LANE_COUNT, LANE_WIDTH);

        Note testNote = new Note();
        testNote.lane = 0;
        testNote.beatPos = 0f;
        testNote.start = 0f;
        testNote.stop = 16f;

        notes.Add(testNote);

        testNote.beatPos = 1f;
        notes.Add(testNote);

        testNote.beatPos = 2f;
        notes.Add(testNote);

        testNote.beatPos = 3f;
        notes.Add(testNote);

        StartSong();
    }

    public void StartSong()
    {
        if (song != null && beatsPerBar != 0 && tempo != 0)
        {
            StartCoroutine(PlaySong());
            metCoroutine = StartCoroutine(KeepTime());
        }
    }

    float CurrentBeatPos()
    {
        return beat % beatsPerBar;
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

    void ClearNotes()
    {
        foreach (List<NoteHandler> lane in activeNotes)
        {
            for (int i = lane.Count - 1; i >= 0; i--)
            {
                NoteHandler nh = lane[i];
                if (beat > nh.end)
                {
                    nh.Disappear();
                    lane.RemoveAt(i);
                }
            }
        }
    }

    public void CheckLane(int lane)
    {
        foreach (NoteHandler nh in activeNotes[lane])
        {
            float diff = CurrentBeatPos() - nh.beatPos;
            bool late = diff > 0;
            float dist = Mathf.Abs(diff);

            if (dist <= PERFECT_MARGIN)
            {
                nh.GetHit();
                Debug.Log("Perfect");
            }
            else if (dist <= GREAT_MARGIN)
            {
                nh.GetHit();
                Debug.Log("Great");
                Debug.Log(late ? "Late" : "Not late");
            }
        }
    }

    IEnumerator PlaySong()
    {
        float wait = BASE_SONG_WAIT + songOffset;
        yield return new WaitForSeconds(wait);
        song.Play();
    }
    
    IEnumerator KeepTime()
    {
        float offset = BASE_SONG_WAIT + GlobalManager.instance.calibration;
        beat = -offset * tempo / SECONDS_PER_MINUTE;
        ldm.SetMetronome(CurrentBeatPos());

        yield return new WaitForEndOfFrame();

        while (true)
        {
            beat += Time.deltaTime * tempo / SECONDS_PER_MINUTE;

            ldm.SetMetronome(CurrentBeatPos());
            SpawnNotes();
            ClearNotes();

            yield return null;
        }
    }
}
