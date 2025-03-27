using UnityEngine;

namespace ARLabs.Core
{
    public class ExperimentSelecter : MonoBehaviour
    {
        [SerializeField] private ExperimentManager _experimentManager;
        [SerializeField] private ExperimentMasterSO[] _experiments;
        public void Awake()
        {
            int selectedExperiment = PlayerPrefs.GetInt("selected_experiment", 0);
            _experimentManager.SetExperiment(_experiments[selectedExperiment]);
        }
    }
}
