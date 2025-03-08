using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircuitApparatus : ApparatusOld
{
    [SerializeField] private int _minKnobsNeededToTurnOn = 2;
    public bool IsInstrumentOn;

    public Knob[] knobs;

    private void Awake()
    {
        knobs = GetComponentsInChildren<Knob>();
    }

    private void Update()
    {
        IsInstrumentOn = CheckIfOn();

        OnUpdate();
    }

    protected virtual void OnUpdate()
    {

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
