using UnityEngine;
using ARLabs.Core;

public class PlugKeyApparatus : CircuitApparatus
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

    [SerializeField] private Transform plug;

    protected override void OnStart()
    {
        base.OnStart();

        Fields.DropdownFields["plugged"].OnChange += OnChangePluggedDropdown;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    public void OnChangePluggedDropdown(object value)
    {
        plugged = (int) value == 1;
    }
}
