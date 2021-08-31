using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
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

    // TODO: Figure out stereo support?
    AudioSource song;
    Coroutine metCoroutine;
    LoopDisplayHandler loopDisplayHandler;

    public GameObject[] notePrefabs = new GameObject[LoopDisplayHandler.LANE_COUNT];

    public PlayManager()
    {
        for (int i = 0; i < LoopDisplayHandler.LANE_COUNT; i++)
            activeNotes[i] = new List<NoteHandler>();
    }

    void Awake()
    {
        loopDisplayHandler = FindObjectOfType<LoopDisplayHandler>();
    }

    void Start()
    {
        System.Array.Sort(hitRanges, (a, b) => a.margin.CompareTo(b.margin));
    }

    // TODO: Stop song
    
    public void LoadSong(Song s)
    {
        song = gameObject.AddComponent<AudioSource>();
        song.clip = s.Clip;

        beatsPerBar = s.beatsPerBar;
        tempo = s.tempo;
        songOffset = s.offset;

        foreach (Note n in s.track)
            notes.Add(n);

        loopDisplayHandler.Initialize(beatsPerBar);

        StartCoroutine(StartSong());
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
                activeNote.transform.position = loopDisplayHandler.CalcNotePosition(note.lane, note.beatPos);
                activeNote.transform.localScale = loopDisplayHandler.CalcNoteScale();

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

    // TODO: Pausing

    IEnumerator KeepTime()
    {
        beat = -BASE_SONG_WAIT * TimeToBeat;
        loopDisplayHandler.SetMetronome(CurrentBeatPos());

        yield return new WaitForEndOfFrame();

        while (true)
        {
            beat += Time.deltaTime * tempo / SECONDS_PER_MINUTE;

            loopDisplayHandler.SetMetronome(CurrentBeatPos());
            SpawnNotes();
            ClearNotes();

            yield return null;
        }
    }

    IEnumerator StartSong()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (song != null && beatsPerBar != 0 && tempo != 0)
        {
            StartCoroutine(PlaySong());
            metCoroutine = StartCoroutine(KeepTime());
        }
    }
}