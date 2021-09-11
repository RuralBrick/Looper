using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    public const float BASE_SONG_WAIT = 3f;

    // TODO: Redo timing with absolute time

    float beat = 0;
    int beatsPerBar = 0;
    float tempo = 0;

    public static int score = 0;
    public static int combo = 0;
    public static int maxCombo = 0;

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
    Dictionary<HitRangeType, int> hitRangePoints = new Dictionary<HitRangeType, int>()
    {
        { HitRangeType.Perfect, 100 },
        { HitRangeType.Great, 25 }
    };

    List<Note> notes = new List<Note>();
    List<NoteHandler>[] activeNotes = new List<NoteHandler>[LoopDisplayHandler.LANE_COUNT];

    Coroutine metCoroutine;
    LoopDisplayHandler loopDisplayHandler;
    Text scoreText;
    Text comboText;
    HitSplashHandler perfectSplash;
    HitSplashHandler greatSplash;
    HitSplashHandler missSplash;
    HitSplashHandler earlySplash;
    HitSplashHandler lateSplash;

    int Score
    {
        get => score;
        set
        {
            score = value;
            scoreText.text = $"Score: {score}";
        }
    }

    int Combo
    {
        get => combo;
        set
        {
            combo = value;
            if (combo > maxCombo)
            {
                maxCombo = combo;
            }
            comboText.text = $"Combo: {combo}";
        }
    }

    public GameObject[] notePrefabs = new GameObject[LoopDisplayHandler.LANE_COUNT];

    public PlayManager()
    {
        for (int i = 0; i < LoopDisplayHandler.LANE_COUNT; i++)
            activeNotes[i] = new List<NoteHandler>();
    }

    void Awake()
    {
        loopDisplayHandler = FindObjectOfType<LoopDisplayHandler>();
        scoreText = GameObject.Find("Score Text").GetComponent<Text>();
        comboText = GameObject.Find("Combo Text").GetComponent<Text>();
        perfectSplash = GameObject.Find("Perfect Text").GetComponent<HitSplashHandler>();
        greatSplash = GameObject.Find("Great Text").GetComponent<HitSplashHandler>();
        missSplash = GameObject.Find("Miss Text").GetComponent<HitSplashHandler>();
        earlySplash = GameObject.Find("Early Text").GetComponent<HitSplashHandler>();
        lateSplash = GameObject.Find("Late Text").GetComponent<HitSplashHandler>();
    }

    void Start()
    {
        Array.Sort(hitRanges, (a, b) => a.margin.CompareTo(b.margin));
        Score = 0;
        Combo = 0;
        maxCombo = 0;
    }

    public void LoadSong(Song s)
    {
        beatsPerBar = s.beatsPerBar;
        tempo = s.tempo;

        foreach (Note n in s.track)
            notes.Add(n);

        loopDisplayHandler.Initialize(beatsPerBar);
        HitSplashHandler.Initialize(tempo);

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

    float HitBeat
    {
        get => beat - GlobalManager.instance.hitOffset * TimeToBeat;
    }

    void CheckMisses()
    {
        int misses = 0;
        foreach (List<NoteHandler> lane in activeNotes)
        {
            foreach (NoteHandler noteHandler in lane)
            {
                if (noteHandler.MissedNote(HitBeat, hitRanges[hitRanges.Length - 1].margin))
                {
                    misses++;
                }
            }
        }
        if (misses > 0)
        {
            Combo = 0;
            missSplash.Splash();
        }
    }

    void ClearNotes()
    {
        foreach (List<NoteHandler> lane in activeNotes)
        {
            foreach (NoteHandler nh in lane)
            {
                if (nh.ShouldDespawn(beat))
                {
                    nh.Disappear(delegate
                    { 
                        if (!lane.Remove(nh))
                            Debug.LogWarning("Could not remove note handler");
                        CheckEnd();
                    });
                }
            }
        }
    }

    void CheckEnd()
    {
        if (notes.Count == 0 && Array.TrueForAll(activeNotes, lane => lane.Count == 0))
        {
            GlobalManager.instance.SaveScore(score);
            GlobalManager.ChangeScene("ResultsScene");
        }
    }

    float TimeToBeat
    {
        get => tempo / SECONDS_PER_MINUTE;
    }

    float PointsMultiplier()
    {
        return (100 + combo) / 100f;
    }

    // TODO: Add hit sfx

    public void CheckLane(int lane)
    {
        foreach (NoteHandler nh in activeNotes[lane])
        {
            HitRangeType hitType;
            bool late;
            if (nh.GetHit(HitBeat , hitRanges, out hitType, out late))
            {
                switch (hitType)
                {
                    case HitRangeType.Perfect:
                        Score += (int)(hitRangePoints[HitRangeType.Perfect] * PointsMultiplier());
                        Combo++;
                        perfectSplash.Splash();
                        break;
                    case HitRangeType.Great:
                        Score += (int)(hitRangePoints[HitRangeType.Great] * PointsMultiplier());
                        Combo++;
                        greatSplash.Splash();
                        if (late)
                            lateSplash.Splash();
                        else
                            earlySplash.Splash();
                        break;
                }
            }
        }
    }

    float MetBeatPos
    {
        get => (beat - GlobalManager.instance.hitOffset * TimeToBeat) % beatsPerBar;
    }

    // TODO: Pausing

    IEnumerator KeepTime()
    {
        beat = -BASE_SONG_WAIT * TimeToBeat;
        loopDisplayHandler.SetMetronome(MetBeatPos);

        yield return new WaitForEndOfFrame();

        while (true)
        {
            beat += Time.deltaTime * TimeToBeat;

            loopDisplayHandler.SetMetronome(MetBeatPos);
            SpawnNotes();
            CheckMisses();
            ClearNotes();

            yield return null;
        }
    }

    IEnumerator StartSong()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (beatsPerBar != 0 && tempo != 0)
        {
            GlobalManager.instance.PlaySong();
            metCoroutine = StartCoroutine(KeepTime());
        }
    }
}
