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
    public LaneInput LanePressed;

    [Range(-0.05f, 0.05f)]
    public float calibration = 0f;

    string currentScene;

    SongManager sngm;

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
        // HACK
        SetupGameScene();
        // end HACK
    }
    
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
        }
        currentScene = scene.name;
    }

    public void TeardownScenes(Scene scene)
    {
        switch (scene.name)
        {
            case "GameScene": TeardownGameScene(); break;
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
    #endregion
}
