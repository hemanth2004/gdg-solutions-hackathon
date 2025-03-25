using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WireController : MonoBehaviour
{
    public bool Placed; // External toggle to stop updating the points when rope is placed

    //public Transform StartPoint, EndPoint;
    public Knob StartKnob, EndKnob;

    [SerializeField] private float _bendFactor = 0.25f; // The fraction of distance to create the bend in rope (0 = no bend)
    [SerializeField] private float _wireWidth = 0.15f;

    [Tooltip("Offset from the starting to place colliders")]
    [SerializeField, Min(0)] private int _colStartIndexOffset = 1;
    [Tooltip("Offset from the end to place colliders")]
    [SerializeField, Min(0)] private int _colEndIndexOffset = 1;
    [SerializeField, Range(1, 4)] private int _numColliders = 2;
    [SerializeField] private Vector3 _startColliderSize = new Vector3(0.4f, 0.4f, 1f);
    [SerializeField] private LineRenderer _electronLineRenderer;
    [SerializeField] private Vector3 visualizationOffset;

    private LineRenderer _lineRenderer;
    private List<Vector3> _allWireSections;

    [HideInInspector] public Vector3 FingerPosition;
    public Vector2 MinMaxVoltage;

    [SerializeField] private bool _visualiseElectrons = true;
    [SerializeField] private bool _visualisePotential = true;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _allWireSections = new List<Vector3>();
    }

    private void Update()
    {
        if (!Placed)
            UpdateWire();

        // TODO: When not visualising, reset wire color
        // TODO: Change when to call
        if (CircuitManager.Instance.HasPath)
        {
            if (_visualisePotential)
                VisualizeWireVoltage();
            if (_visualiseElectrons)
                VisualiseElectrons();
        }
    }

    [ContextMenu("Place Collider")]
    public void PlaceCollider()
    {
        BoxCollider[] childCols = GetComponentsInChildren<BoxCollider>();

        int increment = (_allWireSections.Count - (_colStartIndexOffset + _colEndIndexOffset)) / _numColliders;
        for (int i = 0; i < _numColliders; i++)
        {
            int start = _colStartIndexOffset + i * increment;
            int end = start + increment;
            // If we reach end, force the last index
            if (i == _numColliders - 1)
                end = _allWireSections.Count - 1 - _colEndIndexOffset;

            SetCollider(_allWireSections[start], _allWireSections[end], childCols.Length > 0 ? childCols[i] : null);
        }
    }

    private void SetCollider(Vector3 start, Vector3 end, BoxCollider collider=null)
    {
        if (collider == null)
        {
            collider = new GameObject("Collider").AddComponent<BoxCollider>();
            collider.transform.parent = transform;
            collider.gameObject.layer = gameObject.layer;
        }

        collider.transform.position = (start + end) * 0.5f;
        collider.transform.forward = (end - start).normalized;

        Vector3 size = _startColliderSize;
        size.z = Vector3.Distance(start, end);
        collider.size = size;
    }

    public void UpdateWire()
    {
        _lineRenderer.startWidth = _wireWidth;
        _lineRenderer.endWidth = _wireWidth;


        //Update the list with rope sections by approximating the rope with a bezier curve
        Vector3 A = StartKnob.transform.position;
        Vector3 D = EndKnob != null ? EndKnob.transform.position : FingerPosition;

        float ADdistance = Vector3.Distance(A, D);
        
        //Start Bend Control point
        Vector3 B = A + StartKnob.transform.forward * ADdistance * _bendFactor;

        // End Bend Control Point
        Vector3 offset = EndKnob != null ? EndKnob.transform.forward * ADdistance * _bendFactor : Vector3.up * 0.1f;
        Vector3 C = D + offset;

        //Get the positions
        GetBezierCurve(A, B, C, D, _allWireSections);

        //An array with all rope section positions
        Vector3[] positions = new Vector3[_allWireSections.Count];
        for (int i = 0; i < _allWireSections.Count; i++)
        {
            positions[i] = _allWireSections[i];
        }

        //Add the positions to the line renderer
        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
    }

    public void SetWireColor(Color color)
    {
        GetComponent<LineRenderer>().startColor = color;
        GetComponent<LineRenderer>().endColor = color;
    }

    #region Bezier Curve
    private void GetBezierCurve(Vector3 A, Vector3 B, Vector3 C, Vector3 D, List<Vector3> allRopeSections)
    {
        //The resolution of the line
        //Make sure the resolution is adding up to 1, so 0.3 will give a gap at the end, but 0.2 will work
        float resolution = 0.1f;

        //Clear the list
        allRopeSections.Clear();

        float t = 0;

        while (t <= 1f)
        {
            //Find the coordinates between the control points with a Bezier curve
            Vector3 newPos = DeCasteljausAlgorithm(A, B, C, D, t);

            allRopeSections.Add(newPos);

            //Which t position are we at?
            t += resolution;
        }

        allRopeSections.Add(D);
    }

    //The De Casteljau's Algorithm
    private Vector3 DeCasteljausAlgorithm(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
    {
        //Linear interpolation = lerp = (1 - t) * A + t * B
        //Could use Vector3.Lerp(A, B, t)

        //To make it faster
        float oneMinusT = 1f - t;

        //Layer 1
        Vector3 Q = oneMinusT * A + t * B;
        Vector3 R = oneMinusT * B + t * C;
        Vector3 S = oneMinusT * C + t * D;

        //Layer 2
        Vector3 P = oneMinusT * Q + t * R;
        Vector3 T = oneMinusT * R + t * S;

        //Final interpolated position
        Vector3 U = oneMinusT * P + t * T;

        return U;
    }

    //private void OnDrawGizmos()
    //{
    //    if (StartPoint == null)
    //        return;
    //    if (EndPoint == null)
    //        return;

    //    //Update the list with rope sections by approximating the rope with a bezier curve
    //    Vector3 A = StartPoint.position;
    //    Vector3 D = EndPoint.position;

    //    float ADdistance = Vector3.Distance(A, D);

    //    //Start Bend Control point
    //    Vector3 B = A + StartPoint.forward * ADdistance * _bendFactor;

    //    // End Bend Control Point
    //    Vector3 C = D + EndPoint.forward * ADdistance * _bendFactor;

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(A, 0.2f);
    //    Gizmos.DrawSphere(B, 0.2f);
    //    Gizmos.DrawSphere(C, 0.2f);
    //    Gizmos.DrawSphere(D, 0.2f);

    //    Gizmos.color = Color.green;
    //    if (_allWireSections == null || _allWireSections.Count == 0)
    //    {
    //        List<Vector3> pathPoints = new List<Vector3>();
    //        GetBezierCurve(A, B, C, D, pathPoints);
    //        Gizmos.DrawLineStrip(pathPoints.ToArray(), false);
    //    } 
    //    else
    //    {
    //        Gizmos.DrawLineStrip(_allWireSections.ToArray(), false);
    //    }

    //}
    #endregion

    private void OnDrawGizmos()
    {
        if (Placed)
        {
            //Gizmos.color = Color.yellow;
            //foreach (var point in _allWireSections)
            //{
            //    Gizmos.DrawSphere(point, 0.1f);
            //}

            //Gizmos.color = Color.blue;
            //Vector3 start = _allWireSections[1];
            //Vector3 mid = _allWireSections[_allWireSections.Count / 2];
            //Gizmos.DrawLine(start, mid);

            //Gizmos.color = Color.green;
            //Gizmos.DrawSphere((start + mid) * 0.5f, 0.1f);
        }
    }

    private void VisualizeWireVoltage()
    {
        if (!CircuitManager.Instance.HasPower) return;

        BatteryScript battery = FindObjectOfType<BatteryScript>();

        var path = CircuitManager.Instance.GetValidKnobPaths()[0];
        Color highVColor = Color.Lerp(CircuitManager.Instance.LowVoltageColor, CircuitManager.Instance.HighVoltageColor, MinMaxVoltage.y / battery.Voltage);
        Color lowVColor = Color.Lerp(CircuitManager.Instance.LowVoltageColor, CircuitManager.Instance.HighVoltageColor, MinMaxVoltage.x / battery.Voltage);
        
        if (path.IndexOf(StartKnob) < path.IndexOf(EndKnob))
        {
            _lineRenderer.startColor = highVColor;
            _lineRenderer.endColor = lowVColor;
        }
        else
        {
            _lineRenderer.startColor = lowVColor;
            _lineRenderer.endColor = highVColor;
        }

        _visualisePotential = true;
    }

    private void VisualiseElectrons()
    {
        if (!CircuitManager.Instance.HasPower) return;

        _electronLineRenderer.positionCount = _lineRenderer.positionCount;
        Vector3[] positions = new Vector3[_lineRenderer.positionCount];
        _lineRenderer.GetPositions(positions);
        for(int i = 0; i < positions.Length; i++)
        {
            positions[i] += visualizationOffset;
        }
        _electronLineRenderer.SetPositions(positions);
        _electronLineRenderer.gameObject.SetActive(true);

        var path = CircuitManager.Instance.GetValidKnobPaths()[0];
        float curSpeed = _electronLineRenderer.material.GetFloat("_Speed");
        if (path.IndexOf(StartKnob) < path.IndexOf(EndKnob))
            _electronLineRenderer.material.SetFloat("_Speed", curSpeed * -1f);

        _visualiseElectrons = true;
    }

    public void StopElectronVis()
    {
        _electronLineRenderer.gameObject.SetActive(false);
        _visualiseElectrons = false;
    }

    public void StopPotentialVis()
    {
        _visualisePotential = false;
        SetWireColor(StartKnob.WireColor);
    }

    public void StartElectronVis()
    {
        _visualiseElectrons = true;
    }

    public void StartPotentialVis()
    {
        _visualisePotential = true;
    }
}
