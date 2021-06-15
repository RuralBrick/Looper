using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Song
{
    public AudioClip file;
    public string title;
    public int beatsPerBar = 4;
    public int beatUnit = 4;
    public float tempo;
}

public class SongLibraryManager : MonoBehaviour
{
    static SongLibraryManager instance;

    public Song[] songs;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public Song FindSong(string title)
    {
        Song s = Array.Find(songs, song => song.title == title);
        if (s == null)
        {
            Debug.LogWarningFormat($"Song {title} not found");
            return null;
        }
        return s;
    }
}
