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

public class SongLibrary : MonoBehaviour
{
    string resourcesPath;
    string savePath;
    const string extension = "lprs";

    List<(string, Song)> songs;
    List<(string, Song)> userSongs;

    BinaryFormatter bf = new BinaryFormatter();

    void Awake()
    {
        resourcesPath = $"{Application.dataPath}/Resources/Song Data";
        savePath = $"{Application.persistentDataPath}/Song Data";
    }

    void Start()
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        songs = new List<(string, Song)>();

        TextAsset[] songData = Resources.LoadAll<TextAsset>("Song Data");
        foreach (var file in songData)
        {
            MemoryStream ms = new MemoryStream(file.bytes);
            Song s = bf.Deserialize(ms) as Song;
            ms.Close();

            songs.Add((file.name, s));
        }

        userSongs = new List<(string, Song)>();

        string[] filePaths = Directory.GetFiles(savePath, $"*.{extension}");
        foreach (string filePath in filePaths)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            Song s = bf.Deserialize(fs) as Song;
            fs.Close();

            userSongs.Add((Path.GetFileNameWithoutExtension(filePath), s));
        }
    }

    public string[] GetSongTitles()
    {
        string[] titles = new string[songs.Count + userSongs.Count];

        int t = 0;
        for (int i = 0; i < songs.Count; i++)
        {
            titles[t] = songs[i].Item2.title;
            t++;
        }
        for (int i = 0; i < userSongs.Count; i++)
        {
            titles[t] = userSongs[i].Item2.title;
            t++;
        }

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

    // TODO: Find/delete song by different identifier

    public (string, Song) FindSong(string title)
    {
        (string fn, Song s) = userSongs.Find(entry => entry.Item2.title == title);
        
        if (s != null)
            return (fn, s);

        (fn, s) = songs.Find(entry => entry.Item2.title == title);
        if (s == null)
        {
            Debug.LogWarning($"Song {title} not found");
            return ("", null);
        }
        return (fn, s);
    }

    public void SaveSongToResources(Song song, string fileName)
    {
        string filePath = $"{resourcesPath}/{fileName}.bytes";

        FileStream fs = new FileStream(filePath, FileMode.Create);
        bf.Serialize(fs, song);
        fs.Close();

        Debug.Log($"{fileName}.bytes saved");
    }

    public void SaveSong(Song song, string fileName)
    {
        if (!userSongs.Exists(entry => entry.Item1 == fileName))
        {
            userSongs.Add((fileName, song));
        }

        string filePath = $"{savePath}/{fileName}.{extension}";

        FileStream fs = new FileStream(filePath, FileMode.Create);
        bf.Serialize(fs, song);
        fs.Close();

        Debug.Log($"{fileName}.{extension} saved");
    }

    public void DeleteSong(string title)
    {
        (string fn, Song s) = userSongs.Find(entry => entry.Item2.title == title);
        if (s == null)
        {
            Debug.LogWarning($"User song {title} not found");
            return;
        }

        string filePath = $"{savePath}/{fn}.{extension}";
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"{fn}.{extension} does not exist");
            return;
        }

        userSongs.Remove((fn, s));
        File.Delete(filePath);

        Debug.Log($"{fn}.{extension} deleted");
    }
}
