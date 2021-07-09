using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteInfoHandler : MonoBehaviour
{
    Button closeButton;
    InputField startField;
    InputField stopField;
    Button removeButton;

    void Awake()
    {
        closeButton = transform.Find("Close Button").GetComponent<Button>();
        startField = transform.Find("Start Field").GetComponent<InputField>();
        stopField = transform.Find("Stop Field").GetComponent<InputField>();
        removeButton = transform.Find("Remove Button").GetComponent<Button>();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void SetStart(float start)
    {
        startField.text = start.ToString();
    }

    public void SetStop(float stop)
    {
        stopField.text = stop.ToString();
    }
}
