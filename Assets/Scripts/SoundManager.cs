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
    AudioSource song;
    float songDelay;

    public Sound[] sounds;

    void Awake()
    {
        song = gameObject.AddComponent<AudioSource>();
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void LoadSong(Song s)
    {
        song.clip = s.Clip;
        songDelay = PlayManager.BASE_SONG_WAIT + s.offset + GlobalManager.instance.syncOffset;
    }

    public void PlaySong()
    {
        song.PlayDelayed(songDelay);
    }

    public void StopSong()
    {
        song.Stop();
    }

    public void SetSongVolume(float volume)
    {
        song.volume = volume;
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
}
