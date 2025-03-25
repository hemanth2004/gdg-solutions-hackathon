using UnityEngine;
using ARLabs.Core;


public class BobForcesViz : Visualization
{
    [SerializeField] private float scale;
    [SerializeField] private LineRenderer gravityLine;
    [SerializeField] private LineRenderer tensionLine;
    [SerializeField] private LineRenderer normalLine;

    private void Start()
    {
        VisualizationName = "bob_forces";
    }


    private PendulumApparatus _pendulum;

    private void Update()
    {
        if (IsVisualizing)
        {
            Vector3 bobPosition = _pendulum.BobTransform.position;
            Vector3 gravityVector = Vector3.down * _pendulum.gravity * scale;
            Vector3 stringDirection = -_pendulum.BobTransform.up; // Direction along the string

            // Calculate tension and normal components
            float gravityMagnitude = _pendulum.gravity;
            float angle = Vector3.Angle(Vector3.down, stringDirection) * Mathf.Deg2Rad;

            // Tension component (parallel to string)
            float tensionMagnitude = gravityMagnitude * Mathf.Cos(angle) * scale;
            Vector3 tensionVector = stringDirection * tensionMagnitude;

            // Normal component (perpendicular to string, always pointing downward)
            float normalMagnitude = gravityMagnitude * Mathf.Sin(angle) * scale;
            Vector3 normalDirection = Vector3.ProjectOnPlane(Vector3.down, stringDirection).normalized;
            Vector3 normalVector = normalDirection * normalMagnitude;

            // Set line positions
            gravityLine.SetPosition(0, bobPosition);
            gravityLine.SetPosition(1, bobPosition + gravityVector);

            tensionLine.SetPosition(0, bobPosition);
            tensionLine.SetPosition(1, bobPosition + tensionVector);

            normalLine.SetPosition(0, bobPosition);
            normalLine.SetPosition(1, bobPosition + normalVector);
        }
    }

    public override bool CheckIfVisualizationIsPossible()
    {
        foreach (var apparatus in ExperimentManager.Instance.InstantiatedApparatus)
        {
            if (apparatus.Head == "Pendulum")
            {
                _pendulum = apparatus.GetComponent<PendulumApparatus>();
            }
        }

        return _pendulum != null;
    }

    public override void BeginVisualization()
    {
        base.BeginVisualization();

        foreach (var apparatus in ExperimentManager.Instance.InstantiatedApparatus)
        {
            if (apparatus.Head == "Pendulum")
            {
                _pendulum = apparatus.GetComponent<PendulumApparatus>();
            }
        }
    }

    public override void EndVisualization()
    {
        base.EndVisualization();
    }
}
