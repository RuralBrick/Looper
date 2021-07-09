using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceholderHandler : MonoBehaviour
{
    SpriteRenderer sr;

    public int lane;
    public float beatPos;
    public EditorManager em;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        sr.enabled = false;
    }

    void OnMouseEnter()
    {
        sr.enabled = true;
    }

    void OnMouseExit()
    {
        sr.enabled = false;
    }

    void OnMouseUpAsButton()
    {
        em.AddNote(lane, beatPos);
    }
}
