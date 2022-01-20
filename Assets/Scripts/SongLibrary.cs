using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Song
{
    [Serializable]
    class AudioClipData
    {
        string name;
        float[] samples;
        int lengthSamples;
        int channels;
        int frequency;
        bool stream;

        public AudioClipData(AudioClip reference)
        {
            name = reference.name;
            lengthSamples = reference.samples;
            channels = reference.channels;
            frequency = reference.frequency;
            stream = false;

            samples = new float[lengthSamples * channels];
            reference.GetData(samples, 0);
        }

        public AudioClip GenerateClip()
        {
            AudioClip clip = AudioClip.Create(name, lengthSamples, channels, frequency, stream);
            clip.SetData(samples, 0);
            return clip;
        }
    }

    public string title = "";
    AudioClipData clipData;
    public float offset = 0f;
    public int beatsPerBar = 4;
    public int beatUnit = 4;
    public float tempo = 120;
    public Note[] track = new Note[0];

    public AudioClip Clip
    {
        get
        {
            if (clipData == null)
            {
                Debug.LogWarning("Clip data not found");
                return null;
            }
            return clipData.GenerateClip();
        }
        set => clipData = new AudioClipData(value);
    }
}

public class SongLibrary : MonoBehaviour
{
    BinaryFormatter bf = new BinaryFormatter();

    Dictionary<string, Song> songs;

    void Awake()
    {
        songs = new Dictionary<string, Song>();

        TextAsset[] songData = Resources.LoadAll<TextAsset>("Song Data");
        foreach (var file in songData)
        {
            using MemoryStream ms = new MemoryStream(file.bytes);
            using var decompressor = new GZipStream(ms, CompressionMode.Decompress);

            Song s = bf.Deserialize(decompressor) as Song;
            songs.Add(file.name, s);
        }
    }

    public (string, string)[] GetSongs()
    {
        string[] titles = new string[songs.Count];
        (string, string)[] songList = new (string, string)[songs.Count];

        int t = 0;
        foreach (var entry in songs)
        {
            titles[t] = entry.Value.title;
            songList[t] = (entry.Key, entry.Value.title);
            t++;
        }

        Array.Sort(titles, songList);
        return songList;
    }

    public (Song, bool) FindSong(string fileName)
    {
        Song s;
        if (songs.TryGetValue(fileName, out s))
        {
            return (s, false);
        }

        Debug.LogWarning($"Song {fileName} not found");
        return (null, false);
    }

    public (string, Song, bool) FindSongByTitle(string title)
    {
        var entry = songs.Where(entry => entry.Value.title == title).FirstOrDefault();
        string fn = entry.Key;
        Song s = entry.Value;
        if (s != null)
        {
            return (fn, s, false);
        }

        Debug.LogWarning($"Song {title} not found");
        return ("", null, false);
    }

    public void SaveSongToResources(Song song, string fileName) { }

    public void SaveSong(Song song, string fileName) { }

    public void DeleteSong(string fileName) { }
}
