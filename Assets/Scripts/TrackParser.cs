using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public struct Note
{
    public int lane;
    public float beatPos;
    public float start;
    public float stop;
}

// TODO: Save tracks to Resources

public class TrackParser : MonoBehaviour
{
    BinaryFormatter bf = new BinaryFormatter();

    public void SaveTrack(Note[] notes, string fileName) { }

    public Note[] LoadTrack(string fileName)
    {
        return null;
    }

    public Note[] ParseTrack(TextAsset trackFile)
    {
        using (MemoryStream ms = new MemoryStream(trackFile.bytes))
        {
            Note[] notes = bf.Deserialize(ms) as Note[];
            return notes;
        }
    }
}
