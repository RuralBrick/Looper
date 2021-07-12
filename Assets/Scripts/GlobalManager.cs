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

    [Range(-0.05f, 0.05f)]
    public float calibration = 0f;

    string currentScene;

    SongManager sngm;
    EditorManager em;
    CalibrationManager cm;

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

    // TODO: Have hit show up ON stop

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
        currentScene = scene.name;
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

        Song s = sl.FindSong("test track");
        sngm.LoadSong(s);
    }

    void TeardownGameScene()
    {
        LanePressed -= sngm.CheckLane;
        sngm = null;
    }

    void SetupEditorScene()
    {
        em = FindObjectOfType<EditorManager>();

        Song s = sl.FindSong("test track");
        em.LoadSong(s);
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
