using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHandler : MonoBehaviour
{
    const float SECONDS_PER_MINUTE = 60f;
    const float BEATS_ANTICIPATION = 2f;
    const float SIZE_INCREASE = 1.1f;
    const float SIZE_CHANGE_TIME = 0.1f;

    SpriteRenderer spriteRenderer;

    float stop;
    float fadeTime;
    List<float> hits;

    Coroutine fadeOut;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        StartCoroutine(FadeIn());
    }

    public bool ShouldDespawn(float beat)
    {
        return beat > stop;
    }

    public void Disappear(ListRemove listRemove)
    {
        if (fadeOut == null)
            fadeOut = StartCoroutine(FadeOut(listRemove));
    }

    // TODO: Figure out misses
    
    public bool GetHit(float beat, PlayManager.HitRange[] hitRanges, out PlayManager.HitRangeType hit, out bool late)
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
        
        hit = PlayManager.HitRangeType.None;
        late = false;

        return false;
    }

    IEnumerator FadeIn()
    {
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;

        yield return new WaitForEndOfFrame();

        float timePassed = 0f;

        while (timePassed < fadeTime)
        {
            timePassed += Time.deltaTime;
            color.a = timePassed / fadeTime;
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 1f;
        spriteRenderer.color = color;

        yield return null;
    }

    public delegate bool ListRemove(NoteHandler nh);

    IEnumerator FadeOut(ListRemove lastCall)
    {
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;

        yield return new WaitForEndOfFrame();

        float timeLeft = fadeTime;

        while (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            color.a = timeLeft / fadeTime;
            spriteRenderer.color = color;
            yield return null;
        }

        if (!lastCall(this))
            Debug.LogWarning("Could not remove note handler");
        Destroy(gameObject);

        yield return null;
    }
    
    IEnumerator Pop()
    {
        Vector3 origSize = transform.localScale;
        transform.localScale = origSize * SIZE_INCREASE;

        yield return new WaitForSeconds(SIZE_CHANGE_TIME);

        transform.localScale = origSize;

        yield return null;
    }
}
