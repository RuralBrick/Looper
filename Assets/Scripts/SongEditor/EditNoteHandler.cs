using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditNoteHandler : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public Color normalColor;
    public Color highlightedColor;
    public Color pressedColor;
    public Color selectedColor;

    [HideInInspector]
    public EditorManager editorManager;
    [HideInInspector]
    public Ref<Note> info;
    bool selected = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        spriteRenderer.color = normalColor;
    }

    void OnMouseEnter()
    {
        if (!selected)
            spriteRenderer.color = highlightedColor;
    }

    void OnMouseExit()
    {
        if (!selected)
            spriteRenderer.color = normalColor;
    }

    void OnMouseDown()
    {
        spriteRenderer.color = pressedColor;
    }

    void OnMouseUpAsButton()
    {
        editorManager.SelectNote(this);
    }

    public void Select()
    {
        selected = true;
        spriteRenderer.color = selectedColor;
    }

    public void Deselect()
    {
        selected = false;
        spriteRenderer.color = normalColor;
    }
}
