using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongListHandler : MonoBehaviour
{
    float songButtonHeight;

    RectTransform content;
    List<SongButtonHandler> songButtons = new List<SongButtonHandler>();
    
    public GameObject songButtonPrefab;

    void Awake()
    {
        songButtonHeight = songButtonPrefab.GetComponent<RectTransform>().rect.height;

        content = transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
        for (int i = 0; i < content.childCount; i++)
        {
            songButtons.Add(content.GetChild(i).GetComponent<SongButtonHandler>());
        }
    }

    public void CreateList(string[] titles)
    {
        foreach (SongButtonHandler sbh in songButtons)
            Destroy(sbh.gameObject);
        songButtons.Clear();

        content.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, titles.Length * songButtonHeight);

        for (int i = 0; i < titles.Length; i++)
        {
            GameObject newSongButton = Instantiate(songButtonPrefab, content.transform);

            RectTransform songButtonRectTransform = newSongButton.GetComponent<RectTransform>();
            songButtonRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, i * songButtonHeight, songButtonHeight);

            SongButtonHandler songButtonHandler = newSongButton.GetComponent<SongButtonHandler>();
            songButtonHandler.SetSong(titles[i]);

            songButtons.Add(songButtonHandler);
        }
    }
}
