using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ref<T> where T : struct
{
    public T Value { get; set; }
}

public class EditorManager : MonoBehaviour
{
    Song currentSong;
    public static int currentBar;

    List<Ref<Note>> notes = new List<Ref<Note>>();

    [Serializable]
    public struct NoteButton
    {
        public int noteVal;
        public Button noteButton;
    }
    NoteButton currentNoteVal;
    bool usingTriplet;

    struct SelectedNote
    {
        public Ref<Note> noteRef;
        public EditNoteHandler enh;
    }
    SelectedNote selectedNote;
    List<EditNoteHandler> barNotes = new List<EditNoteHandler>();
    
    LoopDisplayHandler ldm;
    SongDisplayManager sdm;

    InputField offsetField;
    Slider offsetSlider;
    NoteInfoHandler nih;
    List<PlaceholderHandler> placeholders = new List<PlaceholderHandler>();

    public GameObject[] placeholderPrefabs = new GameObject[LoopDisplayHandler.LANE_COUNT];
    public GameObject[] editNotePrefabs = new GameObject[LoopDisplayHandler.LANE_COUNT];
    public NoteButton[] noteValButtons;

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayHandler>();
        sdm = FindObjectOfType<SongDisplayManager>();
        offsetField = GameObject.Find("Offset Field").transform.GetComponent<InputField>();
        offsetSlider = GameObject.Find("Offset Slider").transform.GetComponent<Slider>();
        nih = FindObjectOfType<NoteInfoHandler>();
    }

    void Start()
    {
        ldm.HideMetronome();
        nih.Hide();
        SelectNoteVal(4);
    }

    int CurrentBar
    {
        get => currentBar;
        set
        {
            if (value < 0) currentBar = 0;
            else currentBar = value;

            if (selectedNote.noteRef != null &&
                !NoteInBar(selectedNote.noteRef.Value.start, selectedNote.noteRef.Value.stop))
                DeselectNote();
            SpawnEditNotes();
            sdm.SpawnNoteLines(notes);

            sdm.DisplayBar();
        }
    }

    public void LoadSong(Song s)
    {
        currentSong = s;
        currentSong.beatsPerBar = s.beatsPerBar;
        currentSong.beatUnit = s.beatUnit;

        foreach (Note n in GlobalManager.instance.LoadTrack(s.trackFile))
        {
            Ref<Note> newNoteRef = new Ref<Note>();
            newNoteRef.Value = n;
            notes.Add(newNoteRef);
        }

        ldm.Initialize(s.beatsPerBar);
        sdm.Initialize(s);

        GameObject.Find("Title Field").transform.GetComponent<InputField>().text = s.title;
        offsetField.text = s.offset.ToString();
        offsetSlider.value = s.offset;

        CurrentBar = 0;
    }

    public void SaveSong()
    {
        Note[] track = new Note[notes.Count];

        for (int i = 0; i < notes.Count; i++)
            track[i] = notes[i].Value;

        GlobalManager.instance.SaveSong(currentSong, track);
    }

    public void DisplayNextBar()
    {
        CurrentBar++;
    }

    public void DisplayPrevBar()
    {
        CurrentBar--;
    }

    public void DisplayNextPhrase()
    {
        CurrentBar += SongDisplayManager.BarsDisplayed();
    }

    public void DisplayPrevPhrase()
    {
        CurrentBar -= SongDisplayManager.BarsDisplayed(); 
    }

    public void SetTitle(string title)
    {
        currentSong.title = title;
    }

    public void SetOffset(string textOffset)
    {
        float newOffset;
        if (float.TryParse(textOffset, out newOffset))
        {
            currentSong.offset = newOffset;
            offsetSlider.value = newOffset;
            sdm.SetOffset(newOffset);
        }
    }

    public void SetOffset(float newOffset)
    {
        currentSong.offset = newOffset;
        offsetField.text = newOffset.ToString();
        sdm.SetOffset(newOffset);
    }

    public void SetTrackName(string trackName)
    {
        currentSong.trackFile = trackName;
    }

    void SpawnEditNotes()
    {
        for (int i = barNotes.Count - 1; i >= 0; i--)
        {
            Destroy(barNotes[i].gameObject);
            barNotes.RemoveAt(i);
        }

        foreach (Ref<Note> n in notes)
        {
            if (NoteInBar(n.Value.start, n.Value.stop))
            {
                GameObject newEditNote = Instantiate(editNotePrefabs[n.Value.lane]);
                newEditNote.transform.position = ldm.CalcNotePosition(n.Value.lane, n.Value.beatPos);
                newEditNote.transform.localScale = ldm.CalcNoteScale();

                EditNoteHandler enh = newEditNote.GetComponent<EditNoteHandler>();
                enh.em = this;
                enh.info = n;

                barNotes.Add(enh);
            }
        }
    }

    bool NoteInBar(float start, float stop)
    {
        float barStart = currentBar * currentSong.beatsPerBar;
        float barEnd = barStart + currentSong.beatsPerBar;
        return (barStart < stop - Mathf.Epsilon) && (start < barEnd - Mathf.Epsilon);
    }

    public void AddNote(int lane, float beatPos)
    {
        foreach (Ref<Note> n in notes)
        {
            if (lane == n.Value.lane && 
                Mathf.Approximately(beatPos, n.Value.beatPos) && 
                NoteInBar(n.Value.start, n.Value.stop))
                return;
        }

        Note newNote = new Note();
        newNote.lane = lane;
        newNote.beatPos = beatPos;
        newNote.start = currentBar * currentSong.beatsPerBar;
        newNote.stop = newNote.start + currentSong.beatsPerBar;

        Ref<Note> noteRef = new Ref<Note>();
        noteRef.Value = newNote;

        notes.Add(noteRef);

        SpawnEditNotes();
        SelectNote(barNotes.Find(enh => enh.info == noteRef));
        sdm.SpawnNoteLines(notes);
    }

    public void SelectNote(EditNoteHandler enh)
    {
        if (selectedNote.enh != null) selectedNote.enh.Deselect();
        enh.Select();
        selectedNote.noteRef = enh.info;
        selectedNote.enh = enh;

        nih.SetStart(enh.info.Value.start);
        nih.SetStop(enh.info.Value.stop);

        nih.Show();
    }

    public void DeselectNote()
    {
        nih.Hide();

        if (selectedNote.enh != null) selectedNote.enh.Deselect();

        selectedNote.noteRef = null;
        selectedNote.enh = null;
    }

    public void SetSelectedNoteStart(string startText)
    {
        float start;
        if (float.TryParse(startText, out start))
        {
            if (start < 0 || start >= selectedNote.noteRef.Value.stop - Mathf.Epsilon)
            {
                nih.SetStart(selectedNote.noteRef.Value.start);
                return;
            }

            Note info = selectedNote.noteRef.Value;
            info.start = start;
            selectedNote.noteRef.Value = info;

            if (!NoteInBar(info.start, info.stop))
                DeselectNote();

            SpawnEditNotes();
            sdm.SpawnNoteLines(notes);
        }
    }

    public void SetSelectedNoteStop(string stopText)
    {
        float stop;
        if (float.TryParse(stopText, out stop))
        {
            if (stop < 0 || stop <= selectedNote.noteRef.Value.start + Mathf.Epsilon)
            {
                nih.SetStop(selectedNote.noteRef.Value.stop);
                return;
            }

            Note info = selectedNote.noteRef.Value;
            info.stop = stop;
            selectedNote.noteRef.Value = info;

            if (!NoteInBar(info.start, info.stop))
                DeselectNote();

            SpawnEditNotes();
            sdm.SpawnNoteLines(notes);
        }
    }

    public void RemoveSelectedNote()
    {
        nih.Hide();

        notes.Remove(selectedNote.noteRef);

        selectedNote.noteRef = null;
        selectedNote.enh = null;

        SpawnEditNotes();
        sdm.SpawnNoteLines(notes);
    }

    void SpawnPlaceholders()
    {
        for (int i = placeholders.Count - 1; i >= 0; i--)
        {
            Destroy(placeholders[i].gameObject);
            placeholders.RemoveAt(i);
        }

        if (currentNoteVal.noteVal == 0 || currentNoteVal.noteButton == null)
            return;

        float noteLength = (float)currentSong.beatUnit / currentNoteVal.noteVal;
        noteLength = usingTriplet ? noteLength * 2f / 3f : noteLength;

        for(int lane = 0; lane < LoopDisplayHandler.LANE_COUNT; lane++)
            for (float beat = 0; beat < currentSong.beatsPerBar; beat += noteLength)
            {
                GameObject newPlaceholder = Instantiate(placeholderPrefabs[lane]);
                newPlaceholder.transform.position = ldm.CalcNotePosition(lane, beat);
                newPlaceholder.transform.localScale = ldm.CalcNoteScale();

                PlaceholderHandler ph = newPlaceholder.GetComponent<PlaceholderHandler>();
                ph.em = this;
                ph.lane = lane;
                ph.beatPos = beat;

                placeholders.Add(ph);
            }
    }

    public void SelectNoteVal(int noteVal)
    {
        NoteButton selectedNote = Array.Find(noteValButtons, noteButton => noteButton.noteVal == noteVal);
        if (selectedNote.noteVal == 0 || selectedNote.noteButton == null)
        {
            Debug.LogWarningFormat($"Note button {noteVal} not found");
            return;
        }
        if (currentNoteVal.noteButton != null) currentNoteVal.noteButton.interactable = true;
        selectedNote.noteButton.interactable = false;
        currentNoteVal = selectedNote;
        SpawnPlaceholders();
    }

    public void SelectTriplet(bool turnOn)
    {
        usingTriplet = turnOn;
        SpawnPlaceholders();
    }
}
