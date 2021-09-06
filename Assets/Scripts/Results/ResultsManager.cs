using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsManager : MonoBehaviour
{
    Text scoreText;
    Text maxComboText;

    void Awake()
    {
        scoreText = GameObject.Find("Score Text").GetComponent<Text>();
        maxComboText = GameObject.Find("Max Combo Text").GetComponent<Text>();
    }

    public void SetText(int score, int maxCombo)
    {
        scoreText.text = $"Score: {score}";
        maxComboText.text = $"Max Combo: {maxCombo}";
    }
}
