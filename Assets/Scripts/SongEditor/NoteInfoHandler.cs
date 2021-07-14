using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteInfoHandler : MonoBehaviour
{
    Button closeButton;
    InputField startField;
    InputField startAdjustField;
    InputField stopField;
    InputField stopAdjustField;
    Button removeButton;

    void Awake()
    {
        closeButton = transform.Find("Close Button").GetComponent<Button>();
        startField = transform.Find("Start Field").GetComponent<InputField>();
        startAdjustField = transform.Find("Start Adjust Field").GetComponent<InputField>();
        stopField = transform.Find("Stop Field").GetComponent<InputField>();
        stopAdjustField = transform.Find("Stop Adjust Field").GetComponent<InputField>();
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

    public void ClearStartAdjust()
    {
        startAdjustField.text = "";
    }

    public void SetStop(float stop)
    {
        stopField.text = stop.ToString();
    }

    public void ClearStopAdjust()
    {
        stopAdjustField.text = "";
    }
}
