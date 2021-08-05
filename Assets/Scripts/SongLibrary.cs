using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// TODO: Remove all TestSong stuff

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
    string savePath;
    const string extension = "lprs";

    (string, Song)[] songs;

    void Awake()
    {
        savePath = $"{Application.dataPath}/Song Data";
    }

    void Start()
    {
        string[] filePaths = Directory.GetFiles(savePath, $"*.{extension}");

        songs = new (string, Song)[filePaths.Length];
        for (int i = 0; i < filePaths.Length; i++)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePaths[i]);
            songs[i] = (fileName, LoadSong(fileName));
        }
    }

    public (string, Song) FindSong(string title)
    {
        (string fn, Song s) = Array.Find(songs, entry => entry.Item2.title == title);
        if (s == null)
        {
            Debug.LogWarning($"Song {title} not found");
            return ("", null);
        }
        return (fn, s);
    }

    public void SaveSong(Song song, string fileName)
    {
        string filePath = $"{savePath}/{fileName}.{extension}";
        FileStream fs = new FileStream(filePath, FileMode.Create);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, song);

        fs.Close();

        Debug.Log($"{fileName}.{extension} saved");
    }

    Song LoadSong(string fileName)
    {
        string filePath = $"{savePath}/{fileName}.{extension}";

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"{fileName}.{extension} does not exits");
            return null;
        }

        FileStream fs = new FileStream(filePath, FileMode.Open);

        BinaryFormatter bf = new BinaryFormatter();
        Song s = bf.Deserialize(fs) as Song;

        fs.Close();

        return s;
    }
}
