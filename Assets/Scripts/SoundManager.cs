using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;
    [HideInInspector]
    public AudioSource source;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;
}

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;

    [HideInInspector]
    public AudioLowPassFilter lowPassFilter;
    public const float maxFreq = 22000;
    public const float minFreq = 10;

    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public Sound Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return null;
        }
        s.source.volume = s.volume;
        s.source.Play();
        return s;
    }

    IEnumerator PlayLater(Sound s, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        s.source.volume = s.volume;
        s.source.Play();
        yield return null;
    }

    public Coroutine PlayLoop(string start, string loop)
    {
        Sound s1 = Array.Find(sounds, sound => sound.name == start);
        Sound s2 = Array.Find(sounds, sound => sound.name == loop);
        if (s1 == null)
        {
            Debug.LogWarning("Start: " + start + " not found");
            return null;
        }
        if (s2 == null)
        {
            Debug.LogWarning("Loop: " + loop + " not found");
            return null;
        }
        s1.source.volume = s1.volume;
        float delay = s1.clip.length;
        Coroutine cr = StartCoroutine(PlayLater(s2, delay));
        s1.source.Play();
        return cr;
    }

    public Sound Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return null;
        }
        s.source.Stop();
        return s;
    }

    public bool isPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return false;
        }
        return s.source.isPlaying;
    }

    public Sound ChangeVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return null;
        }
        s.volume = volume;
        s.source.volume = s.volume;
        return s;
    }

    enum FadeDirection { IN, OUT };
    IEnumerator Fade(Sound s, FadeDirection dir, float time)
    {
        if (dir == FadeDirection.IN)
        {
            s.source.volume = s.volume * 0.1f;
            s.source.Play();
        }
        float start = s.source.volume;
        float end = dir == FadeDirection.IN ? s.volume : 0;
        float timeToVolume = (end - start) / time;
        float timePassed = 0;
        float currVolume;
        while (timePassed < time)
        {
            currVolume = start + timePassed * timeToVolume;
            if (currVolume < 0 || currVolume > s.volume)
                currVolume = end;
            s.source.volume = currVolume;
            timePassed += Time.deltaTime;
            yield return null;
        }
        s.source.volume = end;
        if (dir == FadeDirection.OUT)
            s.source.Stop();
        yield return null;
    }

    public Sound FadeOut(string name, float time)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return null;
        }
        StartCoroutine(Fade(s, FadeDirection.OUT, time));
        return s;
    }

    public Sound FadeIn(string name, float time)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return null;
        }
        StartCoroutine(Fade(s, FadeDirection.IN, time));
        return s;
    }

    public Coroutine FadeInLoop(string start, float time, string loop)
    {
        Sound s1 = Array.Find(sounds, sound => sound.name == start);
        Sound s2 = Array.Find(sounds, sound => sound.name == loop);
        if (s1 == null)
        {
            Debug.LogWarning("Start: " + start + " not found");
            return null;
        }
        if (s2 == null)
        {
            Debug.LogWarning("Loop: " + loop + " not found");
            return null;
        }
        float delay = s1.clip.length;
        Coroutine cr = StartCoroutine(PlayLater(s2, delay));
        StartCoroutine(Fade(s1, FadeDirection.IN, time));
        return cr;
    }

    IEnumerator Fade(AudioLowPassFilter filter, float time, float? targetFreq = null)
    {
        if (targetFreq != null)
        {
            filter.cutoffFrequency = maxFreq;
            filter.enabled = true;
        }
        float start = filter.cutoffFrequency;
        float end = targetFreq != null ? (float)targetFreq : maxFreq;
        float timeToFreq = (end - start) / time;
        float timePassed = 0;
        while (timePassed < time)
        {
            filter.cutoffFrequency = start + timePassed * timeToFreq;
            timePassed += Time.deltaTime;
            yield return null;
        }
        filter.cutoffFrequency = end;
        if (targetFreq == null)
            filter.enabled = false;
        yield return null;
    }

    public void ActivateLowPassFilter(float time, float targetFrequency)
    {
        if (lowPassFilter == null)
        {
            Debug.LogWarning("Low Pass Filter not found");
            return;
        }
        StartCoroutine(Fade(lowPassFilter, time, targetFrequency));
    }

    public void DeactivateLowPassFilter(float time)
    {
        if (lowPassFilter == null)
        {
            Debug.LogWarning("Low Pass Filter not found");
            return;
        }
        StartCoroutine(Fade(lowPassFilter, time));
    }
}
