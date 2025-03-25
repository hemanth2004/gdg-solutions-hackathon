using UnityEngine;

namespace ARLabs.AI
{
    public class WhatsThisButton : MonoBehaviour
    {
        public void OnClick()
        {
            ExperimentPrompt experimentPrompt = new ExperimentPrompt();
            experimentPrompt.sessionID = System.Guid.NewGuid().ToString();
            experimentPrompt.mainPrompt = "What is this experiment about?";

            string jsonMessage = JsonUtility.ToJson(experimentPrompt);
            APIHandler.Instance.AskBackendWithImage(jsonMessage);
        }

        public class ExperimentPrompt : IAIMessage
        {
            public string mainPrompt { get; set; }
            public string sessionID { get; set; }
        }
    }
}