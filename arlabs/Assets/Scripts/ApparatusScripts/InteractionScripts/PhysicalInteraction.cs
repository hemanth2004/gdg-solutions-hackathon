using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicalInteraction : MonoBehaviour
{

    protected virtual void OnFinishInteraction() { }

    public bool isInteracting;
    public void InteractionStart()
    {
        Debug.Log("Down");
        isInteracting = true;
    }

    public void InteractionEnd()
    {
        isInteracting = false;
        OnFinishInteraction();
    }


    public Vector2 touchStart;
    public Vector2 touchEnd;
}
