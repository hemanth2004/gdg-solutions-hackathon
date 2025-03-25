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


            List<string> apparatusNames = new();
            foreach (Apparatus apparatus in expManager.ExperimentMasterSO.RequiredApparatus)
            {
                apparatusNames.Add(apparatus.name);
            }
            experiment.requiredApparatus = apparatusNames.ToArray();

            return experiment;
        }
    }
}
