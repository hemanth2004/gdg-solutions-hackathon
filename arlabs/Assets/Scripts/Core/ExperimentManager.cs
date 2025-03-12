using ARLabs.EventSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KG.StateMachine;
using UnityEngine.XR.ARFoundation;

namespace ARLabs.Core
{
    [RequireComponent(typeof(GameEventListener))]
    public class ExperimentManager : MonoBehaviour
    {
        #region Singleton
        public static ExperimentManager Instance;
        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(this); }
        }
        #endregion


        [SerializeField] private ExperimentMasterSO _experimentMasterSO;
        [SerializeField] private List<Apparatus> _instantiatedApparatus = new List<Apparatus>();
        [SerializeField] private ARPlaneManager _planeManager;


        public List<Apparatus> InstantiatedApparatus => _instantiatedApparatus;

        public ExperimentMasterSO ExperimentMasterSO => _experimentMasterSO;

        private StateMachine<ExperimentManager> _stateMachine;
        public ARPlaneManager planeManager => _planeManager;

        [Header("Preferences")]
        [SerializeField] private float _apparatusProximityDistance = 10f;

        public string curState;
        private void Start()
        {
            GameEventListener gameEventListener = GetComponent<GameEventListener>();
            gameEventListener.Event = _experimentMasterSO.CompletionEvent;
            _experimentMasterSO.CompletionEvent.RegisterListener(gameEventListener);

            // Statemachine setup
            _stateMachine = new StateMachine<ExperimentManager>(this);
            _stateMachine.AddState(new IdleState());
            _stateMachine.AddState(new PlaneDetectionState());
            _stateMachine.AddState(new PlacingState());
            _stateMachine.AddState(new RepositionState());

            _stateMachine.GoToState<IdleState>();

            // Startup Methods
            LoadLab();
            SetInfo();
            LoadApparatusMenu();
        }

        #region InitializationMethods
        // Methods that are called at the very start
        private void LoadLab()
        {
            // temp loading mimic
            Destroy(UIReferences.Instance.LoadingScreen, 2f);
        }
        private void SetInfo()
        {
            UIReferences.Instance.BreadcrumbText.text = ExperimentMasterSO.Subject.ToString() + " > "
                + "Class " + ExperimentMasterSO.Class + " > "
                + ExperimentMasterSO.Topic.ToString() + " > \n<b>"
                + ExperimentMasterSO.ExperimentName + "</b>";

        }
        private void LoadApparatusMenu()
        {
            foreach (Apparatus apparatus in ExperimentMasterSO.RequiredApparatus)
            {
                ApparatusSpawnUI newApparatus = Instantiate(UIReferences.Instance.ApparatusUIPrefab, UIReferences.Instance.apparatusListParent).GetComponent<ApparatusSpawnUI>();

                newApparatus.url = apparatus.Webpage;
                newApparatus.apparatusID = apparatus.ApparatusID;
                TMPro.TMP_Text mainText = newApparatus.transform.GetChild(1).GetChild(0).GetComponent<TMPro.TMP_Text>();
                mainText.text = "<b>" + apparatus.Head + "</b>\n" + apparatus.Description;

                newApparatus.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = apparatus.Thumbnail;
                newApparatus.apparatus = apparatus;
            }
        }
        #endregion

        // Demonstration Purspose Only
        private void Update()
        {
            if (_instantiatedApparatus.Count == 3)
            {
                _experimentMasterSO.CompletionEvent.Raise();
            }
            curState = _stateMachine.GetCurrentStateName();
        }

        #region PlaneMethods
        public void SetPlanesVisibility(bool visible)
        {
            foreach (var plane in _planeManager.trackables)
            {
                plane.GetComponent<ARPlaneMeshVisualizer>().enabled = visible;
                plane.GetComponent<MeshRenderer>().enabled = visible;
            }
        }


        public void ClearDetectedPlanes()
        {
        }
        public void DonePlaneDetection()
        {
            _stateMachine.GoToState<IdleState>();
        }

        public void StartPlaneDetection()
        {
            _stateMachine.GoToState<PlaneDetectionState>();
        }
        #endregion

        #region StateControlMethods
        // Methods so that other scripts like ApparatusManager
        // Can change states
        public void GoToPlacingState()
        {
            _stateMachine.GoToState<PlacingState>();
        }
        public void GoToIdleState()
        {
            _stateMachine.GoToState<IdleState>();
        }
        public void GoToRepositionState()
        {
            _stateMachine.GoToState<RepositionState>();
        }
        #endregion

        // Add this property
        public bool IsRepositioning => _stateMachine.IsInState<RepositionState>();

        private Apparatus _closestCache = null;
        private Vector3 _closestCachePosition;

        /// <summary>
        /// Returns the closest apparatus within proximity distance of the given position,
        /// excluding the source apparatus and all its child attachments
        /// </summary>
        public Apparatus GetApparatusInProximity(Vector3 position, Apparatus sourceApparatus)
        {
            // Check if cached result is still valid
            if (_closestCache != null &&
                Vector3.Distance(position, _closestCachePosition) < _apparatusProximityDistance &&
                _closestCache != sourceApparatus &&
                !IsChildOf(sourceApparatus, _closestCache)) // Add check for child relationship
            {
                Debug.Log("Returning cached apparatus: " + _closestCache.name);
                return _closestCache;
            }

            // Find closest apparatus
            Apparatus closestApparatus = null;
            float closestDistance = float.MaxValue;

            foreach (Apparatus apparatus in _instantiatedApparatus)
            {
                // Skip if it's the source apparatus or any of its children
                if (apparatus == sourceApparatus || IsChildOf(sourceApparatus, apparatus)) continue;

                float distance = Vector3.Distance(position, apparatus.transform.position);
                Debug.Log($"Checking apparatus: {apparatus.name}, Distance: {distance}");

                if (distance < _apparatusProximityDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestApparatus = apparatus;
                }
            }

            // Update cache
            _closestCache = closestApparatus;
            _closestCachePosition = position;

            if (closestApparatus != null)
            {
                Debug.Log("Closest apparatus found: " + closestApparatus.name);
            }
            else
            {
                Debug.Log("No apparatus found within proximity.");
            }

            return closestApparatus;
        }

        /// <summary>
        /// Checks if an apparatus is a child (direct or indirect) of a potential parent apparatus
        /// </summary>
        private bool IsChildOf(Apparatus potentialParent, Apparatus apparatus)
        {
            // Check direct children
            if (potentialParent.ChildAttachments.ContainsKey(apparatus))
                return true;

            // Recursively check children of children
            foreach (var child in potentialParent.ChildAttachments.Keys)
            {
                if (IsChildOf(child, apparatus))
                    return true;
            }

            return false;
        }

    }
}