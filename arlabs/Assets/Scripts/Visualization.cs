using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationOld : MonoBehaviour
{
    public GameObject electronVisualPrefab;
    private UserJourneyManager journeyManager;
    private List<LineRenderer> electronVisuals = new List<LineRenderer>();
    private bool _visualization = true;
    public bool visualization
    {
        get
        {
            return _visualization;
        }
        set
        {
            _visualization = value;
            UpdateWireVisuals();
        }
    }

    private void Awake()
    {
        journeyManager = GetComponent<UserJourneyManager>();
    }

    private void Start()
    {

    }

    public void UpdateWireVisuals()
    {
        //electronVisuals.Clear();
        //foreach(WireController wire in journeyManager.connectionManager._allConnectedWires)
        //{
        //    LineRenderer m = Instantiate(electronVisualPrefab).GetComponent<LineRenderer>();
        //    m.positionCount = wire._lineRenderer.positionCount;
        //    Vector3[] positions = new Vector3[wire._lineRenderer.positionCount];
        //    wire._lineRenderer.GetPositions(positions);
        //    m.SetPositions(positions);
        //    electronVisuals.Add(m);
        //}
    }
}
