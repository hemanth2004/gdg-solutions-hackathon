using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AmmeterScript : CircuitApparatus
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

    [SerializeField] private TMP_Dropdown displayTypeDropdown;

    private void Update()
    {
        fillAmount = CircuitManager.Instance.NetCurrent / 10f;
        analogNeedle.localRotation
            = Quaternion.Euler(
                Vector3.Lerp(eulerStart, eulerEnd, fillAmount)
              );
        digitalText.text = CircuitManager.Instance.NetCurrent.ToString("0.00");
    }

    public void OnChangeDisplayType()
    {
        isAnalog = displayTypeDropdown.value == 0;
    }
}
