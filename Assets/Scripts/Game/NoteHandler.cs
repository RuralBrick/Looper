using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHandler : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const float BEATS_ANTICIPATION = 2f;
    const float SIZE_INCREASE = 1.1f;
    const float SIZE_CHANGE_TIME = 0.1f;

    SpriteRenderer sr;

    float stop;
    float fadeTime;
    List<float> hits;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public static bool ShouldSpawn(float beat, float start)
    {
        return (start - beat) < BEATS_ANTICIPATION;
    }

    public void Initialize(Note n, int beatsPerBar, float tempo)
    {
        stop = n.stop;
        fadeTime = BEATS_ANTICIPATION * SECONDS_PER_MINUTE / tempo;

        float startBar = Mathf.Floor(n.start / beatsPerBar);
        float firstHit = startBar * beatsPerBar + n.beatPos;
        if (n.start % beatsPerBar > n.beatPos) firstHit += beatsPerBar;

        hits = new List<float>();

        for (float h = firstHit; h <= n.stop; h += beatsPerBar)
            hits.Add(h);

        Debug.Log(string.Join(", ", hits));

        StartCoroutine(FadeIn());
    }

    public bool ShouldDespawn(float beat)
    {
        return beat > stop;
    }

    public void Disappear()
    {
        StartCoroutine(FadeOut());
    }

    // TODO: Figure out misses

    public bool GetHit(float beat, SongManager.HitRange[] hitRanges, out SongManager.HitRangeType hit, out bool late)
    {
        for (int i = 0; i < hits.Count; i++)
        {
            float diff = beat - hits[i];
            float dist = Mathf.Abs(diff);

            for (int j = 0; j < hitRanges.Length; j++)
            {
                if (dist <= hitRanges[j].margin)
                {
                    hit = hitRanges[j].type;
                    late = diff > 0;
                    StartCoroutine(Pop());
                    hits.RemoveAt(i);
                    return true;
                }
            }
        }

        hit = SongManager.HitRangeType.None;
        late = false;

        return false;
    }

    IEnumerator FadeIn()
    {
        Color color = sr.color;
        color.a = 0f;
        sr.color = color;

        yield return new WaitForEndOfFrame();

        float timePassed = 0f;

        while (timePassed < fadeTime)
        {
            timePassed += Time.deltaTime;
            color.a = timePassed / fadeTime;
            sr.color = color;
            yield return null;
        }

        color.a = 1f;
        sr.color = color;

        yield return null;
    }

    IEnumerator FadeOut()
    {
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;

        yield return new WaitForEndOfFrame();

        float timeLeft = fadeTime;

        while (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            color.a = timeLeft / fadeTime;
            sr.color = color;
            yield return null;
        }

        Destroy(gameObject);

        yield return null;
    }

    IEnumerator Pop()
    {
        Vector3 origSize = transform.localScale;
        transform.localScale = origSize * SIZE_INCREASE;

        yield return new WaitForSeconds(SIZE_CHANGE_TIME);

        transform.localScale = origSize;
    }
}
