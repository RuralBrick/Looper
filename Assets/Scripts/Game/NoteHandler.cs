using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHandler : MonoBehaviour
{
    const float SIZE_INCREASE = 1.1f;
    const float SIZE_CHANGE_TIME = 0.1f;

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

    public void Disappear()
    {
        StartCoroutine(FadeOut());
    }

    public void GetHit()
    {
        // TODO: Prevent multiple hits on same beat
        StartCoroutine(Pop());
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

    IEnumerator FadeOut()
    {
        Color color = sr.color;
        float alpha = 1f;
        color.a = alpha;
        sr.color = color;
        while (alpha > 0f)
        {
            // HACK
            alpha -= Time.deltaTime;
            if (alpha < 0f)
                alpha = 0f;
            color.a = alpha;
            sr.color = color;
            // end HACK
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator Pop()
    {
        Vector3 origSize = transform.localScale;
        transform.localScale = origSize * SIZE_INCREASE;

        yield return new WaitForSeconds(SIZE_CHANGE_TIME);

        transform.localScale = origSize;
    }
}
