using UnityEngine;
using ARLabs.Core;
using TMPro;

public class AmmeterApparatus : CircuitApparatus
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

    protected override void OnStart()
    {
        base.OnStart();

        Fields.DropdownFields["displayType"].OnChange += OnChangeDisplayType;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        fillAmount = CircuitManager.Instance.NetCurrent / 10f;
        analogNeedle.localRotation
            = Quaternion.Euler(
                Vector3.Lerp(eulerStart, eulerEnd, fillAmount)
              );

        digitalText.text = CircuitManager.Instance.NetCurrent.ToString("0.00");
    }

    private void OnChangeDisplayType(object value)
    {
        isAnalog = (int)value == 0;
    }   

}
