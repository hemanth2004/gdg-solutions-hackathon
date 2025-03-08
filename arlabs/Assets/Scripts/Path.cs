using System.Collections.Generic;
using UnityEngine;

public partial class CircuitManager
{
    [System.Serializable]
    public class Path
    {
        public List<Knob> PathList = new List<Knob>();
        public List<Knob> Visited = new List<Knob>();
        public List<Knob> PrevPath = new List<Knob>();

        private List<WireController> _pathWires = new List<WireController>();
        private List<CircuitApparatus> _pathApparatus = new List<CircuitApparatus>();
        private List<Knob> _fullPath = new List<Knob>();

        public List<WireController> PathWires => _pathWires.Count > 0 ? _pathWires : GetPathWires();
        public List<CircuitApparatus> PathApparatus => _pathApparatus.Count > 0 ? _pathApparatus : GetPathAsApparatus();
        public List<Knob> FullPath => _fullPath.Count > 0 ? _fullPath : GetFullPath();

        public Path(List<Knob> pathList)
        {
            PathList = pathList;
        }

        public Path(List<Knob> pathList, List<Knob> visited, List<Knob> prevPath)
        {
            PathList = pathList;
            Visited = visited;
            PrevPath = prevPath;
        }

        public List<Knob> GetFullPath()
        {
            List<Knob> ret = new List<Knob>(PrevPath);
            ret.AddRange(PathList);

            return ret;
        }

        public void FinalizePath()
        {
            _fullPath = GetFullPath();
            _pathWires = GetPathWires();
            _pathApparatus = GetPathAsApparatus();
        }

        public List<WireController> GetPathWires()
        {
            List<WireController> ret = new List<WireController>();
            
            for (int i = 0; i < FullPath.Count - 1; i++)
            {
                WireController wire = ConnectionManager.Instance.GetWireBetweenKnobs(FullPath[i], FullPath[i + 1]);
                if (wire != null)
                    ret.Add(wire);
            }

            return ret;
        }

        public List<CircuitApparatus> GetPathAsApparatus()
        {
            List<CircuitApparatus> ret = new List<CircuitApparatus> ();

            for (int i = 0; i < FullPath.Count; i++)
            {
                CircuitApparatus apparatus = FullPath[i].transform.parent.GetComponent<CircuitApparatus>();
                if (apparatus != null && !ret.Contains(apparatus)) ret.Add(apparatus);
            }

            return ret;
        }

        public List<Resistance> GetPathResistanceApparatus()
        {
            List<Resistance> ret = new List<Resistance>();
            foreach (var app in PathApparatus)
            {
                if (app is Resistance resistance)
                {
                    ret.Add(resistance);
                }
                else if (app is VoltmeterScript voltmeter)
                {
                    if (voltmeter.AttachedWire != null) { ret.Add(voltmeter.AttachedWire); }
                }
            }

            return ret;
        }

        public float GetPathResistance()
        {
            float ret = 0.0f;
            List<Resistance> resistanceApp = GetPathResistanceApparatus();
            foreach (Resistance res in resistanceApp)
            {
                ret += res.ResistanceValue;
            }

            return ret;
        }

        public void AssignWireVoltages(float startVoltage)
        {
            float decrement = startVoltage / (PathWires.Count);
            
            for (int i = 0; i < PathWires.Count; i++)
            {
                float maxV = startVoltage - i*decrement;
                float minV = maxV - decrement;
                PathWires[i].MinMaxVoltage = new Vector2(minV, maxV);
            }
        }

    }
}
