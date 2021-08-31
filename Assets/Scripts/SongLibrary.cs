using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

// TODO: Save songs to Resources

public class SongLibrary : MonoBehaviour
{
    string savePath;
    const string extension = "lprs";
    BinaryFormatter bf = new BinaryFormatter();

    List<(string, Song)> songs;

    void Awake()
    {
        savePath = $"{Application.dataPath}/Song Data";
    }

    void Start()
    {
        string[] filePaths = Directory.GetFiles(savePath, $"*.{extension}");

        songs = new List<(string, Song)>();
        foreach (string file in filePaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            songs.Add((fileName, LoadSong(fileName)));
        }
    }

    Song LoadSong(string fileName)
    {
        string filePath = $"{savePath}/{fileName}.{extension}";

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"{fileName}.{extension} does not exist");
            return null;
        }

        FileStream fs = new FileStream(filePath, FileMode.Open);
        Song s = bf.Deserialize(fs) as Song;
        fs.Close();

        return s;
    }

    public string[] GetSongTitles()
    {
        string[] titles = new string[songs.Count];

        for (int i = 0; i < songs.Count; i++)
            titles[i] = songs[i].Item2.title;

        return titles;
    }

    public (string, Song) GetFirstSong()
    {
        if (songs.Count == 0)
        {
            Debug.LogWarning($"No songs in library");
            return ("", null);
        }
        return songs[0];
    }

    public (string, Song) FindSong(string title)
    {
        (string fn, Song s) = songs.Find(entry => entry.Item2.title == title);
        if (s == null)
        {
            Debug.LogWarning($"Song {title} not found");
            return ("", null);
        }
        return (fn, s);
    }

    public void SaveSong(Song song, string fileName)
    {
        if (!songs.Exists(entry => entry.Item1 == fileName))
        {
            songs.Add((fileName, song));
        }

        string filePath = $"{savePath}/{fileName}.{extension}";

        FileStream fs = new FileStream(filePath, FileMode.Create);
        bf.Serialize(fs, song);
        fs.Close();

        Debug.Log($"{fileName}.{extension} saved");
    }

    public void DeleteSong(string title)
    {
        (string fn, Song s) = songs.Find(entry => entry.Item2.title == title);
        if (s == null)
        {
            Debug.LogWarning($"Song {title} not found");
            return;
        }

        string filePath = $"{savePath}/{fn}.{extension}";
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"{fn}.{extension} does not exist");
            return;
        }

        songs.Remove((fn, s));
        File.Delete(filePath);

        Debug.Log($"{fn}.{extension} deleted");
    }
}
