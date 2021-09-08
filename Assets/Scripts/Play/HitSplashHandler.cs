using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitSplashHandler : MonoBehaviour
{
    const float SIXTEENTH_NOTE_MULTIPLIER = 1f / 15f;
    const float EIGTH_NOTE_MULTIPLIER = 1f / 30f;
    const float QUARTER_NOTE_MULTIPLIER = 1f / 60f;

    static float fadeTime;

    Text text;
    Color baseColor;

    Coroutine fadeCoroutine;

    void Awake()
    {
        text = GetComponent<Text>();
        baseColor = text.color;
    }

    void Start()
    {
        Color color = baseColor;
        color.a = 0;
        text.color = color;
    }

    public static void Initialize(float tempo)
    {
        fadeTime = tempo * QUARTER_NOTE_MULTIPLIER;
    }

    IEnumerator Fade()
    {
        while (text.color.a > 0)
        {
            Color color = text.color;
            color.a -= fadeTime * Time.deltaTime;
            if (color.a < 0)
            {
                color.a = 0;
            }
            text.color = color;
            yield return null;
        }
    }

    public void Splash()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        Color color = text.color;
        color.a = 1;
        text.color = color;
        fadeCoroutine = StartCoroutine(Fade());
    }
}
