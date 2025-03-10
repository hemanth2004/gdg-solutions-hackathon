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


    }
}