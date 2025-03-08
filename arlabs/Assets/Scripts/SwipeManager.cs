using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : MonoBehaviour
{
    public Vector2 fingerDown;
    public Vector2 fingerUp;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.fingerDown = Input.mousePosition;
            this.fingerUp = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            this.fingerUp = Input.mousePosition;
        }
    }
}
