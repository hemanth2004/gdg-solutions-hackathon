using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Gui;
using UnityEngine.XR.ARFoundation;
using System;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class UserJourneyManager : MonoBehaviour
{



    #region States

    [System.Serializable]
    public enum AppStates
    {
        Instructions,
        Idle,
        DetectingPlanes,
        PlacingApparatus,
        RepositionApparatus,
        VisualizationOld,
        Highlighting
    }

    private AppStates _current;
    public AppStates currentState
    {
        get
        {
            return _current;
        }
        set
        {
            Debug.Log("State Changed to: " + value.ToString());
            _current = value;

            if (value == AppStates.Idle)
            {
                planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                PlacementIndicator.SetActive(false);
                uiReferences.highlightWindow.On = false;
                SetPlanesVisibility(false);
            }
            else if (value == AppStates.Instructions)
            {
                planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                PlacementIndicator.SetActive(false);
                uiReferences.highlightWindow.On = false;
                SetPlanesVisibility(false);
            }
            else if (value == AppStates.DetectingPlanes)
            {
                planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
                PlacementIndicator.SetActive(false);
                uiReferences.highlightWindow.On = false;
                SetPlanesVisibility(true);
            }
            else if (value == AppStates.PlacingApparatus)
            {
                planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                PlacementIndicator.SetActive(true);
                PlacementIndicator.GetComponent<PlacementIndicator>().PlacementMesh = apparatusPrefabs[currentSelectedApparatusName];
                uiReferences.highlightWindow.On = false;
                SetPlanesVisibility(true);
            }
            else if (value == AppStates.RepositionApparatus)
            {
                planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                uiReferences.highlightWindow.On = false;
                SetPlanesVisibility(true);
            }
            else if (value == AppStates.VisualizationOld)
            {
                planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                PlacementIndicator.SetActive(false);
                uiReferences.highlightWindow.On = false;
                SetPlanesVisibility(false);
            }
            else if (value == AppStates.Highlighting)
            {
                planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                PlacementIndicator.SetActive(false);
                uiReferences.highlightWindow.On = true;
                SetPlanesVisibility(false);


                Debug.Log(currentHighlightedApparatus.gameObject.name);
                uiReferences.highlightWindowNameHead.text = currentHighlightedApparatus.gameObject.name;
            }

        }
    }
    #endregion


    public ApparatusOld currentHighlightedApparatus = null; //equal to null -> none selected
    public ARPlaneManager planeManager;
    public RepositionWindow repositionWindow;

    //follow a lowercase-only naming scheme
    //(Using a custom dictionary system since Unity doesnt support setting inspector values for C3 dictionaries
    [UDictionary.Split(70, 30)]
    public UDictionary1 apparatusPrefabs;
    [Serializable]
    public class UDictionary1 : UDictionary<string, GameObject> { }

    public string currentSelectedApparatusName; //string thats used to select the right prefab from the dictionary
    public string currentPhysicalInteraction;

    [SerializeField] private GameObject splash;
    [SerializeField] private GameObject PlacementIndicator;
    [HideInInspector] public UIReferences uiReferences;

    public ConnectionManager connectionManager;
    private SwipeManager swipeManager;
    private InteractionComponentManager interactionManager;
    private bool visualization;
    private bool _currentlyPhysicalInteracting = false;
    private RaycastHit interactionHit;


    private void Start()
    {
        swipeManager = GetComponent<SwipeManager>();
        interactionManager = GetComponent<InteractionComponentManager>();
        connectionManager = GetComponent<ConnectionManager>();
        uiReferences = gameObject.GetComponent<UIReferences>(); //object holding all our ui references
        Debug.Log(uiReferences.gameObject.name);

        //Destroy(splash, 2f); //A fake loading screen, will replace with an actual background loading mechanism in the future.

        currentState = AppStates.Instructions;
    }

    private void Update()
    {
        switch (currentState)
        {
            case AppStates.Instructions:
                break;

            case AppStates.Idle:
                if (Input.GetMouseButtonDown(0))
                {
                    //checking for ui
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        break;
                    }

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Debug.Log("Hit object: " + hit.collider.gameObject.name);

                        if (hit.transform.parent.GetComponent<ApparatusOld>() != null)
                        {
                            if (currentHighlightedApparatus == null) //directly highlight
                            {
                                currentHighlightedApparatus = hit.transform.parent.GetComponent<ApparatusOld>();

                                currentHighlightedApparatus.isHighlighted = true;

                                currentState = AppStates.Highlighting;

                            }
                            else //unhighlight first one and then highlight the newly selected one
                            {
                                currentHighlightedApparatus.isHighlighted = false;
                                currentHighlightedApparatus = hit.transform.parent.GetComponent<ApparatusOld>();

                                currentHighlightedApparatus.isHighlighted = true;
                                currentState = AppStates.Highlighting;

                            }
                        }
                        else //Incase I do hit something but it turns out to be something other than an apparatus
                        {
                            currentHighlightedApparatus.isHighlighted = false;
                            currentHighlightedApparatus = null;
                            currentState = AppStates.Idle;
                        }
                    }
                    else //hitting the void
                    {
                        currentHighlightedApparatus.isHighlighted = false;
                        currentHighlightedApparatus = null;
                        currentState = AppStates.Idle;
                    }
                }
                break;

            case AppStates.DetectingPlanes:
                break;

            case AppStates.Highlighting:


                if (EventSystem.current.IsPointerOverGameObject())
                    break;
                if (Input.GetMouseButtonDown(0) && !_currentlyPhysicalInteracting)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out interactionHit))
                    {


                        if (interactionHit.collider.gameObject.name.Substring(0, interactionManager.INTERACTION_PREFIX.Length) == interactionManager.INTERACTION_PREFIX)
                        {

                            string interactionComponent = interactionHit.collider.gameObject.name.Substring(interactionManager.INTERACTION_PREFIX.Length + 1);
                            GameObject targetInteractionGameObject = interactionHit.collider.gameObject;
                            Debug.Log(interactionComponent);
                            bool onStartInteraction = interactionManager.TryCallFunction(targetInteractionGameObject, interactionComponent, "InteractionStart");
                            bool setTouchStartPos = interactionManager.TrySetVariable(targetInteractionGameObject, interactionComponent, "touchStart", Input.mousePosition);
                            bool setTouchEndPos = interactionManager.TrySetVariable(targetInteractionGameObject, interactionComponent, "touchhEnd", Input.mousePosition);
                            _currentlyPhysicalInteracting = true;
                        }
                    }
                }
                else if (Input.GetMouseButtonUp(0) && _currentlyPhysicalInteracting)
                {
                    Debug.Log("Up");
                    if (interactionHit.collider.gameObject.name.Substring(0, interactionManager.INTERACTION_PREFIX.Length) == interactionManager.INTERACTION_PREFIX)
                    {
                        string interactionComponent = interactionHit.collider.gameObject.name.Substring(interactionManager.INTERACTION_PREFIX.Length + 1);
                        GameObject targetInteractionGameObject = interactionHit.collider.gameObject;
                        bool setTouchEndPos = interactionManager.TrySetVariable(targetInteractionGameObject, interactionComponent, "touchEnd", Input.mousePosition);
                        bool onStartInteraction = interactionManager.TryCallFunction(targetInteractionGameObject, interactionComponent, "InteractionEnd");
                        _currentlyPhysicalInteracting = false;
                    }
                }

                break;
        }
    }
    public void StartLabs()
    {
        // Will be called after reading instructions
        currentState = AppStates.DetectingPlanes;
    }

    public void SetPlanesVisibility(bool visible)
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.GetComponent<ARPlaneMeshVisualizer>().enabled = visible;
            plane.GetComponent<MeshRenderer>().enabled = visible;
        }
    }

    #region PlaneMethods
    public void ClearDetectedPlanes()
    {
    }
    public void DonePlaneDetection()
    {
        currentState = AppStates.Idle;
    }

    public void SetDetectingState()
    {
        currentState = AppStates.DetectingPlanes;
    }
    #endregion

    #region PlacementMethods
    public void StartPlacingApparatus(string prefabKey)
    {
        currentSelectedApparatusName = prefabKey;
        currentState = AppStates.PlacingApparatus;
    }
    public void CancelPlace()
    {
        currentState = AppStates.Idle;
        currentSelectedApparatusName = "";
    }
    public void PlaceApparatus()
    {
        Instantiate(apparatusPrefabs[currentSelectedApparatusName], PlacementIndicator.transform.GetChild(0).position, PlacementIndicator.transform.GetChild(0).rotation);
        currentSelectedApparatusName = "";
        currentState = AppStates.Idle;
    }


    public void RepositionStart()
    {
        repositionWindow.StartReposition(currentHighlightedApparatus.transform);
        currentState = AppStates.RepositionApparatus;

        Knob[] knobs = currentHighlightedApparatus.GetComponentsInChildren<Knob>();
        foreach (Knob knob in knobs)
        {
            knob.EnableWireUpdate();
        }
    }
    public void RepositionPlace()
    {
        repositionWindow.EndReposition();
        currentState = AppStates.Highlighting;

        Knob[] knobs = currentHighlightedApparatus.GetComponentsInChildren<Knob>();
        foreach (Knob knob in knobs)
        {
            knob.DisableWireUpdate();
        }
    }
    public void RepositionCancel()
    {
        repositionWindow.CancelReposition();
        currentState = AppStates.Highlighting;

        Knob[] knobs = currentHighlightedApparatus.GetComponentsInChildren<Knob>();
        foreach (Knob knob in knobs)
        {
            knob.DisableWireUpdate();
        }
    }

    #endregion

    #region HighlightMethods
    public void CloseHighlightWindow()
    {
        currentHighlightedApparatus.isHighlighted = false;
        currentHighlightedApparatus = null;
        currentState = AppStates.Idle;
    }
    #endregion

    #region VisualizationOld
    public void OnVisualizationWindow(bool open)
    {
        uiReferences.visualizationWindow.On = open;
        currentState = open ? AppStates.VisualizationOld : AppStates.Idle;

        Debug.Log(currentState.ToString());
    }

    public void OnTurnElectronVisual()
    {
        if (uiReferences.electronToggle.On)
            GetComponent<CircuitManager>().StartWireElectronVisualisations();
        else
            GetComponent<CircuitManager>().StopWireElectronVisualisations();
    }

    public void OnTurnPotentialVisual()
    {
        if (uiReferences.potentialToggle.On)
            GetComponent<CircuitManager>().StartWirePotentialVisualisations();
        else
            GetComponent<CircuitManager>().StopWirePotentialVisualisations();
    }
    #endregion

}
