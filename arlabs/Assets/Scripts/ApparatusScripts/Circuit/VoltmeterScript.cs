using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoltmeterScript : CircuitApparatus
{

    private bool _isAnalog = true;
    public bool isAnalog
    {
        get
        {
            return _isAnalog;
        }
        set
        {
            _isAnalog = value;

            AnalogMeterObject.SetActive(value);
            DigitalMeterObject.SetActive(!value);

        }
    }

    [SerializeField] private TextMeshPro analogMultiplierLabel, digitalText;
    [SerializeField] private GameObject AnalogMeterObject, DigitalMeterObject;
    [Range(0f, 1f)]
    public float fillAmount;

    [SerializeField] private Transform analogNeedle;
    [SerializeField] private Vector3 eulerStart, eulerEnd;
    [SerializeField] private float _measuredVoltage = 0f;

    [SerializeField] private TMP_Dropdown displayTypeDropdown;
    [SerializeField] private TMP_Dropdown wireTypeDropdown;
    [SerializeField] private GameObject[] _resistiveWirePrefabs;

    public Resistance AttachedWire;

    private void Update()
    {

        if (AttachedWire != null)
            _measuredVoltage = CircuitManager.Instance.NetCurrent * AttachedWire.ResistanceValue;
        else
            _measuredVoltage = 0f;

        fillAmount = Mathf.Abs(_measuredVoltage) / 10f;
        analogNeedle.localRotation
            = Quaternion.Euler(
                Vector3.Lerp(eulerStart, eulerEnd, fillAmount)
              );

        digitalText.text = _measuredVoltage.ToString("0.00");
    }

    public void OnChangeDisplayType()
    {
        isAnalog = displayTypeDropdown.value == 0;
    }

    public void OnChangeResistiveWireType()
    {
        if (wireTypeDropdown.value == 0) DetachWire();
        else AttachWire(wireTypeDropdown.value - 1);
    }

    [ContextMenu("Attach Wire")]
    private void AttachWire(int index)
    {
        if (AttachedWire != null) Destroy(AttachedWire.gameObject);
        AttachedWire = Instantiate(_resistiveWirePrefabs[index], transform).GetComponent<Resistance>();
        AttachedWire.transform.localPosition = Vector3.zero;
        AttachedWire.transform.localRotation = Quaternion.identity;
        AttachedWire.transform.localScale = Vector3.one;
    }

    [ContextMenu("Detach Wire")]
    private void DetachWire()
    {
        Destroy(AttachedWire.gameObject);
        AttachedWire = null;
    }
}
