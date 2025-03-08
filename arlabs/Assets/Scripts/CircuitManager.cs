using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEditor;
using UnityEngine;

public partial class CircuitManager : MonoBehaviour
{
    #region Singleton
    public static CircuitManager Instance;

    private void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(this); }
    } 
    #endregion

    [SerializeField] private bool _debug = false;

    private List<Path> _pathBuffer = new List<Path>();
    private List<Path> _validPaths = new List<Path>();
    private List<Knob> _allKnobs = new List<Knob>();

    private Knob _batteryPositive, _batteryNegative;
    private BatteryScript _battery;
    private PlugKeyScript _plugKey;

    public float NetCurrent;

    public Color HighVoltageColor = Color.red;
    public Color LowVoltageColor = Color.green;

    public bool HasPath => _validPaths.Count > 0;

    public bool HasPower = false;

    public void UpdateReferences()
    {
        if (_battery == null) _battery = FindObjectOfType<BatteryScript>();
        if (_plugKey == null) _plugKey = FindObjectOfType<PlugKeyScript>();
    }

    private void Update()
    {
        if (_battery != null)
        {
            if (_plugKey == null)
                HasPower = _battery.on;
            else
                HasPower = _battery.on && _plugKey.plugged;
        }

        if (HasPower && _validPaths.Count == 0)
            UpdatePaths();

        // TODO: Call this method when new apparatus is places/removed
        UpdateReferences();
        if (_battery != null && _validPaths.Count > 0)
            NetCurrent = _battery.Voltage / _validPaths[0].GetPathResistance();
        else
            NetCurrent = 0;

        // TODO: Call this approrpriately (while visulaistaions are running)
        UpdateWireVoltages();
    }

    // TODO: Call when there is change in app state (wire added/removed, etc) that actually change the circuit (not repositioning)

    [ContextMenu("Update Paths")]
    public void UpdatePaths()
    {
        if (_battery == null) return;

        if (!HasPower) return;

        _allKnobs = FindObjectsOfType<Knob>().ToList();

        var bKnobs = _battery.GetComponentsInChildren<Knob>();
        foreach (var b in bKnobs)
        {
            if (b.IsPositive) _batteryPositive = b;
            else _batteryNegative = b;
        }

        _validPaths = new List<Path>();
        _pathBuffer = new List<Path>();

        Path path0 = new Path(new List<Knob> { _batteryPositive });
        _pathBuffer.Add(path0);

        for (int p = 0;  p < _pathBuffer.Count; p++)
        {
            if (_debug) Debug.Log($"START PATH {p}");
            Path curPath = _pathBuffer[p];

            for (int i = 0; i < curPath.PathList.Count; i++)
            {
                
                Knob curKnob = curPath.PathList[i];
                if (_debug)
                {
                    Debug.Log("Analyzing" + GetKnobLog(curKnob));

                    PrintElements(curPath.PathList, "Path: ");
                    PrintElements(curPath.Visited, "Visisted: ");
                    PrintElements(curPath.PrevPath, "Prev Path: ");
                }

                List<Knob> validNeighbours = new List<Knob>();
                foreach (Knob k in curKnob.NeighbourKnobs) { if (!curPath.Visited.Contains(k)) {  validNeighbours.Add(k);} }
                if (_debug) { PrintElements(curKnob.NeighbourKnobs, "All Neighbours: "); PrintElements(validNeighbours, "Valid Neighbours: "); }

                List<Knob> fullPath = new List<Knob>(curPath.PrevPath);
                fullPath.AddRange(curPath.PathList);

                if (fullPath[0] == _batteryPositive && fullPath[fullPath.Count - 1] ==  _batteryNegative)
                {
                    if (_debug)
                    {
                        Debug.Log("PATH FOUND!");
                        PrintElements(fullPath, "Full Path: ");
                    }
                    Path finalPath = new Path(fullPath);
                    finalPath.FinalizePath();
                    _validPaths.Add(finalPath);
                    break;
                }

                if (validNeighbours.Count == 0) { break; }

                for (int j = 0; j < validNeighbours.Count - 1; j++)
                {
                    if (_debug) Debug.Log($"Duplicating Path #{j + 1}");
                    Knob n = validNeighbours[j];
                    Path newPath = new Path(new List<Knob> { n }, new List<Knob>(curPath.Visited) { n }, new List<Knob>(curPath.PathList));
                    _pathBuffer.Add(newPath);
                    if (_debug) { PrintElements(newPath.PathList, "New Path's Path List: "); PrintElements(newPath.Visited, "New Path's Visited: "); }
                }

                curPath.Visited.Add(curKnob);
                Knob next = validNeighbours[validNeighbours.Count - 1];

                //TODO: Add polarity check

                curPath.PathList.Add(next);

            }

            if (_debug) Debug.Log($"END PATH {p}");
        }

        if (_debug)
        {
            LogValidPaths();
        }
    }

    public List<List<Knob>> GetValidKnobPaths()
    {
        List<List<Knob>> retVal = new List<List<Knob>>();

        for (int i = 0; i < _validPaths.Count; i++)
        {
            retVal.Add(_validPaths[i].GetFullPath());
        }

        return retVal;
    }

    [ContextMenu("Log Valid Paths")]
    private void LogValidPaths()
    {
        Debug.Log("VALID PATHS: ");
        foreach (Path path in _validPaths)
        {
            PrintElements(path.PathList, "");
        }
    }

    private void PrintElements(List<Knob> knobList, string listName)
    {
        string log = listName + " ";
        foreach (Knob knob in knobList) 
        {
            log += GetKnobLog(knob);
            log += ", ";
        }

        Debug.Log(log);
    }

    private string GetKnobLog(Knob knob)
    {
        string log = knob.transform.parent.name;
        log += knob.IsPositive ? "+" : "-";

        return log;
    }

    public void UpdateWireVoltages()
    {
        if (_battery == null) return;

        if (_validPaths.Count > 0)
            _validPaths[0].AssignWireVoltages(_battery.Voltage);
    }

    // USE THESE METHODS TO CONTROL VISUALISATIONS
    public void StartWireElectronVisualisations()
    {
        foreach (var path in _validPaths)
        {
            foreach (var w in path.PathWires)
            {
                w.StartElectronVis();
            }
        }
        Debug.Log("OnE");
    }
    public void StartWirePotentialVisualisations()
    {
        foreach (var path in _validPaths)
        {
            foreach (var w in path.PathWires)
            {
                w.StartPotentialVis();
            }
        }
    }

    public void StopWireElectronVisualisations()
    {
        foreach (var path in _validPaths)
        {
            foreach (var w in path.PathWires)
            {
                w.StopElectronVis();
            }
        }
        Debug.Log("OnE");
    }
    public void StopWirePotentialVisualisations()
    {
        foreach (var path in _validPaths)
        {
            foreach (var w in path.PathWires)
            {
                w.StopPotentialVis();
            }
        }
    }
}
