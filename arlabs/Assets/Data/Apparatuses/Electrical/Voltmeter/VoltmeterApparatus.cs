using UnityEngine;
using ARLabs.Core;
using TMPro;

public class VoltmeterApparatus : CircuitApparatus
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

    [SerializeField] private TextMeshPro digitalText;
    [SerializeField] private GameObject AnalogMeterObject, DigitalMeterObject;
    [Range(0f, 1f)]
    public float fillAmount;

    [SerializeField] private Transform analogNeedle;
    [SerializeField] private Vector3 eulerStart, eulerEnd;
    [SerializeField] private float _measuredVoltage = 0f;
    [SerializeField] private GameObject[] _resistiveWirePrefabs;

    public ResistanceApparatus AttachedWire;

    protected override void OnStart()
    {
        base.OnStart();

        Fields.DropdownFields["displayType"].OnChange += OnChangeDisplayType;
        Fields.DropdownFields["wireType"].OnChange += OnChangeResistiveWireType;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (AttachedWire != null)
            _measuredVoltage = CircuitManager.Instance.NetCurrent * AttachedWire.ResistanceValue;
        else
            _measuredVoltage = 0f;

        fillAmount = Mathf.Abs(_measuredVoltage) / 10f;
        analogNeedle.localRotation
            = Quaternion.Euler(
                Vector3.Lerp(eulerStart, eulerEnd, fillAmount)
              );

        digitalText.text = _measuredVoltage.ToString("0.0");
    }

    private void OnChangeDisplayType(object value)
    {
        isAnalog = (int)value == 0;
    }

    private void OnChangeResistiveWireType(object value)
    {
        if ((int)value == 0) DetachWire();
        else AttachWire((int)value - 1);
    }

    [ContextMenu("Attach Wire")]
    private void AttachWire(int index)
    {
        if (AttachedWire != null) Destroy(AttachedWire.gameObject);
        AttachedWire = Instantiate(_resistiveWirePrefabs[index], transform).GetComponent<ResistanceApparatus>();
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
