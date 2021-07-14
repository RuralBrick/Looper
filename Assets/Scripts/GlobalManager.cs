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

    void SetupGameScene()
    {
        sngm = FindObjectOfType<SongManager>();
        LanePressed += sngm.CheckLane;

        // HACK
        Song s = sl.FindSong(testSong);
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
        Song s = sl.FindSong(testSong);
        if (s != null)
            em.LoadSong(s);
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
