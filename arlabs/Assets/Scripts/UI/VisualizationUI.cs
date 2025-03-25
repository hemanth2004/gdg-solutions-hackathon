using UnityEngine;
using UnityEngine.UI;
using ARLabs.Core;

namespace ARLabs.UI
{
    public class VisualizationUI : MonoBehaviour
    {
        public Text head;
        public Visualization visualization;
        public Lean.Gui.LeanToggle toggle;

        public void Initialize()
        {
            head.text = visualization.VisualizationHead;
        }

        public void OnToggleOn()
        {
            if (visualization.CheckIfVisualizationIsPossible())
            {
                visualization.BeginVisualization();
            }
            else
            {
                toggle.TurnOff();
            }
        }

        public void OnToggleOff()
        {
            visualization.EndVisualization();
        }
    }
}
