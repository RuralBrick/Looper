using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Song
{
    public string title;
    public AudioClip file;
    public float offset = 0f;
    public int beatsPerBar = 4;
    public int beatUnit = 4;
    public float tempo;
    public TextAsset trackFile;

    public Note[] Track
    {
        get
        {
            if (trackFile)
                return GlobalManager.instance.ParseTrack(trackFile);
            return new Note[0];
        }
    }

    public string TrackName {
        get => trackFile ? trackFile.name : "";
    }
}

public class SongLibrary : MonoBehaviour
{
    public Song[] songs;

    public Song FindSong(string title)
    {
        Song s = Array.Find(songs, song => song.title == title);
        if (s == null)
        {
            Debug.LogWarning($"Song {title} not found");
            return null;
        }
        return s;
    }
}
