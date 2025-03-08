using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARLabs.UI;

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
        [SerializeField] private Apparatus _selectedApparatus;   // The current selected apparatus
        [SerializeField] private Apparatus _placingApparatus;    // The apparatus we are creating
        
        public float desktopPlaceDistance = 10f; // The distance (from camera) at which the apparatus is placed while on desktop env

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
            if (Input.GetMouseButtonDown(0))
            {
                // Only interact with other apparatus/create new if we are not currently placing one
                if (_placingApparatus == null && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("CLICKING!!! 🗣️🗣️🗣️🗣️");
                    // Raycast to mouse position
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, _whatIsApparatus))
                    {
                        Debug.Log("CLICKING APPAARTUSS!!! 🗣️🗣️🗣️🗣️");
                        // If we have an apparatus selected, deselect it
                        if (_selectedApparatus != null)
                        {
                            _selectedApparatus.Deselect();
                        }

                        _selectedApparatus = hit.transform.root.GetComponent<Apparatus>(); 
                        //^^^^ reminder^^^ ‼️‼️🗣️🗣️⬆️⬆️


                        if (!_selectedApparatus.CanBeSelected) { _selectedApparatus = null; }

                        // If we click on an Apparatus, make it as selected
                        if (_selectedApparatus != null)
                        {
                            _selectedApparatus.Select();
                        }
                        else
                        {
                            //Debug.Log("RAYCAST TRUE BUT NOT ON APPARATUS");
                        }
                    }
                    else
                    {
                        // Deselect selected apparatus if clicked in void

                        //Debug.Log("RAYCAST FALSE");
                        DeselectApparatus();
                        //UIManager.Instance.SelectionMenu.SetActive(false);
                    }
                }
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
            if(_placingApparatus != null)
            {
                // Debug.Log("Rotating Apparatus !");
                _placingApparatus.transform.Rotate(0f, factor * apparatusRotateAmt, 0f);
            }
            else if(_selectedApparatus != null)
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
            _selectedApparatus.RepositionFinalize();
        }
        public void RepositionCancel()
        {
            _selectedApparatus.RepositionCancel();
        }
    }
}