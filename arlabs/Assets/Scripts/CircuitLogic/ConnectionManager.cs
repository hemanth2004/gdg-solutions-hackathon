using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    [SerializeField] private LayerMask _knobsLayer;
    [SerializeField] private LayerMask _wiresLayer;
    [SerializeField] private GameObject _wirePrefab;

    private Knob _startKnob, _endKnob;
    private WireController _currentWire;
    public List<WireController> _allConnectedWires = new List<WireController>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        _allConnectedWires = new List<WireController>();
    }

    private void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch currentTouch = Input.GetTouch(0);

        // If finger is over UI
        if (EventSystem.current.IsPointerOverGameObject(currentTouch.fingerId))
        {
            return; // Don't perform raycasting if over UI
        }

        // If we have a wire then keep updating its position to finger position
        if (_currentWire != null)
        {
            UpdateCurrentWirePosition(currentTouch.position);
        }

        // If finger is currently over a knob...
        if (Physics.Raycast(Camera.main.ScreenPointToRay(currentTouch.position), out RaycastHit hit, 100f, _knobsLayer))
        {
            // On pressing the finger, find the starting knob and create wire
            if (currentTouch.phase == TouchPhase.Began)
            {
                _startKnob = hit.transform.GetComponent<Knob>();
                // Create a wire only if there isn't a current floating wire
                if (_currentWire == null)
                {
                    _currentWire = CreateWireFromKnob(_startKnob);
                }
            }

            // If we have a starting knob, look for ending knob on finger release
            if (_startKnob != null)
            {
                // If finger is released over a knob...
                if (currentTouch.phase == TouchPhase.Canceled || currentTouch.phase == TouchPhase.Ended)
                {
                    // ...set it as the ending knob
                    _endKnob = hit.transform.GetComponent<Knob>();
                    _currentWire.EndKnob = _endKnob;

                    // and finalize the connection
                    FinalizeConnection();

                    CircuitManager.Instance.UpdatePaths();
                }
            }
        }
        // If we didn't find a knob and we have a floating wire, then destroy it on releasing finger
        else if (_currentWire != null)
        {
            if (currentTouch.phase == TouchPhase.Canceled || currentTouch.phase == TouchPhase.Ended)
            {
                DestroyCurrentConnection();
            }
        }

        // ---------- SAMPLE TO TEST WIRE DETECTION ----------------
        // If we're not in the middle of attaching wires
        if (_currentWire == null)
        {
            // TODO: MAKE UI AND STUFF (INTEGRATE WITH APP LIFECYCLE)
            // Then remove the wire we touch on
            RemoveWireOnTouch(currentTouch);
        }
    }

    private void RemoveWireOnTouch(Touch currentTouch)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(currentTouch.position), out RaycastHit wireHit, 100f, _wiresLayer))
        {
            if (currentTouch.phase == TouchPhase.Began)
            {
                WireController wire = wireHit.transform.parent.GetComponent<WireController>();
                wire.StartKnob.NeighbourKnobs.Remove(wire.EndKnob);
                wire.EndKnob.NeighbourKnobs.Remove(wire.StartKnob);
                _allConnectedWires.Remove(wire);
                Destroy(wire.gameObject);

                CircuitManager.Instance.UpdatePaths();
            }
        }
    }

    private void FinalizeConnection()
    {
        // If the end knob is same as the start or it is a pre existing connection...
        if (_endKnob == _startKnob || ConnectionInList(_currentWire))
        {
            //...then connection is INVALID
            DestroyCurrentConnection();
        }
        // Otherwise, add it to the connection list
        else
        {
            _currentWire.UpdateWire();
            _currentWire.PlaceCollider();
            _currentWire.Placed = true;

            _allConnectedWires.Add(_currentWire);

            _startKnob.ConnectedWires.Add(_currentWire); _startKnob.NeighbourKnobs.Add(_endKnob);
            _endKnob.ConnectedWires.Add(_currentWire); _endKnob.NeighbourKnobs.Add(_startKnob);
        }
    }

    // Creates a wire using a knob as its starting knob
    private WireController CreateWireFromKnob(Knob knob)
    {
        WireController wire = Instantiate(_wirePrefab, knob.transform.position, Quaternion.identity).GetComponent<WireController>();
        wire.SetWireColor(knob.WireColor);
        wire.StartKnob = knob;

        return wire;
    }

    // Moves the current wire with finger
    private void UpdateCurrentWirePosition(Vector2 touchPosition)
    {
        // If we have placed the wire, then remove references and return
        if (_currentWire.Placed)
        {
            _startKnob = _endKnob = null;
            _currentWire = null;
            return;
        }
        
        // Otherwise, make it follow the finger
        _currentWire.FingerPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Camera.main.nearClipPlane * 5f));
    }

    // Destroys wire and resets values
    private void DestroyCurrentConnection()
    {
        if (_currentWire == null) { return; }

        Destroy(_currentWire.gameObject);
        _currentWire = null;
        _startKnob = _endKnob = null;
    }

    // Checks wether there pre-exists a connecting wire similiar to passed wire
    private bool ConnectionInList(WireController wire)
    {
        foreach (var w in _allConnectedWires)
        {
            if (IsSameConnection(w, wire))
                return true;
        }

        return false;
    }

    // Checks wether two wires connect same knobs
    private bool IsSameConnection(WireController A, WireController B)
    {
        return (A.EndKnob == B.EndKnob || A.EndKnob == B.StartKnob) && (A.StartKnob == B.StartKnob || A.StartKnob == B.EndKnob);
    }

    public WireController GetWireBetweenKnobs(Knob A, Knob B)
    {
        foreach (var w in _allConnectedWires)
        {
            if ((w.StartKnob == A || w.EndKnob == A) && (w.StartKnob == B || w.EndKnob == B))
            { return w; }
        }

        return null;
    }
}

// 1. Connecting circuit logic with new apparatus
// 2. Writing scripts that call the backend
//      - Sending data to assisstant:
//          - Create experiment data json
//          - Transcribe audio / Use predefined query
//          - Take the screenshot
//          - Get the {user_name + experiment_name} as the thread_id
//      - Receiving data from assistant:
//          - We would get a sequence of b64 audio, "vis_name state"
//          - Play audio, then toggle vis, play audio, toggle vis
//
//      - Sending data to Apparatus identifier
//          - Send frame, apparatus list
//      - Receiving data from Apparatus identifier
//          - Get the apparatus name
//          - Call the placing action TODO: call this action from llm
//
// 3. Button for what is experiment about
// 4. Button for voice command
// 5. Implement action for placing the apparatus given the apparatus name as string
