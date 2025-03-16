using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BatteryScript : CircuitApparatusOld
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

    [SerializeField] private TMP_Dropdown onOffDropdown;
    [SerializeField] private Slider voltageSlider;
    [SerializeField] private TMP_Text voltageHead;
    [SerializeField] private Renderer onIndicator;

    public void OnChangeVoltageSlider()
    {
        Voltage = voltageSlider.value;
        voltageHead.text = "Voltage (" + Voltage.ToString() + "V)";
    }
    public void OnChangeOnDropdown()
    {
        on = onOffDropdown.value == 0;
    }
}
