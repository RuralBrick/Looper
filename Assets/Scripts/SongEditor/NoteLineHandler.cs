using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteLineHandler : MonoBehaviour
{
    static List<NoteLineHandler> noteLineHandlers = new List<NoteLineHandler>();

    LineRenderer lineRenderer;

    public GameObject hitMarkPrefab;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        noteLineHandlers.Add(this);
    }

    public static void DestroyNoteLines()
    {
        foreach (var nlh in noteLineHandlers)
        {
            Destroy(nlh.gameObject);
        }
        noteLineHandlers.Clear();
    }

    public void SetColor(Color color)
    {
        lineRenderer.startColor = lineRenderer.endColor = color;
    }

    public void SetPositions(float left, float right)
    {
        lineRenderer.SetPositions(new Vector3[] { Vector3.right * left, Vector3.right * right });
    }

    public void SpawnHitMarks(Color color, float start, float stop, float limit, float step)
    {
        for (float x = start; x <= stop && x < limit; x += step)
        {
            HitMarkHandler hitMark = Instantiate(hitMarkPrefab, transform).GetComponent<HitMarkHandler>();
            hitMark.SetColor(color);
            hitMark.SetPositions(x, limit);
        }
    }
}
