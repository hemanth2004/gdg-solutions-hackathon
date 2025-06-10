using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Events;
using AYellowpaper.SerializedCollections;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ARLabs.UI;
using System.Linq;

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
        [SerializeField] private ushort _apparatusID;
        [SerializeField] private Sprite _thumbnail;
        [SerializeField] private bool _canBeSelected = true;
        [SerializeField] private bool _canBeInteracted = true;
        [SerializeField] private GameObject _indicatorMesh;
        [SerializeField] private GameObject _apparatusMesh;
        [SerializeField] private FieldsList _fields = new();
        [SerializeField] private ApparatusPhysicsInfo _physicsInfo = new();

        [SerializeField] protected bool _isRepositioning = false;
        [SerializeField] protected bool _isPlacing = false;
        [SerializeField] protected bool _isGrabbing = false;

        private ARRaycastManager _raycastManager;   // Cached when placing apparatus
        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();
        private Vector3 _previousApparatusPosition;
        private Outline _outline;

        public ApparatusPhysicsInfo PhysicsInfo => _physicsInfo;
        public bool IsPlacing => _isPlacing;
        public bool CanBeSelected => _canBeSelected;
        public bool CanBeInteracted => _canBeInteracted;
        public bool IsGrabbing => _isGrabbing;
        public bool SwayWhileGrabbed = true;

        public string Head => _name;
        public string Description => _desc;
        public string Webpage => _url;
        public ushort ApparatusID => _apparatusID;
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



            // _fields.ButtonFields.Add("Delete", new ButtonFieldInfo()
            // {
            //     Label = "Delete Apparatus",
            //     buttonHead = "Delete",
            //     OnChange = (object a) => ApparatusManager.Instance.DeleteSelectedApparatus()
            // });

            _fields.ButtonFields.Add("Detach", new ButtonFieldInfo()
            {
                hideField = true,
                Label = "Detach",
                buttonHead = "Detach",
                OnChange = (object a) => DetachFromParent()
            });
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

            if (_isGrabbing)
            {
                GrabBehaviour();
            }


            _physicsInfo.UpdatePhysicsInfo(transform.position, Time.deltaTime);
        }


        #region INTERACTING

        // A definition of operations that this apparatus can perform to other apparatuses
        [SerializeField] protected List<ApparatusOperation> _interactEvents = new();

        public virtual void InteractWhileGrabbed(Apparatus apparatus)
        {

        }

        public virtual void InteractAfterGrabRelease(Apparatus apparatus)
        {
            Debug.Log($"Number of interact events: {_interactEvents.Count}");

            List<ApparatusOperation> operationsForThisApparatus = new();

            foreach (var operation in _interactEvents)
            {
                if (operation.targetApparatusType.ToString() == apparatus.GetType().ToString())
                {
                    operation.actualTargetApparatus = apparatus;
                    operationsForThisApparatus.Add(operation);
                }
            }

            if (operationsForThisApparatus.Count == 0)
            {
                // No valid operations found, return to pre-grab position
                transform.position = _preGrabPosition;
                return;
            }

            // Check if any operation involves movement or attachment
            bool involvesMovement = operationsForThisApparatus.Any(op => op.name.Contains("Attach") || op.name.Contains("Move"));

            if (!involvesMovement)
            {
                // If no movement-related operation, return to pre-grab position
                transform.position = _preGrabPosition;
            }

            Debug.Log("Starting menu for " + apparatus.Head);
            OperationContext.Instance.StartMenu(operationsForThisApparatus, this);
        }

        #endregion


        #region ATTACHING

        public void AttachApparatusEvent(Apparatus from, Apparatus to)
        {
            bool success = from.AttachTo(to);
            if (!success)
            {
                from.transform.position = _preGrabPosition;
            }
            else
            {
                from.Fields.ButtonFields["Detach"].hideField = false;
            }
        }
        // All attachments follow a tree structure
        // one apparatus has one parent, but multiple children

        [SerializedDictionary("Apparatus", "AttachPoint")]
        public SerializedDictionary<Apparatus, Transform> _possibleAttachments = new();

        [SerializeField]
        private Apparatus _parentAttachment;
        [SerializedDictionary("Apparatus", "AttachPoint")]
        private SerializedDictionary<Apparatus, Transform> _childAttachments = new();
        public SerializedDictionary<Apparatus, Transform> ChildAttachments => _childAttachments;
        //---

        // Attach this to another apparatus (sets the parent)
        public virtual bool AttachTo(Apparatus apparatus)
        {
            var attachPoint = apparatus.AllowAttachmentFrom(this);
            if (attachPoint != null)
            {
                transform.SetParent(attachPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                _parentAttachment = apparatus;
                apparatus._childAttachments.Add(this, attachPoint);
                return true;
            }
            return false;
        }

        // Allow attachment from another apparatus to become a child
        public virtual Transform AllowAttachmentFrom(Apparatus apparatus)
        {
            bool childAlreadyExists = _childAttachments.Keys.Any(x => x.Head == apparatus.Head);
            if (childAlreadyExists)
            {
                return null;
            }

            foreach (var attachment in _possibleAttachments)
            {
                if (attachment.Key.Head == apparatus.Head)
                {
                    Debug.Log("Attachment found: " + apparatus.Head);
                    return attachment.Value;
                }
            }
            return null;
        }

        // Detaches this from its parent
        public virtual void DetachFromParent()
        {
            if (_parentAttachment != null)
            {
                _parentAttachment.DetachAttachment(this);
                _parentAttachment = null;
                transform.SetParent(null);
                ApparatusManager.Instance.DetachAndRepositionApparatus(this);
                _fields.ButtonFields["Detach"].hideField = true;
            }
        }

        // Detaches an attachment from this apparatus
        public virtual void DetachAttachment(Apparatus apparatus)
        {
            var keyToRemove = _childAttachments.Keys.FirstOrDefault(x => x.Head == apparatus.Head);
            if (keyToRemove != null)
            {
                _childAttachments.Remove(keyToRemove);
            }
        }

        #endregion


        #region GRABBING
        Vector3 _preGrabPosition;

        // Long press grab
        protected virtual void GrabBehaviour()
        {
            Vector3 targetPosition;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Vector2 rayOrigin = Input.mousePosition;
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = ApparatusManager.Instance.desktopPlaceDistance;
            targetPosition = ApparatusManager.Instance.mainCamera.ScreenToWorldPoint(mousePosition);
#else
            if (_raycastManager.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), _hits, TrackableType.Planes))
            {
                Pose hitPose = _hits[0].pose;
                targetPosition = hitPose.position;
            }
            else
            {
                return;
            }
#endif

            if (SwayWhileGrabbed)
            {
                // Calculate a smoother velocity
                Vector3 velocity = Vector3.ClampMagnitude(
                    (targetPosition - transform.position),
                    2f  // Max velocity magnitude
                );

                float swayAmount = 0.05f; // Position sway intensity
                float swaySpeed = 3f;     // Position sway speed
                float tiltAmount = 50f;   // Maximum rotation in degrees

                // Create a sway offset based on velocity
                Vector3 swayOffset = new Vector3(
                    Mathf.Sin(Time.time * swaySpeed) * velocity.magnitude * swayAmount,
                    0,
                    Mathf.Cos(Time.time * swaySpeed) * velocity.magnitude * swayAmount
                );

                // Calculate tilt based on movement direction
                Vector3 tiltRotation = new Vector3(
                    -velocity.z * tiltAmount, // Tilt forward/backward
                    0,
                    velocity.x * tiltAmount   // Tilt left/right
                );

                // Smoothly move to target position
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition + swayOffset,
                    Time.deltaTime * 15f
                );

                // Smoothly rotate to tilt
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(tiltRotation),
                    Time.deltaTime * 8f
                );
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.identity;
            }

            Apparatus closestApparatus = ExperimentManager.Instance.GetApparatusInProximity(transform.position, this);
            if (closestApparatus != null)
            {
                InteractWhileGrabbed(closestApparatus);
            }
        }

        // Start long press grab
        public virtual void StartGrab()
        {
            // Check if apparatus is free to be grabbed
            if (_parentAttachment != null)
            {
                Debug.Log(Head + " is attached to another apparatus and cannot be moved");
                return;
            }

            _preGrabPosition = transform.position;
            _isGrabbing = true;

            // Track initial velocity for physics calculations
            _physicsInfo.ResetMovementTracking();
        }

        // End long press grab
        public virtual void EndGrab()
        {
            Apparatus closestApparatus = ExperimentManager.Instance.GetApparatusInProximity(transform.position, this);
            if (closestApparatus != null)
            {
                InteractAfterGrabRelease(closestApparatus);
            }
            else
            {
                // No apparatus nearby, just place it
                transform.position = _preGrabPosition;
            }

            _isGrabbing = false;
        }
        #endregion


        #region PLACING
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
            _apparatusID = ApparatusManager.Instance.GenerateUniqueUShortId();
            _isPlacing = false;
            _indicatorMesh.SetActive(false);
            _apparatusMesh.SetActive(true);
            ExperimentManager.Instance.GoToIdleState();
            UIReferences.Instance.placingWindowName.text = "";
        }
        #endregion


        #region REPOSITIONING
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

        // What to do upon start repositioning
        public virtual void RepositionStart()
        {
            if (_parentAttachment)
            {
                return;
            }

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
        #endregion


        #region SELECTION
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
                if (fi.Value.hideField)
                    continue;

                Instantiate(
                    UIReferences.Instance.ButtonFieldPrefab,
                    fieldsParent
                    ).GetComponent<ButtonField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.SliderFields)
            {
                if (fi.Value.hideField)
                    continue;

                Instantiate(
                    UIReferences.Instance.SliderFieldPrefab,
                    fieldsParent
                    ).GetComponent<SliderField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.BoolFields)
            {
                if (fi.Value.hideField)
                    continue;
                Instantiate(
                    UIReferences.Instance.BoolFieldPrefab,
                    fieldsParent
                    ).GetComponent<BoolField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.DropdownFields)
            {
                if (fi.Value.hideField)
                    continue;
                Instantiate(
                    UIReferences.Instance.DropdownFieldPrefab,
                    fieldsParent
                    ).GetComponent<DropdownField>().Initialize(fi.Value);
            }
            foreach (var fi in Fields.TextFields)
            {
                if (fi.Value.hideField)
                    continue;
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
        #endregion

        // What to do when deleting
        public virtual void Delete()
        {
            ApparatusManager.Instance.ReleaseId(_apparatusID);
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public struct ApparatusPhysicsInfo
    {
        [SerializeField]
        private float _mass;

        private Vector3 _position;
        private Vector3 _previousFramePosition;
        private Vector3 _velocity;
        private Vector3 _grabOffset;

        public float Mass => _mass;
        public Vector3 Position => _position;
        public Vector3 Velocity => _velocity;
        public Vector3 GrabOffset
        {
            get => _grabOffset;
            set => _grabOffset = value;
        }

        public Vector3 Momentum => _velocity * _mass;

        /// <summary>
        /// Updates the physics information based on the new position
        /// </summary>
        /// <param name="newPosition">Current position of the apparatus</param>
        /// <param name="deltaTime">Time elapsed since last frame</param>
        public void UpdatePhysicsInfo(Vector3 newPosition, float deltaTime)
        {
            if (deltaTime <= 0)
            {
                Debug.LogWarning("Invalid deltaTime in UpdatePhysicsInfo");
                return;
            }

            _previousFramePosition = _position;
            _position = newPosition;

            _velocity = (_position - _previousFramePosition) / deltaTime;
        }

        public void ResetMovementTracking()
        {
            // Implementation of ResetMovementTracking method
        }
    }
}

