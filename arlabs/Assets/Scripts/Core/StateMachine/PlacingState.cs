using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;
using KG.StateMachine;
public class PlacingState : StateBase<ExperimentManager>
{
    public void OnEnter(ExperimentManager entity)
    {
        ExperimentManager.Instance.SetPlanesVisibility(true);
    }
    public void OnExit(ExperimentManager entity)
    {
        ExperimentManager.Instance.SetPlanesVisibility(false);
    }
}
