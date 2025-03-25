using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using ARLabs.Core;
using System.Linq;

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
        public Visualization[] visualizations;

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
            List<Visualization> visualizationsPossible = new();
            foreach (var visualization in expManager.ExperimentMasterSO.Visualizations)
            {
                visualizationsPossible.Add(new Visualization(visualization.VisualizationName, visualization.VisualizationDesc));
            }
            experiment.visualizations = visualizationsPossible.ToArray();

            // Get the possible actions
            // ...
            // todo

            return experiment;
        }

        [System.Serializable]
        public struct Visualization
        {
            public string name;
            public string desc;

            public Visualization(string name, string desc)
            {
                this.name = name;
                this.desc = desc;
            }
        }

    }
}
