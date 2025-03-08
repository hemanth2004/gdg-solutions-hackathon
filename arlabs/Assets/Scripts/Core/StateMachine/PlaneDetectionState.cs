using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;
using KG.StateMachine;

public class PlaneDetectionState : StateBase<ExperimentManager>
{
    public void OnEnter(ExperimentManager entity)
    {
        entity.planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        entity.SetPlanesVisibility(true);
    }
    public void OnExit(ExperimentManager entity)
    {
        entity.planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
        entity.SetPlanesVisibility(false);
    }
}
