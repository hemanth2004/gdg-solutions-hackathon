using UnityEngine;
using System.Collections.Generic;
using ARLabs.Core;

namespace ARLabs.AI
{
    public class ExplainExperimentButton : MonoBehaviour
    {
        public void OnClick()
        {
            ExperimentContext experiment = ExperimentContext.GetExperimentContext();
            experiment.mainPrompt = "Explain the experiment concisely, no filler words";

            // Convert to JSON string before sending
            string jsonMessage = JsonUtility.ToJson(experiment);
            Debug.Log(jsonMessage);
            APIHandler.Instance.AskBackendWithText("Explain the experiment concisely, no filler words\n" + jsonMessage);
        }
    }
}

