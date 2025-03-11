using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;
using System;

public class ConeApparatus : Apparatus
{
    [SerializeField] private GameObject highlightSphere;
    protected override void OnStart()
    {
        base.OnStart();
        Fields.SliderFields["size"].OnChange += OnChangeScale;
        Fields.BoolFields["highlight"].OnChange += OnChangeHighlight;


        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Attach To Capsule",
            targetApparatusType = typeof(CapsuleApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => AttachApparatusEvent(from, to)
        });
    }



    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    private void OnChangeScale(object value)
    {
        transform.localScale = Vector3.one * (float)value;
    }

    private void OnChangeHighlight(object state)
    {
        highlightSphere.SetActive((bool)state);
    }
}
