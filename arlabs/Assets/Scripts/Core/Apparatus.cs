using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ARLabs.UI;

namespace ARLabs.Core
{
    /// <summary>
    /// The base class of every single apparatus, functionality common to all. (Place, Reposition, Delete)
    /// Every object in the place apparatus list.
    /// Can define wether apparatus can be repositioned or not
    /// </summary>

    public class Apparatus : MonoBehaviour
    {
        [SerializeField] private string _name;
        [SerializeField] private string _desc;
        [SerializeField] private string _url;
        [SerializeField] private int _apparatusID;
        [SerializeField] private Sprite _thumbnail;
        [SerializeField] private bool _canBeSelected = true;
        [SerializeField] private bool _canBeInteracted = true;
        [SerializeField] private GameObject _indicatorMesh;
        [SerializeField] private GameObject _apparatusMesh;
        [SerializeField] private FieldsList _fields = new();

        [SerializeField] protected bool _isRepositioning = false;
        [SerializeField] protected bool _isPlacing = false;
        [SerializeField] protected bool _isInteracting = false;

        private ARRaycastManager _raycastManager;   // Cached when placing apparatus
        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();
        private Vector3 _previousApparatusPosition;
        private Outline _outline;

        public bool IsPlacing => _isPlacing;
        public bool CanBeSelected => _canBeSelected;
        public bool CanBeInteracted => _canBeInteracted;
        public bool IsInteracting => _isInteracting;

        public string Head => _name;
        public string Description => _desc;
        public string Webpage => _url;
        public int ApparatusID => _apparatusID;
        public Sprite Thumbnail => _thumbnail;
        public FieldsList Fields => _fields;

        private void Start()
        {
            OnStart();
        }

        protected virtual void OnStart()
        {
            if (GetComponent<Outline>() == null)
                _outline = gameObject.AddComponent(typeof(Outline)) as Outline;
            else
                _outline = GetComponent<Outline>();

            _outline.OutlineColor = Color.white;
            _outline.OutlineWidth = 7;
            _outline.OutlineMode = Outline.Mode.OutlineAll;
            _outline.enabled = false;
        }

        private void Update()
        {
            OnUpdate();
        }

        protected virtual void OnUpdate()
        {
            if (_isPlacing)
            {
                PlaceBehaviour();
            }

            if (_isRepositioning)
            {
                RepositionBehaviour();
            }

            if (_isInteracting)
            {
                InteractionBehaviour();
            }
        }

        // What do when we are placing
        protected virtual void PlaceBehaviour()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Vector2 rayOrigin = Input.mousePosition;
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = ApparatusManager.Instance.desktopPlaceDistance;
            Vector3 worldPosition = ApparatusManager.Instance.mainCamera.ScreenToWorldPoint(mousePosition);

            if(!Input.GetKey(KeyCode.LeftControl))
                transform.position = worldPosition;
#else
            Vector2 rayOrigin = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (_raycastManager.Raycast(rayOrigin, _hits, TrackableType.Planes))
            {
                Pose hitPose = _hits[0].pose;
                transform.position = hitPose.position;
            }
#endif
        }

        // What do when we are repositioning
        protected virtual void RepositionBehaviour()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Vector2 rayOrigin = Input.mousePosition;
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = ApparatusManager.Instance.desktopPlaceDistance;
            Vector3 worldPosition = ApparatusManager.Instance.mainCamera.ScreenToWorldPoint(mousePosition);

            if(!Input.GetKey(KeyCode.LeftControl))
                transform.position = worldPosition;
#else
            Vector2 rayOrigin = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (_raycastManager.Raycast(rayOrigin, _hits, TrackableType.Planes))
            {
                Pose hitPose = _hits[0].pose;
                transform.position = hitPose.position;
            }
#endif
        }

        Vector3 _preInteractionPosition;
        // Long press interaction
        protected virtual void InteractionBehaviour()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Vector2 rayOrigin = Input.mousePosition;
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = ApparatusManager.Instance.desktopPlaceDistance;
            Vector3 worldPosition = ApparatusManager.Instance.mainCamera.ScreenToWorldPoint(mousePosition);

            if(!Input.GetKey(KeyCode.LeftControl))
                transform.position = worldPosition;
#else
            if (_raycastManager.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), _hits, TrackableType.Planes))
            {
                Pose hitPose = _hits[0].pose;
                transform.position = hitPose.position;
            }
#endif
        }

        // Start long press interaction
        public virtual void StartInteraction()
        {
            _preInteractionPosition = transform.position;
            _isInteracting = true;
        }

        // End long press interaction
        public virtual void EndInteraction()
        {
            transform.position = _preInteractionPosition;
            _isInteracting = false;
        }

        // Places the apparatus on the ar plane
        public virtual void StartPlacing(ARRaycastManager raycastManager)
        {
            _raycastManager = raycastManager;

            _isPlacing = true;
            _indicatorMesh.SetActive(true);
            _apparatusMesh.SetActive(false);

            ExperimentManager.Instance.GoToPlacingState();
            UIReferences.Instance.placingWindowName.text = Head;
        }



        // Finalizes the placement
        public virtual void FinalizePlace()
        {
            _isPlacing = false;
            _indicatorMesh.SetActive(false);
            _apparatusMesh.SetActive(true);
            ExperimentManager.Instance.GoToIdleState();
            UIReferences.Instance.placingWindowName.text = "";
        }

        // What to do upon start repositioning
        public virtual void RepositionStart()
        {
            _isRepositioning = true;
            _previousApparatusPosition = transform.position;
            ExperimentManager.Instance.GoToRepositionState();
            UIReferences.Instance.repositionWindowName.text = Head;
        }
        public virtual void RepositionFinalize()
        {
            _isRepositioning = false;
            ExperimentManager.Instance.GoToIdleState();
            UIReferences.Instance.repositionWindowName.text = "";
        }
        public virtual void RepositionCancel()
        {
            _isRepositioning = false;
            transform.position = _previousApparatusPosition;
            ExperimentManager.Instance.GoToIdleState();
            UIReferences.Instance.repositionWindowName.text = "";
        }

        // What to do on selection
        public virtual void Select()
        {
            _outline.enabled = true;
            UIReferences.Instance.highlightWindow.TurnOn();
            UIReferences.Instance.higlightWindowName.text = Head;


            /*  foreach(FieldInfo field in Fields)
              {
                  switch(field.FieldType)
                  {
                      case FieldType.Bool: 
                          Instantiate(UIReferences.Instance.BoolFieldPrefab, UIReferences.Instance.apparatusFieldListParent).GetComponent<BoolField>().info = field;
                          break;
                      case FieldType.Slider: 
                          Instantiate(UIReferences.Instance.SliderFieldPrefab, UIReferences.Instance.apparatusFieldListParent).GetComponent<SliderField>().info = field;
                          break;
                      case FieldType.Text: 
                          Instantiate(UIReferences.Instance.TextFieldPrefab, UIReferences.Instance.apparatusFieldListParent).GetComponent<TextField>().info = field;
                          break;
                      case FieldType.Dropdown: 
                          Instantiate(UIReferences.Instance.DropdownFieldPrefab, UIReferences.Instance.apparatusFieldListParent).GetComponent<DropdownField>().info = field;
                          break;
                  }
              }*/

            Transform fieldsParent = UIReferences.Instance.apparatusFieldListParent;
            foreach (var fi in Fields.ButtonFields)
            {
                Instantiate(
                    UIReferences.Instance.ButtonFieldPrefab,
                    fieldsParent
                    ).GetComponent<ButtonField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.SliderFields)
            {
                Instantiate(
                    UIReferences.Instance.SliderFieldPrefab,
                    fieldsParent
                    ).GetComponent<SliderField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.BoolFields)
            {
                Instantiate(
                    UIReferences.Instance.BoolFieldPrefab,
                    fieldsParent
                    ).GetComponent<BoolField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.DropdownFields)
            {
                Instantiate(
                    UIReferences.Instance.DropdownFieldPrefab,
                    fieldsParent
                    ).GetComponent<DropdownField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.TextFields)
            {
                Instantiate(
                    UIReferences.Instance.TextFieldPrefab,
                    fieldsParent
                    ).GetComponent<TextField>().Initialize(fi.Value);
            }
        }

        // What to do on deselection
        public virtual void Deselect()
        {
            _outline.enabled = false;
            UIReferences.Instance.highlightWindow.TurnOff();
            UIReferences.Instance.higlightWindowName.text = "";
            /*
            if (_apparatusMesh != null)
                _apparatusMesh.GetComponent<MeshRenderer>().material.color = Color.white;
                // Failsafe as touches are detected past ui elements
            */

            _isRepositioning = false;

            foreach (Transform field in UIReferences.Instance.apparatusFieldListParent)
            {
                Destroy(field.gameObject);
            }
        }

        // What to do when deleting
        public virtual void Delete()
        {
            Destroy(gameObject);
        }
    }

}

