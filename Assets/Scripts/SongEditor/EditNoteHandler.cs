using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditNoteHandler : MonoBehaviour
{
    SpriteRenderer sr;

    public Color normalColor;
    public Color highlightedColor;
    public Color pressedColor;
    public Color selectedColor;

    [HideInInspector]
    public EditorManager em;
    [HideInInspector]
    public Ref<Note> info;
    bool selected = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        sr.color = normalColor;
    }

    void OnMouseEnter()
    {
        if (!selected)
            sr.color = highlightedColor;
    }

    void OnMouseExit()
    {
        if (!selected)
            sr.color = normalColor;
    }

    void OnMouseDown()
    {
        sr.color = pressedColor;
    }

    void OnMouseUpAsButton()
    {
        em.SelectNote(this);
    }

    public void Select()
    {
        selected = true;
        sr.color = selectedColor;
    }

    public void Deselect()
    {
        selected = false;
        sr.color = normalColor;
    }
}
