using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using ARLabs.Core;
using System.Linq;

namespace ARLabs.AI
{
    [System.Serializable]
    public class AIChatMessage
    {
        public string sessionID;
        public string prompt;
        public string base64Image;
        public ExperimentContext experimentContext;
        public string language = "English";
    }

    [System.Serializable]
    public class ExperimentContext
    {
        public string schoolClass;
        public string subject;
        public string topic;
        public string name;
        public string theory;
        public string procedure;


        /**
        * Apparatus
        */
        public string[] requiredApparatus; // backend will request gcs using the string name
        public ApparatusInstance[] instantiatedApparatus; // backend will request gcs using the string name


        /**
        * Visualizations
        */
        public Visualization[] availableVisualizations;

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


            // Get the required apparatus names
            List<string> apparatusNames = new();
            foreach (Apparatus apparatus in expManager.ExperimentMasterSO.RequiredApparatus)
            {
                apparatusNames.Add(apparatus.name);
            }
            experiment.requiredApparatus = apparatusNames.ToArray();


            // Get the currently placed apparatus names
            List<ApparatusInstance> instantiatedApparatus = new();
            foreach (var apparatus in expManager.InstantiatedApparatus)
            {
                instantiatedApparatus.Add(new ApparatusInstance(apparatus.Head, apparatus.ApparatusID));
            }
            experiment.instantiatedApparatus = instantiatedApparatus.ToArray();


            // Get the possible visualizations
            List<Visualization> visualizationsPossible = new();
            foreach (var visualization in expManager.ExperimentMasterSO.Visualizations)
            {
                visualizationsPossible.Add(new Visualization(visualization.VisualizationName, visualization.VisualizationDesc));
            }
            experiment.availableVisualizations = visualizationsPossible.ToArray();


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

        [System.Serializable]
        public struct ApparatusInstance
        {
            public string name;
            public ushort apparatusId;

            public ApparatusInstance(string name, ushort id)
            {
                this.name = name;
                this.apparatusId = id;
            }
        }


    }
}
