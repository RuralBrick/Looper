using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OffsetSliderHandler : MonoBehaviour, IPointerUpHandler
{
    Slider slider;

    public EditorManager editorManager;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        editorManager.ConfirmOffset();
        slider.value = 0;
    }
}
