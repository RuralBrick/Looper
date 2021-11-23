using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector3 mousePos;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GlobalManager.ToggleEscMenu();
        }

        Vector3 rawMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(rawMousePos.x, rawMousePos.y, 0f);

        if (Input.GetButtonDown("Lane0")) GlobalManager.instance.LanePressed(0);
        if (Input.GetButtonDown("Lane1")) GlobalManager.instance.LanePressed(1);
        if (Input.GetButtonDown("Lane2")) GlobalManager.instance.LanePressed(2);
        if (Input.GetButtonDown("Lane3")) GlobalManager.instance.LanePressed(3);
    }
}
