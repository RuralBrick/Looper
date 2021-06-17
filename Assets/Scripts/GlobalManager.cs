using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;

    SoundManager sm;
    SongLibraryManager slm;

    [Range(-0.05f, 0.05f)]
    public float calibration = 0f;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        sm = GetComponentInChildren<SoundManager>();
        slm = GetComponentInChildren<SongLibraryManager>();
    }

    void Start()
    {
        Song s = slm.FindSong("test track");
        FindObjectOfType<SongManager>().LoadSong(s);
    }
}
