using UnityEngine;
using ARLabs.Core;


public class BobForcesViz : Visualization
{
    [SerializeField] private float scale;
    [SerializeField] private LineRenderer gravityLine;
    [SerializeField] private LineRenderer tensionLine;
    [SerializeField] private LineRenderer normalLine;
    [SerializeField] private Transform tensionArrow;
    [SerializeField] private Transform normalArrow;
    [SerializeField] private Transform gravityArrow;

    private void Start()
    {
        VisualizationName = "bob_forces";
    }


    private PendulumApparatus _pendulum;

    private void Update()
    {
        gravityLine.enabled = tensionLine.enabled = normalLine.enabled = IsVisualizing;
        gravityArrow.gameObject.SetActive(IsVisualizing);
        tensionArrow.gameObject.SetActive(IsVisualizing);
        normalArrow.gameObject.SetActive(IsVisualizing);

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

            Camera mainCamera = Camera.main;
            Vector3 cameraDirection = mainCamera.transform.position - bobPosition;

            // Set line positions
            gravityLine.SetPosition(0, bobPosition);
            gravityLine.SetPosition(1, bobPosition + gravityVector);
            gravityArrow.position = bobPosition + gravityVector;
            gravityArrow.rotation = Quaternion.LookRotation(gravityVector,
                Vector3.ProjectOnPlane(cameraDirection, gravityVector).normalized);

            tensionLine.SetPosition(0, bobPosition);
            tensionLine.SetPosition(1, bobPosition + tensionVector);
            tensionArrow.position = bobPosition + tensionVector;
            tensionArrow.rotation = Quaternion.LookRotation(tensionVector,
                Vector3.ProjectOnPlane(cameraDirection, tensionVector).normalized);

            normalLine.SetPosition(0, bobPosition);
            normalLine.SetPosition(1, bobPosition + normalVector);
            normalArrow.position = bobPosition + normalVector;
            normalArrow.rotation = Quaternion.LookRotation(normalVector,
                Vector3.ProjectOnPlane(cameraDirection, normalVector).normalized);
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
