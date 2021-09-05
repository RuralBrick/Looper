using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;

    SoundManager soundManager;
    InputManager inputManager;
    SongLibrary songLibrary;
    TrackParser trackParser;

    public delegate void LaneInput(int lane);
    public LaneInput LanePressed = delegate { };
    
    // TODO: Use user prefs
    public float syncOffset = 0f;
    public float hitOffset = 0f;

    string fileName;
    Song currentSong;

    CalibrationManager calibrationManager;
    SongSelectManager songSelectManager;
    PlayManager playManager;
    EditorManager editorManager;

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

        soundManager = GetComponentInChildren<SoundManager>();
        inputManager = GetComponentInChildren<InputManager>();
        songLibrary = GetComponentInChildren<SongLibrary>();
        trackParser = GetComponentInChildren<TrackParser>();
    }

    void Start()
    {
        SceneManager.sceneLoaded += SetupScenes;
        SceneManager.sceneUnloaded += TeardownScenes;

        // HACK
        if (testSong != "")
            SelectSong(testSong);
        else
            (fileName, currentSong) = songLibrary.GetFirstSong();
        // end HACK

        SetupScenes(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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

    public void SelectSong(string title)
    {
        (string fn, Song s) = songLibrary.FindSong(title);

        if (s != null)
        {
            fileName = fn;
            currentSong = s;
            // HACK
            songSelectManager?.SetSong(s.title, 0);
            // end HACK
        }
    }

    public void SaveTrack(Note[] notes, string fileName)
    {
        trackParser.SaveTrack(notes, fileName);
    }

    public void SaveSong(Song s, string fileName)
    {
        songLibrary.SaveSong(s, fileName);
    }

    public void DevSaveSong(Song s, string fileName)
    {
        songLibrary.SaveSongToResources(s, fileName);
    }

    public Note[] ParseTrack(TextAsset trackFile)
    {
        return trackParser.ParseTrack(trackFile);
    }
    #endregion

    #region Input
    public Vector3 MousePosition()
    {
        return inputManager.mousePos;
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
            case "CalibrationScene": SetupCalibrationScene(); break;
            case "SongSelectScene": SetupSongSelectScene(); break;
            case "PlayScene": SetupPlayScene(); break;
            case "EditorScene": SetupEditorScene(); break;
        }
    }

    public void TeardownScenes(Scene scene)
    {
        switch (scene.name)
        {
            case "CalibrationScene": TeardownCalibrationScene(); break;
            case "SongSelectScene": TeardownSongSelectScene(); break;
            case "PlayScene": TeardownPlayScene(); break;
            case "EditorScene": TeardownEditorScene(); break;
        }
    }

    void SetupCalibrationScene()
    {
        calibrationManager = FindObjectOfType<CalibrationManager>();
        LanePressed += calibrationManager.Hit;
    }

    void TeardownCalibrationScene()
    {
        LanePressed -= calibrationManager.Hit;
        calibrationManager = null;
    }

    void SetupSongSelectScene()
    {
        songSelectManager = FindObjectOfType<SongSelectManager>();

        songSelectManager.LoadLibrary(songLibrary.GetSongTitles());
        // HACK
        songSelectManager.SetSong(currentSong.title, 0);
        // end HACK
    }

    void TeardownSongSelectScene()
    {
        songSelectManager = null;
    }

    void SetupPlayScene()
    {
        playManager = FindObjectOfType<PlayManager>();
        LanePressed += playManager.CheckLane;

        if (currentSong != null)
        {
            soundManager.LoadSong(currentSong);
            playManager.LoadSong(currentSong);
        }
    }

    void TeardownPlayScene()
    {
        LanePressed -= playManager.CheckLane;
        playManager = null;
    }

    void SetupEditorScene()
    {
        editorManager = FindObjectOfType<EditorManager>();

        if (currentSong != null)
            editorManager.LoadSong(currentSong, fileName);
    }

    void TeardownEditorScene()
    {
        editorManager = null;
    }
    #endregion

    #region Sounds
    public void PlaySong()
    {
        soundManager.PlaySong();
    }

    public void StartMetronome()
    {
        soundManager.Play("metronome");
    }

    public void StopMetronome()
    {
        soundManager.Stop("metronome");
    }
    #endregion
}
