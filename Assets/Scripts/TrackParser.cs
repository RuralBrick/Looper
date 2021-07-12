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

public class TrackParser : MonoBehaviour
{
    string tracksPath;

    void Awake()
    {
        tracksPath = Application.dataPath + "/Tracks";
    }

    public void SaveTrack(Note[] notes, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            Debug.LogWarning("Track name cannot be blank");
            return;
        }

        string filePath = tracksPath + "/" + fileName + ".bytes";
        FileStream fs = new FileStream(filePath, FileMode.Create);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, notes);

        fs.Close();

        Debug.Log($"{fileName}.bytes saved");
    }

    public Note[] LoadTrack(string fileName)
    {
        string filePath = tracksPath + "/" + fileName + ".bytes";

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"File {fileName} not found");
            return null;
        }

        FileStream fs = new FileStream(filePath, FileMode.Open);

        BinaryFormatter bf = new BinaryFormatter();
        Note[] notes = bf.Deserialize(fs) as Note[];

        fs.Close();

        Debug.Log($"{fileName}.bytes loaded");

        return notes;
    }

    public Note[] ParseTrack(TextAsset trackFile)
    {
        MemoryStream ms = new MemoryStream(trackFile.bytes);

        BinaryFormatter bf = new BinaryFormatter();
        Note[] notes = bf.Deserialize(ms) as Note[];

        ms.Close();

        return notes;
    }
}
