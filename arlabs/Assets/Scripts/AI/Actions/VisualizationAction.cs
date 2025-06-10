using UnityEngine;
using ARLabs.Core;

namespace ARLabs.AI
{
    public class VisualizationAction : IAction
    {
        public string visualizationName;
        public string stateToSet; // "true" or "false" based on whether visualization should be on or off 

        public bool Execute()
        {
            try
            {
                // hardcoded parsing lol
                VisualizationManager.Instance.ToggleVisualization(visualizationName, stateToSet == "true");
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
