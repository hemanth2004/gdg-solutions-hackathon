using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ARLabs.Core;

public class CircuitApparatus : Apparatus
{
    [SerializeField] private int _minKnobsNeededToTurnOn = 2;
    public bool IsInstrumentOn;

    public Knob[] knobs;

    protected override void OnStart()
    {
        base.OnStart();
        knobs = GetComponentsInChildren<Knob>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        IsInstrumentOn = CheckIfOn();
    }

    private bool CheckIfOn()
    {
        var validPaths = CircuitManager.Instance.GetValidKnobPaths();

        for (int i = 0; i < validPaths.Count; i++)
        {
            int count = 0;

            for (int j = 0; j < validPaths[i].Count; j++)
            {
                if (knobs.Contains(validPaths[i][j])) count++;
            }

            if (count >= _minKnobsNeededToTurnOn)
                return true;
        }

        return false;
    }
}