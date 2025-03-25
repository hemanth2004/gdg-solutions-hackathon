using UnityEngine;
using ARLabs.Core;

public class ElectronViz : Visualization
{

    private void Start()
    {
        VisualizationName = "electron_flow";
    }


    public override bool CheckIfVisualizationIsPossible()
    {
        return CircuitManager.Instance.HasPower;
    }

    public override void BeginVisualization()
    {
        base.BeginVisualization();
        CircuitManager.Instance.StartWireElectronVisualisations();
    }

    public override void EndVisualization()
    {
        base.EndVisualization();
        CircuitManager.Instance.StopWireElectronVisualisations();
    }
}
