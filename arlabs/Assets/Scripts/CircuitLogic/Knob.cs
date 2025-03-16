using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knob : MonoBehaviour
{
    public Color WireColor = Color.black;

    public List<WireController> ConnectedWires = new List<WireController>();
    public List<Knob> NeighbourKnobs = new List<Knob>();

    public bool IsPositive = false;

    public void EnableWireUpdate()
    {
        foreach (WireController controller in ConnectedWires)
        {
            controller.Placed = false;
        }
    }

    public void DisableWireUpdate()
    {
        foreach(WireController controller in ConnectedWires)
        {
            controller.Placed = true;
            controller.PlaceCollider();
        }
    }
}
