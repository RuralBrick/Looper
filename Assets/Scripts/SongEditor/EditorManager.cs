using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    const int LANE_COUNT = 4;
    const float LANE_WIDTH = 0.75f;

    Song currentSong;
    int currentBar;

    LoopDisplayHandler ldm;
    SongDisplayManager sdm;

    InputField offsetField;
    Slider offsetSlider;

    void Awake()
    {
        ldm = FindObjectOfType<LoopDisplayHandler>();
        sdm = FindObjectOfType<SongDisplayManager>();
        offsetField = GameObject.Find("Offset Field").transform.GetComponent<InputField>();
        offsetSlider = GameObject.Find("Offset Slider").transform.GetComponent<Slider>();
    }

    void Start()
    {
        ldm.HideMetronome();
    }

    public void LoadSong(Song s)
    {
        currentSong = s;

        ldm.Initialize(s.beatsPerBar, LANE_COUNT, LANE_WIDTH);
        
        sdm.Initialize(s);

        GameObject.Find("Title Field").transform.GetComponent<InputField>().text = s.title;
        offsetField.text = s.offset.ToString();
        offsetSlider.value = s.offset;

        currentBar = 0;
        sdm.DisplayBar(currentBar);
    }

    int CurrentBar
    {
        get => currentBar;
        set
        {
            if (value < 0) currentBar = 0;
            else currentBar = value;
            sdm.DisplayBar(currentBar);
        }
    }

    public void DisplayNextBar()
    {
        CurrentBar++;
    }

    public void DisplayPrevBar()
    {
        CurrentBar--;
    }

    public void DisplayNextPhrase()
    {
        CurrentBar += SongDisplayManager.BarsDisplayed();
    }

    public void DisplayPrevPhrase()
    {
        CurrentBar -= SongDisplayManager.BarsDisplayed(); 
    }

    public void SetOffset(string textOffset)
    {
        float newOffset;
        if (float.TryParse(textOffset, out newOffset))
        {
            currentSong.offset = newOffset;
            offsetSlider.value = newOffset;
            sdm.SetOffset(currentBar, newOffset);
        }
    }

    public void SetOffset(float newOffset)
    {
        currentSong.offset = newOffset;
        offsetField.text = newOffset.ToString();
        sdm.SetOffset(currentBar, newOffset);
    }
}
