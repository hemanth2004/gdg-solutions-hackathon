using UnityEngine;
using ARLabs.Core;

public class BatteryApparatus : CircuitApparatus
{
    private bool _on = false;
    public bool on
    {
        get
        {
            return _on;
        }
        set
        {
            _on = value;
            onIndicator.material.color = value ? Color.green : Color.grey;
            CircuitManager.Instance.UpdatePaths();
        }
    }

    public float Voltage = 5f;
    [SerializeField] private MeshRenderer onIndicator;

    protected override void OnStart()
    {
        base.OnStart();

        Fields.DropdownFields["onOff"].OnChange += OnChangeOnOff;
        Fields.SliderFields["voltage"].OnChange += OnChangeVoltage;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    private void OnChangeOnOff(object value)
    {
        on = (int)value == 1;
    }

    private void OnChangeVoltage(object value)
    {
        Voltage = (float)value;
    }
}
