using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const float BASE_SONG_WAIT = 3f;

    float beat = 0;
    int beatsPerBar = 0;
    float tempo = 0;
    float songOffset = 0f;

    public enum HitRangeType { None, Perfect, Great }
    public struct HitRange
    {
        public float margin;
        public HitRangeType type;
    }
    HitRange[] hitRanges =
    {
        new HitRange{ margin = 0.075f, type = HitRangeType.Perfect },
        new HitRange{ margin = 0.25f, type = HitRangeType.Great }
    };

    List<Note> notes = new List<Note>();
    List<NoteHandler>[] activeNotes = new List<NoteHandler>[LoopDisplayHandler.LANE_COUNT];

    AudioSource song;
    Coroutine metCoroutine;
    LoopDisplayHandler ldm;

    public GameObject[] notePrefabs = new GameObject[LoopDisplayHandler.LANE_COUNT];

    public SongManager()
    {
        for (int i = 0; i < LoopDisplayHandler.LANE_COUNT; i++)
            activeNotes[i] = new List<NoteHandler>();
    }

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayHandler>();
    }

    void Start()
    {
        System.Array.Sort(hitRanges, (a, b) => a.margin.CompareTo(b.margin));
    }

    public void LoadSong(Song s)
    {
        song = gameObject.AddComponent<AudioSource>();
        song.clip = s.file;

        beatsPerBar = s.beatsPerBar;
        tempo = s.tempo;
        songOffset = s.offset;
        
        foreach (Note n in s.Track)
            notes.Add(n);
        
        ldm.Initialize(beatsPerBar);

        StartSong();
    }

    void SpawnNotes()
    {
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            Note note = notes[i];
            if (NoteHandler.ShouldSpawn(beat, note.start))
            {
                GameObject notePrefab = notePrefabs[note.lane];
                
                GameObject activeNote = Instantiate(notePrefab);
                activeNote.transform.position = ldm.CalcNotePosition(note.lane, note.beatPos);
                activeNote.transform.localScale = ldm.CalcNoteScale();

                NoteHandler nh = activeNote.GetComponent<NoteHandler>();
                nh.Initialize(note, beatsPerBar, tempo);

                activeNotes[note.lane].Add(nh);
                notes.RemoveAt(i);
            }
        }
    }

    void ClearNotes()
    {
        foreach (List<NoteHandler> lane in activeNotes)
            foreach (NoteHandler nh in lane)
                if (nh.ShouldDespawn(beat))
                    nh.Disappear(lane.Remove);
    }

    float TimeToBeat
    {
        get => tempo / SECONDS_PER_MINUTE;
    }

    float HitBeatOffset
    {
        get => GlobalManager.instance.hitOffset * TimeToBeat;
    }

    public void CheckLane(int lane)
    {
        foreach (NoteHandler nh in activeNotes[lane])
        {
            HitRangeType hitType;
            bool late;
            float hitBeat = beat + HitBeatOffset;
            if (nh.GetHit(hitBeat , hitRanges, out hitType, out late))
            {
                switch (hitType)
                {
                    case HitRangeType.Perfect:
                        Debug.Log("Perfect");
                        break;
                    case HitRangeType.Great:
                        Debug.Log("Great, " + (late ? "Late" : "Early"));
                        break;
                }
            }
        }
    }

    IEnumerator PlaySong()
    {
        float wait = BASE_SONG_WAIT + songOffset - GlobalManager.instance.syncOffset;
        yield return new WaitForSeconds(wait);
        song.Play();
    }

    float CurrentBeatPos()
    {
        return beat % beatsPerBar;
    }

    IEnumerator KeepTime()
    {
        beat = -BASE_SONG_WAIT * TimeToBeat;
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

    void StartSong()
    {
        if (song != null && beatsPerBar != 0 && tempo != 0)
        {
            StartCoroutine(PlaySong());
            metCoroutine = StartCoroutine(KeepTime());
        }
    }
}
