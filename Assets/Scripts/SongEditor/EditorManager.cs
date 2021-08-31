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
    const int PHRASE_SCROLL_AMOUNT = 4;
    const float NEW_NOTE_BUFFER = 1f;
    const int NEW_NOTE_BARS = 4;

    Song currentSong;
    string currentFileName;
    string currentTrackName;
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
        public EditNoteHandler editNoteHandler;
    }
    SelectedNote selectedNote;
    List<EditNoteHandler> barNotes = new List<EditNoteHandler>();
    
    LoopDisplayHandler loopDisplayHandler;
    SongDisplayManager songDisplayManager;

    InputField offsetField;
    Slider offsetSlider;
    InputField beatsPerBarField;
    InputField beatUnitField;
    InputField tempoField;
    NoteInfoHandler noteInfoHandler;
    List<PlaceholderHandler> placeholders = new List<PlaceholderHandler>();

    public GameObject[] placeholderPrefabs = new GameObject[LoopDisplayHandler.LANE_COUNT];
    public GameObject[] editNotePrefabs = new GameObject[LoopDisplayHandler.LANE_COUNT];
    public NoteButton[] noteValButtons;

    void Awake()
    {
        loopDisplayHandler = FindObjectOfType<LoopDisplayHandler>();
        songDisplayManager = FindObjectOfType<SongDisplayManager>();
        offsetField = GameObject.Find("Offset Field").transform.GetComponent<InputField>();
        offsetSlider = GameObject.Find("Offset Slider").transform.GetComponent<Slider>();
        beatsPerBarField = GameObject.Find("Beats Per Bar Field").transform.GetComponent<InputField>();
        beatUnitField = GameObject.Find("Beat Unit Field").transform.GetComponent<InputField>();
        tempoField = GameObject.Find("Tempo Field").transform.GetComponent<InputField>();
        noteInfoHandler = FindObjectOfType<NoteInfoHandler>();
    }

    void Start()
    {
        loopDisplayHandler.HideMetronome();
        noteInfoHandler.Hide();
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
                !NoteInBar(selectedNote.noteRef.Value.start,
                           selectedNote.noteRef.Value.stop,
                           selectedNote.noteRef.Value.beatPos))
                DeselectNote();

            SpawnEditNotes();
            songDisplayManager.DisplayBar(notes);
        }
    }

    void Initialize()
    {
        foreach (Note n in currentSong.track)
        {
            Ref<Note> newNoteRef = new Ref<Note>();
            newNoteRef.Value = n;
            notes.Add(newNoteRef);
        }

        loopDisplayHandler.Initialize(currentSong.beatsPerBar);
        songDisplayManager.Initialize(currentSong);

        GameObject.Find("Title Field").transform.GetComponent<InputField>().text = currentSong.title;
        offsetField.text = currentSong.offset.ToString();
        offsetSlider.value = currentSong.offset;
        beatsPerBarField.text = currentSong.beatsPerBar.ToString();
        beatUnitField.text = currentSong.beatUnit.ToString();
        tempoField.text = currentSong.tempo.ToString();
        GameObject.Find("File Name Field").transform.GetComponent<InputField>().text = currentFileName;
        //GameObject.Find("Track Name Field").transform.GetComponent<InputField>().text = currentTrackName;

        CurrentBar = 0;
    }

    public void InitializeEmpty()
    {
        currentSong = new Song();
        currentFileName = "";
        currentTrackName = "";

        Initialize();
    }

    public void LoadSong(Song s, string fileName)
    {
        currentSong = s;
        currentFileName = fileName;
        currentTrackName = "";

        Initialize();
    }

    public void SetTitle(string title)
    {
        currentSong.title = title;
    }

    // TODO: Make audioClip upload field

    public void SetOffset(string textOffset)
    {
        float newOffset;
        if (float.TryParse(textOffset, out newOffset))
        {
            currentSong.offset = newOffset;
            offsetSlider.value = newOffset;
            songDisplayManager.SetOffset(newOffset);
        }
    }

    public void SetOffset(float newOffset)
    {
        currentSong.offset = newOffset;
        offsetField.text = newOffset.ToString();
        songDisplayManager.SetOffset(newOffset);
    }

    public void SetBeatsPerBar(string beatsPerBarText)
    {
        int beatsPerBar;
        if (int.TryParse(beatsPerBarText, out beatsPerBar) && beatsPerBar > 0)
        {
            currentSong.beatsPerBar = beatsPerBar;
            loopDisplayHandler.Initialize(beatsPerBar);
            songDisplayManager.Initialize(currentSong);
            CurrentBar = currentBar;
        }
        else
        {
            beatsPerBarField.text = currentSong.beatsPerBar.ToString();
        }
    }

    public void SetBeatUnit(string beatUnitText)
    {
        int beatUnit;
        if (int.TryParse(beatUnitText, out beatUnit) && beatUnit > 0)
        {
            currentSong.beatUnit = beatUnit;
            SpawnPlaceholders();
        }
        else
        {
            beatUnitField.text = currentSong.beatUnit.ToString();
        }
    }

    public void SetTempo(string tempoText)
    {
        float tempo;
        if (float.TryParse(tempoText, out tempo) && tempo > 0)
        {
            currentSong.tempo = tempo;
            songDisplayManager.Initialize(currentSong);
        }
        else
        {
            tempoField.text = currentSong.tempo.ToString();
        }
    }

    public void SetTrackName(string trackName)
    {
        currentTrackName = trackName;
    }

    public void SetFileName(string fileName)
    {
        currentFileName = fileName;
    }

    // TODO: Save track option

    public void SaveSong()
    {
        currentSong.track = new Note[notes.Count];
        
        for (int i = 0; i < notes.Count; i++)
            currentSong.track[i] = notes[i].Value;

        GlobalManager.instance.SaveSong(currentSong, currentFileName);
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
        CurrentBar += PHRASE_SCROLL_AMOUNT;
    }

    public void DisplayPrevPhrase()
    {
        CurrentBar -= PHRASE_SCROLL_AMOUNT;
    }

    bool NoteInBar(float start, float stop, float beatPos)
    {
        float barBeat = currentBar * currentSong.beatsPerBar;
        float rangeStart = start - barBeat;
        float rangeEnd = stop - barBeat;
        return rangeStart <= beatPos && beatPos <= rangeEnd;
    }

    void SpawnEditNotes()
    {
        foreach (EditNoteHandler enh in barNotes)
            Destroy(enh.gameObject);
        barNotes.Clear();

        foreach (Ref<Note> n in notes)
        {
            if (NoteInBar(n.Value.start, n.Value.stop, n.Value.beatPos))
            {
                GameObject newEditNote = Instantiate(editNotePrefabs[n.Value.lane]);
                newEditNote.transform.position = loopDisplayHandler.CalcNotePosition(n.Value.lane, n.Value.beatPos);
                newEditNote.transform.localScale = loopDisplayHandler.CalcNoteScale();

                EditNoteHandler enh = newEditNote.GetComponent<EditNoteHandler>();
                enh.editorManager = this;
                enh.info = n;

                barNotes.Add(enh);
            }
        }
    }

    void UpdateNoteDisplay()
    {
        SpawnEditNotes();
        songDisplayManager.SpawnNoteLines(notes);
    }

    // TODO: Try to save note when switch note

    public void SelectNote(EditNoteHandler enh)
    {
        if (selectedNote.editNoteHandler != null) selectedNote.editNoteHandler.Deselect();
        enh.Select();
        selectedNote.noteRef = enh.info;
        selectedNote.editNoteHandler = enh;

        noteInfoHandler.SetStart(enh.info.Value.start);
        noteInfoHandler.SetStop(enh.info.Value.stop);

        noteInfoHandler.Show();
    }

    public void DeselectNote()
    {
        noteInfoHandler.Hide();

        if (selectedNote.editNoteHandler != null) selectedNote.editNoteHandler.Deselect();

        selectedNote.noteRef = null;
        selectedNote.editNoteHandler = null;
    }

    public void AddNote(int lane, float beatPos)
    {
        foreach (Ref<Note> n in notes)
        {
            if (lane == n.Value.lane &&
                Mathf.Approximately(beatPos, n.Value.beatPos) &&
                NoteInBar(n.Value.start, n.Value.stop, n.Value.beatPos))
                return;
        }

        Note newNote = new Note();
        newNote.lane = lane;
        newNote.beatPos = beatPos;
        float barBeat = currentBar * currentSong.beatsPerBar;
        float noteBeat = barBeat + beatPos;
        newNote.start = barBeat;
        newNote.stop = noteBeat;

        Ref<Note> noteRef = new Ref<Note>();
        noteRef.Value = newNote;

        notes.Add(noteRef);

        UpdateNoteDisplay();
        SelectNote(barNotes.Find(enh => enh.info == noteRef));
    }

    bool BeatPosInRange(float start, float end, float beatPos)
    {
        float offset = (start + currentSong.beatsPerBar) % currentSong.beatsPerBar;
        float adjustedBeatPos = (beatPos + currentSong.beatsPerBar - offset) % currentSong.beatsPerBar;
        float rangeStart = 0f; // NOTE: start - start
        float rangeEnd = end - start;
        return rangeStart <= adjustedBeatPos && adjustedBeatPos <= rangeEnd;
    }

    bool StartValid(Note n, float start)
    {
        float firstPreZeroBeat = n.beatPos - currentSong.beatsPerBar;
        return start > firstPreZeroBeat && start <= n.stop && BeatPosInRange(start, n.stop, n.beatPos);
    }

    // TODO: Prevent note overlap (not same lane, beatPos, and range)

    public void SetSelectedNoteStart(string startText)
    {
        float start;
        if (float.TryParse(startText, out start))
        {
            Note info = selectedNote.noteRef.Value;
            
            if (!StartValid(info, start))
            {
                noteInfoHandler.SetStart(info.start);
                return;
            }

            info.start = start;
            selectedNote.noteRef.Value = info;

            if (!NoteInBar(info.start, info.stop, info.beatPos))
                DeselectNote();

            UpdateNoteDisplay();
        }
    }

    public void AdjustSelectedNoteStart(string adjustText)
    {
        float adjust;
        if (float.TryParse(adjustText, out adjust))
        {
            Note info = selectedNote.noteRef.Value;
            float newStart = info.start + adjust;

            if (!StartValid(info, newStart))
                return;

            info.start = newStart;
            selectedNote.noteRef.Value = info;

            noteInfoHandler.SetStart(newStart);
            noteInfoHandler.ClearStartAdjust();

            if (!NoteInBar(info.start, info.stop, info.beatPos))
                DeselectNote();

            UpdateNoteDisplay();
        }
    }

    bool StopValid(Note n, float stop)
    {
        return stop >= 0 && stop >= n.start && BeatPosInRange(n.start, stop, n.beatPos);
    }

    public void SetSelectedNoteStop(string stopText)
    {
        float stop;
        if (float.TryParse(stopText, out stop))
        {
            Note info = selectedNote.noteRef.Value;
            
            if (!StopValid(info, stop))
            {
                noteInfoHandler.SetStop(info.stop);
                return;
            }

            info.stop = stop;
            selectedNote.noteRef.Value = info;

            if (!NoteInBar(info.start, info.stop, info.beatPos))
                DeselectNote();

            UpdateNoteDisplay();
        }
    }

    public void AdjustSelectedNoteStop(string adjustText)
    {
        float adjust;
        if (float.TryParse(adjustText, out adjust))
        {
            Note info = selectedNote.noteRef.Value;
            float newStop = info.stop + adjust;

            if (!StopValid(info, newStop))
                return;

            info.stop = newStop;
            selectedNote.noteRef.Value = info;

            noteInfoHandler.SetStop(newStop);
            noteInfoHandler.ClearStopAdjust();

            if (!NoteInBar(info.start, info.stop, info.beatPos))
                DeselectNote();

            UpdateNoteDisplay();
        }
    }

    public void RemoveSelectedNote()
    {
        noteInfoHandler.Hide();

        notes.Remove(selectedNote.noteRef);

        selectedNote.noteRef = null;
        selectedNote.editNoteHandler = null;

        UpdateNoteDisplay();
    }

    void SpawnPlaceholders()
    {
        foreach (PlaceholderHandler ph in placeholders)
            Destroy(ph.gameObject);
        placeholders.Clear();

        if (currentNoteVal.noteVal == 0 || currentNoteVal.noteButton == null)
            return;

        float noteLength = (float)currentSong.beatUnit / currentNoteVal.noteVal;
        if (usingTriplet) noteLength *= 2/3f;

        for(int lane = 0; lane < LoopDisplayHandler.LANE_COUNT; lane++)
            for (float beat = 0; beat < currentSong.beatsPerBar; beat += noteLength)
            {
                GameObject newPlaceholder = Instantiate(placeholderPrefabs[lane]);
                newPlaceholder.transform.position = loopDisplayHandler.CalcNotePosition(lane, beat);
                newPlaceholder.transform.localScale = loopDisplayHandler.CalcNoteScale();

                PlaceholderHandler ph = newPlaceholder.GetComponent<PlaceholderHandler>();
                ph.editorManager = this;
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
            Debug.LogWarning($"Note button {noteVal} not found");
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
