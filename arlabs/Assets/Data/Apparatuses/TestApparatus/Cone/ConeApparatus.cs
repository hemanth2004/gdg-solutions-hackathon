using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;

public class ConeApparatus : Apparatus
{
    [SerializeField] private GameObject highlightSphere;
    protected override void OnStart()
    {
        base.OnStart();
        Fields.SliderFields["size"].OnChange += OnChangeScale;
        Fields.BoolFields["highlight"].OnChange += OnChangeHighlight;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    private void OnChangeScale(object value)
    {
        transform.localScale = Vector3.one * (float) value;
    }

    private void OnChangeHighlight(object state)
    {
        highlightSphere.SetActive((bool) state);
    }
}
