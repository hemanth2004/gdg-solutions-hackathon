using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlugKeyScript : CircuitApparatus
{
    private bool _plugged = false;
    public bool plugged
    {
        get
        {
            return _plugged;
        }
        set
        {
            _plugged = value;
            plug.localPosition = new Vector3(0f, !value ? 0.1f : 0.04f, 0f);
        }
    }

    [SerializeField] private TMP_Dropdown pluggedDropdown;
    [SerializeField] private Transform plug;

    public void OnChangePluggedDropdown()
    {
        plugged = pluggedDropdown.value == 0;
    }


}
