using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;
using KG.StateMachine;

public class PlaneDetectionState : StateBase<ExperimentManager>
{
    public override void OnEnter(ExperimentManager entity)
    {
        Debug.Log("Starting plane detection");
        entity.planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        entity.SetPlanesVisibility(true);
    }
    public override void OnExit(ExperimentManager entity)
    {
        Debug.Log("Stopping plane detection");
        entity.planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
        entity.SetPlanesVisibility(false);
    }
}
