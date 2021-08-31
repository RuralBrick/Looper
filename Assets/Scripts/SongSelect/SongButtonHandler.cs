using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongButtonHandler : MonoBehaviour
{
    Button button;
    Text text;

    void Awake()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<Text>();
    }

    public void SetSong(string title)
    {
        Button.ButtonClickedEvent newOnClick = new Button.ButtonClickedEvent();
        newOnClick.AddListener(delegate { GlobalManager.instance.SelectSong(title); });
        button.onClick = newOnClick;

        text.text = title;
    }
}
