using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;

    SoundManager sndm;
    InputManager im;
    SongLibrary sl;
    TrackParser tp;

    public delegate void LaneInput(int lane);
    public LaneInput LanePressed = delegate { };

    public float syncOffset = 0f;
    public float hitOffset = 0f;

    SongManager sngm;
    EditorManager em;
    CalibrationManager cm;

    // HACK
    public string testSong;
    // end HACK

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        sndm = GetComponentInChildren<SoundManager>();
        im = GetComponentInChildren<InputManager>();
        sl = GetComponentInChildren<SongLibrary>();
        tp = GetComponentInChildren<TrackParser>();
    }

    void Start()
    {
        SceneManager.sceneLoaded += SetupScenes;
        SceneManager.sceneUnloaded += TeardownScenes;
        SetupScenes(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        // HACK
        /*TestSong temp = sl.FindTestSong("test track");
        Song save = new Song();
        save.title = temp.title;
        save.File = temp.file;
        save.offset = temp.offset;
        save.beatsPerBar = temp.beatsPerBar;
        save.beatUnit = temp.beatUnit;
        save.tempo = temp.tempo;
        save.track = temp.Track;
        sl.SaveSong(save, "test-track-save");*/
        // end HACK
    }

    #region Helper
    public static void FormatLine(ref LineRenderer line, Color color, Material material, 
        string sortingLayer, int sortingOrder, float width, bool useWorldSpace = false)
    {
        line.startColor = line.endColor = color;
        line.material = material;
        line.sortingLayerName = sortingLayer;
        line.sortingOrder = sortingOrder;
        line.startWidth = line.endWidth = width;
        line.useWorldSpace = useWorldSpace;
    }

    // TODO: Save song to SongLibrary

    public void SaveTrack(Note[] notes, string fileName)
    {
        tp.SaveTrack(notes, fileName);
    }

    public void SaveSong(Song s, string fileName)
    {
        sl.SaveSong(s, fileName);
    }

    public Note[] ParseTrack(TextAsset trackFile)
    {
        return tp.ParseTrack(trackFile);
    }
    #endregion

    #region Input
    public Vector3 MousePosition()
    {
        return im.mousePos;
    }
    #endregion

    #region Scenes
    public static void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SetupScenes(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "GameScene": SetupGameScene(); break;
            case "EditorScene": SetupEditorScene(); break;
            case "CalibrationScene": SetupCalibrationScene(); break;
        }
    }

    public void TeardownScenes(Scene scene)
    {
        switch (scene.name)
        {
            case "GameScene": TeardownGameScene(); break;
            case "EditorScene": TeardownEditorScene(); break;
            case "CalibrationScene": TeardownCalibrationScene(); break;
        }
    }

    // TODO: Color code note prefabs

    void SetupGameScene()
    {
        sngm = FindObjectOfType<SongManager>();
        LanePressed += sngm.CheckLane;

        // HACK
        (string fn, Song s) = sl.FindSong("Techno Motif");
        if (s != null)
            sngm.LoadSong(s);
        // end HACK
    }

    void TeardownGameScene()
    {
        LanePressed -= sngm.CheckLane;
        sngm = null;
    }

    void SetupEditorScene()
    {
        em = FindObjectOfType<EditorManager>();

        // HACK
        /*TestSong temp = sl.FindTestSong("Techno Motif");
        if (temp != null)
        {
            Song s = new Song();
            (s.title, s.Clip, s.offset, s.beatsPerBar, s.beatUnit, s.tempo, s.track) =
                (temp.title, temp.file, temp.offset, temp.beatsPerBar, temp.beatUnit, temp.tempo, temp.Track);
            em.LoadSong(s, "");
        }*/
        (string fn, Song s) = sl.FindSong("Techno Motif");
        if (s != null)
            em.LoadSong(s, fn);
        // end HACK
    }

    void TeardownEditorScene()
    {
        em = null;
    }

    void SetupCalibrationScene()
    {
        cm = FindObjectOfType<CalibrationManager>();
        LanePressed += cm.Hit;
    }

    void TeardownCalibrationScene()
    {
        LanePressed -= cm.Hit;
        cm = null;
    }
    #endregion

    #region Sounds
    public void StartMetronome()
    {
        sndm.Play("metronome");
    }

    public void StopMetronome()
    {
        sndm.Stop("metronome");
    }
    #endregion
}
