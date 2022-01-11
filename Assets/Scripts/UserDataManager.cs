using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
class UserData
{
    public string[] songs;
    public int[] songScores;

    public string[] userSongs;
    public int[] userSongScores;
}

public class UserDataManager : MonoBehaviour
{
    const string fileName = "looperUser.data";
    string filePath;

    BinaryFormatter binaryFormatter = new BinaryFormatter();

    Dictionary<string, int> songScores;
    Dictionary<string, int> userSongScores;

    void Awake()
    {
        filePath = $"{Application.persistentDataPath}/{fileName}";
    }

    void Start()
    {
        songScores = new Dictionary<string, int>();
        userSongScores = new Dictionary<string, int>();

        if (File.Exists(filePath))
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                UserData userData = binaryFormatter.Deserialize(fileStream) as UserData;
                for (int i = 0; i < userData.songs.Length; i++)
                {
                    songScores.Add(userData.songs[i], userData.songScores[i]);
                }
                for (int i = 0; i < userData.userSongs.Length; i++)
                {
                    userSongScores.Add(userData.userSongs[i], userData.userSongScores[i]);
                }
            }
        }
    }

    void SaveScores()
    {
        UserData userData = new UserData();

        userData.songs = new string[songScores.Count];
        userData.songScores = new int[songScores.Count];
        int i = 0;
        foreach (var item in songScores)
        {
            userData.songs[i] = item.Key;
            userData.songScores[i] = item.Value;
            i++;
        }

        userData.userSongs = new string[userSongScores.Count];
        userData.userSongScores = new int[userSongScores.Count];
        i = 0;
        foreach (var item in userSongScores)
        {
            userData.userSongs[i] = item.Key;
            userData.userSongScores[i] = item.Value;
            i++;
        }

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            binaryFormatter.Serialize(fileStream, userData);
            Debug.Log($"{fileName} saved");
        }
    }

    public void SaveSongScore(string fileName, int score)
    {
        if (songScores.ContainsKey(fileName))
        {
            if (songScores[fileName] < score)
                songScores[fileName] = score;
        }
        else
        {
            songScores.Add(fileName, score);
        }

        SaveScores();
    }

    public void SaveUserSongScore(string fileName, int score)
    {
        if (userSongScores.ContainsKey(fileName))
        {
            if (userSongScores[fileName] < score)
                userSongScores[fileName] = score;
        }
        else
        {
            userSongScores.Add(fileName, score);
        }

        SaveScores();
    }

    public int GetSongScore(string fileName)
    {
        int score;
        if (songScores.TryGetValue(fileName, out score))
        {
            return score;
        }

        return 0;
    }
    
    public int GetUserSongScore(string fileName)
    {
        int score;
        if (userSongScores.TryGetValue(fileName, out score))
        {
            return score;
        }

        return 0;
    }
}
