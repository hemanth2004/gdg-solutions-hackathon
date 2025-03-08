using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RheostatScript : Resistance
{
    [SerializeField] private Slider _resistanceSlider;
    [SerializeField] private TMP_Text _resistanceText;
    [SerializeField] private Transform _resistanceSliderTransform;
    [SerializeField] private float _minSliderY = 0.529f;
    [SerializeField] private float _maxSliderY = -0.417f;

    public void OnResistanceSliderUpdate()
    {
        ResistanceValue = _resistanceSlider.value;
        _resistanceText.text = "Resistance" + " (" + ResistanceValue.ToString("0.0") + " Ohms)";
        Vector3 newSliderPos = _resistanceSliderTransform.localPosition;
        newSliderPos.y = Mathf.Lerp(_minSliderY, _maxSliderY, _resistanceSlider.value / (_resistanceSlider.maxValue - _resistanceSlider.minValue));
        _resistanceSliderTransform.localPosition = newSliderPos;
    }
}
