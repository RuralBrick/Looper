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
    public void SaveTrack(Note[] notes, string fileName)
    {
        string filePath = Application.persistentDataPath + "/" + fileName + ".loop";
        FileStream fs = new FileStream(filePath, FileMode.Create);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, notes);

        fs.Close();
    }

    public Note[] LoadTrack(string fileName)
    {
        string filePath = Application.persistentDataPath + "/" + fileName + ".loop";

        if (!File.Exists(filePath))
        {
            Debug.LogWarningFormat($"File {fileName} not found");
            return null;
        }

        FileStream fs = new FileStream(filePath, FileMode.Open);

        BinaryFormatter bf = new BinaryFormatter();
        Note[] notes = bf.Deserialize(fs) as Note[];

        fs.Close();

        return notes;
    }
}
