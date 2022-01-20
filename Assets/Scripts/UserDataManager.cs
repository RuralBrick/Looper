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
    Dictionary<string, int> songScores;
    Dictionary<string, int> userSongScores;

    void Start()
    {
        songScores = new Dictionary<string, int>();
        userSongScores = new Dictionary<string, int>();
    }

    void SaveScores() { }

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
