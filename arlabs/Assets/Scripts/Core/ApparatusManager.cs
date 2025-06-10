using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARLabs.UI;
using ARLabs;

namespace ARLabs.Core
{
    public class ApparatusManager : MonoBehaviour
    {
        #region Singleton
        public static ApparatusManager Instance;
        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(this); }
        }
        #endregion

        [SerializeField] private ApparatusHighlight ApparatusHighlightUI;
        [SerializeField] private LayerMask _whatIsApparatus = ~0;
        [SerializeField] private float apparatusRotateAmt; // Amt to rotate the apparatus by
        private ARRaycastManager _raycastManager;
        [SerializeField] private Apparatus _selectedApparatus;   // The current selected/grabbing apparatus
        [SerializeField] private Apparatus _placingApparatus;    // The apparatus we are creating

        public float desktopPlaceDistance = 10f; // The distance (from camera) at which the apparatus is placed while on desktop env

        [Header("Grab Settings")]
        [SerializeField] private InteractLoad _interactLoadUI;
        [SerializeField] private float tapTimeThreshold = 0.5f;
        [SerializeField] private float interactTimeThreshold = 1f;
        [ShowOnly] public bool isPressed;
        private float pressStartTime;
        private Apparatus pressedApparatus;
        private HashSet<ushort> usedIds = new HashSet<ushort>();


        [HideInInspector] public Camera mainCamera;


        private void Start()
        {
            _selectedApparatus = null;
            _placingApparatus = null;
            _raycastManager = FindObjectOfType<ARRaycastManager>();
            mainCamera = Camera.main;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            mainCamera.transform.parent.position += new Vector3(0f, 2f, 0f);
            mainCamera.transform.parent.eulerAngles = new Vector3(30f, 0f, 0f);
#endif
        }

        private void Update()
        {
            // On Tap Down
            if (Input.GetMouseButtonDown(0))
            {

#if UNITY_EDITOR
                // Check if we're clicking on UI first
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;  // Exit early if clicking on UI
#elif UNITY_ANDROID || UNITY_IOS
                if (TouchOverUI.IsPointerOverUIObject())
                    return;  // Exit early if clicking on UI
#endif

                // Only interact with other apparatus/create new if we are not currently placing one
                if (_placingApparatus == null && !ExperimentManager.Instance.IsRepositioning)
                {
                    if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, _whatIsApparatus))
                    {
                        pressStartTime = Time.time;
                        isPressed = true;
                        pressedApparatus = FindEarliestApparatusAncestor(hit.transform);
                    }
                    else
                    {
                        // Deselect selected apparatus if clicked in void (but not on UI)
                        DeselectApparatus();
                    }
                }
            }

            // On Tap Hold
            if (isPressed && Input.GetMouseButton(0))
            {
                float pressDuration = Time.time - pressStartTime;

                if (pressDuration < tapTimeThreshold && pressedApparatus != null
                    && pressedApparatus != null && pressedApparatus.CanBeSelected)
                {
                    _interactLoadUI.SetActive(false);
                }
                else if (pressDuration > tapTimeThreshold && pressDuration < interactTimeThreshold
                    && pressedApparatus != null && pressedApparatus.CanBeSelected)
                {
                    _interactLoadUI.SetActive(true);
                    _interactLoadUI.SetValue(pressDuration / interactTimeThreshold);
                    _interactLoadUI.SetPosition(mainCamera.WorldToScreenPoint(pressedApparatus.transform.position)
                        + new Vector3(0f, 100f, 0f)); // offset so that the finger doesn't hide the load icon
                }
                else if (pressDuration >= interactTimeThreshold
                    && pressedApparatus != null && pressedApparatus.CanBeSelected)
                {
                    _interactLoadUI.SetActive(false);
                    if (_selectedApparatus != null)
                    {
                        _selectedApparatus.Deselect();
                    }
                    _selectedApparatus = pressedApparatus;
                    _selectedApparatus.StartGrab();
                    isPressed = false;
                }
            }


            // On Tap Up
            if (Input.GetMouseButtonUp(0) && pressedApparatus != null)
            {
                float pressDuration = Time.time - pressStartTime;
                if (pressDuration < interactTimeThreshold)
                {
                    if (_selectedApparatus != null)
                    {
                        _selectedApparatus.Deselect();
                    }
                    if (pressedApparatus.CanBeSelected)
                    {
                        _selectedApparatus = pressedApparatus;
                        _selectedApparatus.Select();
                    }
                }
                else if (_selectedApparatus != null)
                {
                    _selectedApparatus.EndGrab();
                    _selectedApparatus = null;
                }

                _interactLoadUI.SetActive(false);
                isPressed = false;
                pressedApparatus = null;
            }
        }

        // Instantiates and starts placing a given apparatus game object
        public void CreateApparatus(GameObject apparatusGO)
        {
            if (_selectedApparatus != null) return; // Don't create if we have a selected apparatus
            if (_placingApparatus != null) return;  // Don't create if we are placing one currently

            //UIManager.Instance.PlacementMenu.SetActive(true);

            _placingApparatus = Instantiate(apparatusGO).GetComponent<Apparatus>();
            _placingApparatus.StartPlacing(_raycastManager);

            // Usually handled by UnityEvents but tying up
            // instantiated GOs with events was tough
            UIReferences.Instance.apparatusMenuWindow.TurnOff();
            UIReferences.Instance.placingWindow.TurnOn();

        }

        // Called from UI Button
        public void RepositionSelectedApparatus()
        {
            if (_selectedApparatus == null) { return; }
            _selectedApparatus.RepositionStart();
        }

        // Called from UI Button
        public void DeleteSelectedApparatus()
        {
            Debug.Log("Deleting Selected");
            if (_selectedApparatus == null) { return; }
            ExperimentManager.Instance.InstantiatedApparatus.Remove(_selectedApparatus);
            _selectedApparatus.Delete();
            _selectedApparatus = null;

            // UX Specific Behaviour
            //UIManager.Instance.SelectionMenu.SetActive(false);    // TODO: Maybe move out of script and put as unity event in button click event
        }

        // Called from UI Button
        public void CancelPlacingApparatus()
        {
            if (_placingApparatus == null) { return; }
            _placingApparatus.Delete();
            _placingApparatus = null;

            ExperimentManager.Instance.GoToIdleState();

            // UX Specific Behaviour
            //UIManager.Instance.PlacementMenu.SetActive(false);    // TODO: Maybe move out of script and put as unity event in button click event
        }

        // Called from UI Button
        public void FinalizePlacement()
        {
            // UX Specific Behaviour
            //UIManager.Instance.PlacementMenu.SetActive(false);

            ExperimentManager.Instance.InstantiatedApparatus.Add(_placingApparatus);
            _placingApparatus.FinalizePlace();
            _placingApparatus = null;
        }

        // Called during placement and repositioning
        public void RotateRight(float factor)
        {
            if (_placingApparatus != null)
            {
                // Debug.Log("Rotating Apparatus !");
                _placingApparatus.transform.Rotate(0f, factor * apparatusRotateAmt, 0f);
            }
            else if (_selectedApparatus != null)
            {
                _selectedApparatus.transform.Rotate(0f, factor * apparatusRotateAmt, 0f);
            }
        }

        // Called from 
        public void DeselectApparatus()
        {
            _selectedApparatus?.Deselect();
            _selectedApparatus = null;
        }
        public void RepositionStart()
        {
            _selectedApparatus.RepositionStart();
        }
        public void RepositionFinalize()
        {
            FinalizeRepositionAndSelect();
        }
        public void RepositionCancel()
        {
            _selectedApparatus.RepositionCancel();
        }

        public void DetachAndRepositionApparatus(Apparatus apparatus)
        {
            if (_selectedApparatus != null)
            {
                _selectedApparatus.Deselect();
            }
            _selectedApparatus = apparatus;
            apparatus.DetachFromParent();

            // Start repositioning
            RepositionStart();
            UIReferences.Instance.repositionWindow.TurnOn();
        }

        // Add this method to handle post-repositioning selection
        private void FinalizeRepositionAndSelect()
        {
            if (_selectedApparatus != null)
            {
                _selectedApparatus.RepositionFinalize();
                _selectedApparatus.Select(); // Re-select to refresh the UI
            }
        }

        // util
        private Apparatus FindEarliestApparatusAncestor(Transform hit)
        {
            if (hit == null) return null;

            if (hit.TryGetComponent<Apparatus>(out var apparatus))
            {
                return apparatus; // Return the first Apparatus we find
            }

            // If no Apparatus on this object, check parent
            return FindEarliestApparatusAncestor(hit.parent);
        }


        // Util: Generate an apparatusID
        public ushort GenerateUniqueUShortId()
        {
            for (ushort id = 0; id < ushort.MaxValue; id++)
            {
                if (!usedIds.Contains(id))
                {
                    usedIds.Add(id);
                    return id;
                }
            }

            throw new System.Exception("No available ID left in ushort range.");
        }
        // Release used IDs
        public void ReleaseId(ushort id) { usedIds.Remove(id); }
    }
}