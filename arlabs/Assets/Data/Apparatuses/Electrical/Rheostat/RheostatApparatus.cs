using UnityEngine;
using ARLabs.Core;

public class RheostatApparatus : ResistanceApparatus
{

    [SerializeField] private Transform _resistanceSliderTransform;
    [SerializeField] private float _minSliderY = 0.529f;
    [SerializeField] private float _maxSliderY = -0.417f;

    protected override void OnStart()
    {
        base.OnStart();

        Fields.SliderFields["resistance"].OnChange += OnChangeResistance;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    private void OnChangeResistance(object value)
    {
        ResistanceValue = (float) value;
        float MaxValue = Fields.SliderFields["resistance"].rangeMax;
        float MinValue = Fields.SliderFields["resistance"].rangeMin;

        Vector3 newSliderPos = _resistanceSliderTransform.localPosition;
        newSliderPos.y = Mathf.Lerp(_minSliderY, _maxSliderY, ResistanceValue / (MaxValue - MinValue));
        _resistanceSliderTransform.localPosition = newSliderPos;
    }
}
