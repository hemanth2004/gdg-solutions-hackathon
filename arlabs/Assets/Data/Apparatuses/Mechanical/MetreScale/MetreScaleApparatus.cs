using UnityEngine;
using System.Collections;
using ARLabs.Core;

public class MetreScaleApparatus : Apparatus
{
    [SerializeField] private Material scaleMaterial;
    [SerializeField] private Gradient scaleGradient;

    private Color __originalColour;

    protected override void OnStart()
    {
        base.OnStart();

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Measure String Length",
            targetApparatusType = typeof(PendulumApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => MeasureLength(from, to)
        });

        __originalColour = scaleMaterial.color;

        scaleMaterial.color = scaleGradient.Evaluate(0f);
        Fields.SliderFields["colour"].OnChange += OnChangeColour;
        Fields.TextFields["length"].value = "-";
        Fields.TextFields["length"].isReadOnly = true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    private void MeasureLength(Apparatus from, Apparatus to)
    {
        if (to is PendulumApparatus pendulum)
        {
            float length = pendulum.Fields.SliderFields["length"].value;
            Fields.TextFields["length"].value = "" + length;
        }
    }


    private void OnChangeColour(object value)
    {
        scaleMaterial.color = scaleGradient.Evaluate((float)value);
    }

    private void OnDestroy()
    {
        scaleMaterial.color = __originalColour;
    }
}
