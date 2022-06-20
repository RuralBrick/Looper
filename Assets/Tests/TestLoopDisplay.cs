using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class TestLoopDisplay
{
    const float RADIUS = 4f;
    const float DOWNBEAT_LINE_WIDTH = 0.1f;

    const float NOTE_WIDTH = 0.7f;
    const float LANE_WIDTH = 0.75f;

    const float TRACK_WIDTH = LoopDisplayHandler.LANE_COUNT * LANE_WIDTH;
    const float INNER_GAP = RADIUS - TRACK_WIDTH;

    const int beatsPerBar = 4;

    LoopDisplayHandler loopDisplayHandler;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Loop Display.prefab", typeof(GameObject))
        ) as GameObject;
        loopDisplayHandler = gameObject.GetComponent<LoopDisplayHandler>();
        loopDisplayHandler.Initialize(beatsPerBar);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(loopDisplayHandler.gameObject);
    }

    [UnityTest]
    public IEnumerator CalcNotePosition_Lane0_beatPos0()
    {
        GameObject gameObject = new GameObject();
        LoopDisplayHandler loopDisplayHandler = gameObject.AddComponent<LoopDisplayHandler>();
        loopDisplayHandler.Initialize(beatsPerBar);

        Vector3 expectedPos = new Vector3(0, INNER_GAP + LANE_WIDTH / 2f, 0);
        Vector3 actualPos = loopDisplayHandler.CalcNotePosition(0, 0);
        Assert.AreEqual(expectedPos, actualPos);

        yield return null;
    }
}
