using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHandler : MonoBehaviour
{
    public float beatPos;
    public float end;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Color color = sr.color;
        float alpha = 0f;
        color.a = alpha;
        sr.color = color;
        while (alpha < 1f)
        {
            // HACK
            alpha += Time.deltaTime;
            if (alpha > 1f)
                alpha = 1f;
            color.a = alpha;
            sr.color = color;
            // end HACK
            yield return null;
        }
    }
}
