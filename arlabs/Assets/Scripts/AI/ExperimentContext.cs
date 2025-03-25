using UnityEngine;
using System.Collections.Generic;
using ARLabs.Core;

namespace ARLabs.AI
{
    [System.Serializable]
    public class ExperimentContext : IAIMessage
    {
        public string mainPrompt { get; set; }
        public string sessionID { get; set; }
        public string schoolClass;
        public string subject;
        public string topic;
        public string name;
        public string theory;
        public string procedure;
        public string[] requiredApparatus;
        public string[] instantiatedApparatus;
        public string[] visualizations;

        public static ExperimentContext GetExperimentContext()
        {
            ExperimentManager expManager = ExperimentManager.Instance;
            ExperimentContext experiment = new ExperimentContext();

            experiment.name = expManager.ExperimentMasterSO.ExperimentName;
            experiment.theory = expManager.ExperimentMasterSO.Theory;
            experiment.procedure = expManager.ExperimentMasterSO.Procedure;
            experiment.subject = expManager.ExperimentMasterSO.Subject.ToString();
            experiment.topic = expManager.ExperimentMasterSO.Topic.ToString();
            experiment.schoolClass = expManager.ExperimentMasterSO.Class.ToString();
            experiment.sessionID = expManager.SessionID;

            // Get the required apparatus names
            List<string> apparatusNames = new();
            foreach (Apparatus apparatus in expManager.ExperimentMasterSO.RequiredApparatus)
            {
                apparatusNames.Add(apparatus.name);
            }
            experiment.requiredApparatus = apparatusNames.ToArray();

            // Get the currently placed apparatus names
            List<string> instantiatedApparatusNames = new();
            foreach (var apparatus in expManager.InstantiatedApparatus)
            {
                instantiatedApparatusNames.Add(apparatus.Head);
            }
            experiment.instantiatedApparatus = instantiatedApparatusNames.ToArray();

            // Get the possible visualizations
            List<string> visualizationsPossibleNames = new();
            foreach (var visualization in expManager.ExperimentMasterSO.Visualizations)
            {
                visualizationsPossibleNames.Add(visualization.VisualizationName);
            }
            experiment.visualizations = visualizationsPossibleNames.ToArray();

            // Get the possible actions
            // ...
            // todo

            return experiment;
        }
    }
}
