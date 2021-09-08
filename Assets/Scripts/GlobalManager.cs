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
    UserDataManager userDataManager;

    public delegate void LaneInput(int lane);
    public LaneInput LanePressed = delegate { };
    
    public float syncOffset = 0f;
    public float hitOffset = 0f;

    string fileName = "";
    Song currentSong = null;
    bool usingUserSong = false;

    CalibrationManager calibrationManager;
    SongSelectManager songSelectManager;
    EditorManager editorManager;
    PlayManager playManager;
    ResultsManager resultsManager;

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
        userDataManager = GetComponentInChildren<UserDataManager>();
    }

    void Start()
    {
        SceneManager.sceneLoaded += SetupScenes;
        SceneManager.sceneUnloaded += TeardownScenes;

        if (PlayerPrefs.HasKey("SFX Volume"))
        {
            // TODO: Tell soundManager
        }
        if (PlayerPrefs.HasKey("Music Volume"))
        {
            soundManager.SetSongVolume(PlayerPrefs.GetFloat("Music Volume"));
        }
        if (syncOffset != 0)
        {
            PlayerPrefs.SetFloat("Sync Offset", syncOffset);
        }
        else if (PlayerPrefs.HasKey("Sync Offset"))
        {
            syncOffset = PlayerPrefs.GetFloat("Sync Offset");
        }
        if (hitOffset != 0)
        {
            PlayerPrefs.SetFloat("Hit Offset", hitOffset);
        }
        else if (PlayerPrefs.HasKey("Hit Offset"))
        {
            hitOffset = PlayerPrefs.GetFloat("Hit Offset");
        }

        // HACK
        if (testSong != "")
            (fileName, currentSong, usingUserSong) = songLibrary.FindSongByTitle(testSong);
        // end HACK

        SetupScenes(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    #region Helper
    public static void SetSFXVolume(float volume)
    {
        // TODO: Tell soundManager

        PlayerPrefs.SetFloat("SFX Volume", volume);
    }

    public static void SetMusicVolume(float volume)
    {
        instance.soundManager.SetSongVolume(volume);
        PlayerPrefs.SetFloat("Music Volume", volume);
    }
    
    public static void SetSyncOffset(float offset)
    {
        instance.syncOffset = offset;
        PlayerPrefs.SetFloat("Sync Offset", offset);
    }

    public static void SetHitOffset(float offset)
    {
        instance.hitOffset = offset;
        PlayerPrefs.SetFloat("Hit Offset", offset);
    }

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

    public void SelectSong(string fileName)
    {
        (Song s, bool uus) = songLibrary.FindSong(fileName);

        if (s != null)
        {
            this.fileName = fileName;
            currentSong = s;
            usingUserSong = uus;
            songSelectManager?.SetSong(s.title, uus ? 
                userDataManager.GetUserSongScore(fileName) : 
                userDataManager.GetSongScore(fileName));
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

    public void SaveScore(int score)
    {
        if (usingUserSong)
        {
            userDataManager.SaveUserSongScore(fileName, score);
        }
        else
        {
            userDataManager.SaveSongScore(fileName, score);
        }
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
            case "TitleScene": SetupTitleScene(); break;
            case "CalibrationScene": SetupCalibrationScene(); break;
            case "SongSelectScene": SetupSongSelectScene(); break;
            case "EditorScene": SetupEditorScene(); break;
            case "PlayScene": SetupPlayScene(); break;
            case "ResultsScene": SetupResultsScene(); break;
        }
    }

    public void TeardownScenes(Scene scene)
    {
        switch (scene.name)
        {
            case "CalibrationScene": TeardownCalibrationScene(); break;
            case "SongSelectScene": TeardownSongSelectScene(); break;
            case "EditorScene": TeardownEditorScene(); break;
            case "PlayScene": TeardownPlayScene(); break;
            case "ResultsScene": TeardownResultsScene(); break;
        }
    }

    void SetupTitleScene()
    {
        if (PlayerPrefs.HasKey("SFX Volume"))
        {
            UnityEngine.UI.Slider sfxSlider = GameObject.Find("SFX Volume Slider").GetComponent<UnityEngine.UI.Slider>();
            sfxSlider.value = PlayerPrefs.GetFloat("SFX Volume");
        }
        if (PlayerPrefs.HasKey("Music Volume"))
        {
            UnityEngine.UI.Slider musicSlider = GameObject.Find("Music Volume Slider").GetComponent<UnityEngine.UI.Slider>();
            musicSlider.value = PlayerPrefs.GetFloat("Music Volume");
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

        var songs = songLibrary.GetSongs();
        songSelectManager.LoadLibrary(songs);
        SelectSong(songs[0].Item1);
    }

    void TeardownSongSelectScene()
    {
        songSelectManager = null;
    }

    void SetupEditorScene()
    {
        editorManager = FindObjectOfType<EditorManager>();

        if (currentSong != null)
            editorManager.LoadSong(currentSong, fileName);
        else
            editorManager.InitializeEmpty();
    }

    void TeardownEditorScene()
    {
        editorManager = null;
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

    void SetupResultsScene()
    {
        resultsManager = FindObjectOfType<ResultsManager>();

        resultsManager.SetText(PlayManager.score, PlayManager.maxCombo);
    }

    void TeardownResultsScene()
    {
        resultsManager = null;

        soundManager.StopSong();
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
