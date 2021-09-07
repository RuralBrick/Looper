using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongSelectManager : MonoBehaviour
{
    Text titleText;
    Text highScoreText;
    SongListHandler songListHandler;

    void Awake()
    {
        titleText = GameObject.Find("Title Text").GetComponent<Text>();
        highScoreText = GameObject.Find("High Score Text").GetComponent<Text>();
        songListHandler = FindObjectOfType<SongListHandler>();
    }

    public void LoadLibrary((string, string)[] songList)
    {
        songListHandler.CreateList(songList);
    }

    public void SetSong(string title, int highScore)
    {
        titleText.text = title;
        highScoreText.text = $"High Score: {highScore}";
    }
}
