using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    string resourcesPath;
    string savePath;
    const string extension = "lprs";

    BinaryFormatter bf = new BinaryFormatter();

    Dictionary<string, Song> songs;
    Dictionary<string, Song> userSongs;

    void Awake()
    {
        resourcesPath = $"{Application.dataPath}/Resources/Song Data";
        savePath = $"{Application.persistentDataPath}/Song Data";
        
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        songs = new Dictionary<string, Song>();

        TextAsset[] songData = Resources.LoadAll<TextAsset>("Song Data");
        foreach (var file in songData)
        {
            MemoryStream ms = new MemoryStream(file.bytes);
            Song s = bf.Deserialize(ms) as Song;
            ms.Close();

            songs.Add(file.name, s);
        }

        userSongs = new Dictionary<string, Song>();

        string[] filePaths = Directory.GetFiles(savePath, $"*.{extension}");
        foreach (string filePath in filePaths)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            Song s = bf.Deserialize(fs) as Song;
            fs.Close();

            userSongs.Add(Path.GetFileNameWithoutExtension(filePath), s);
        }
    }

    public (string, string)[] GetSongs()
    {
        string[] titles = new string[songs.Count + userSongs.Count];
        (string, string)[] songList = new (string, string)[songs.Count + userSongs.Count];

        int t = 0;
        foreach (var entry in songs)
        {
            titles[t] = entry.Value.title;
            songList[t] = (entry.Key, entry.Value.title);
            t++;
        }
        foreach (var entry in userSongs)
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
        if (userSongs.TryGetValue(fileName, out s))
        {
            return (s, true);
        }
        if (songs.TryGetValue(fileName, out s))
        {
            return (s, false);
        }

        Debug.LogWarning($"Song {fileName} not found");
        return (null, false);
    }

    public (string, Song, bool) FindSongByTitle(string title)
    {
        var entry = userSongs.Where(entry => entry.Value.title == title).FirstOrDefault();
        string fn = entry.Key;
        Song s = entry.Value;

        if (s != null)
        {
            return (fn, s, true);
        }

        entry = songs.Where(entry => entry.Value.title == title).FirstOrDefault();
        fn = entry.Key;
        s = entry.Value;
        if (s != null)
        {
            return (fn, s, false);
        }

        Debug.LogWarning($"Song {title} not found");
        return ("", null, false);
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
        if (!userSongs.ContainsKey(fileName))
        {
            userSongs.Add(fileName, song);
        }
        else
        {
            userSongs[fileName] = song;
        }

        string filePath = $"{savePath}/{fileName}.{extension}";

        FileStream fs = new FileStream(filePath, FileMode.Create);
        bf.Serialize(fs, song);
        fs.Close();

        Debug.Log($"{fileName}.{extension} saved");
    }

    public void DeleteSong(string fileName)
    {
        if (!userSongs.ContainsKey(fileName))
        {
            Debug.LogWarning($"User song {fileName} not found");
            return;
        }
        
        string filePath = $"{savePath}/{fileName}.{extension}";
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"{fileName}.{extension} does not exist");
            return;
        }

        userSongs.Remove(fileName);
        File.Delete(filePath);

        Debug.Log($"{fileName}.{extension} deleted");
    }
}
