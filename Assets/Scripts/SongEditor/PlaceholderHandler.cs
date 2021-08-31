using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceholderHandler : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public int lane;
    public float beatPos;
    public EditorManager editorManager;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        spriteRenderer.enabled = false;
    }

    void OnMouseEnter()
    {
        spriteRenderer.enabled = true;
    }

    void OnMouseExit()
    {
        spriteRenderer.enabled = false;
    }

    void OnMouseUpAsButton()
    {
        editorManager.AddNote(lane, beatPos);
    }
}
