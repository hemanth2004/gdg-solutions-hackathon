using UnityEngine;
using ARLabs.Core;

public class PotentialGradViz : Visualization
{

    private void Start()
    {
        VisualizationName = "potential_gradient";
    }


    public override bool CheckIfVisualizationIsPossible()
    {
        return CircuitManager.Instance.HasPower;
    }

    public override void BeginVisualization()
    {
        base.BeginVisualization();
        CircuitManager.Instance.StartWirePotentialVisualisations();
    }

    public override void EndVisualization()
    {
        base.EndVisualization();
        CircuitManager.Instance.StopWirePotentialVisualisations();
    }
}
