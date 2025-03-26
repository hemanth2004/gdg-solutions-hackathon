using UnityEngine;
using UnityEngine.UI;
using ARLabs.UI;

namespace ARLabs.Core
{
    public class VisualizationManager : MonoBehaviour
    {
        public static VisualizationManager Instance;
        private void Awake()
        {
            Instance = this;
        }

        [SerializeField] private GameObject visualizationUIPrefab;
        [SerializeField] private Transform visualizationsUIContainer;
        [SerializeField] private Transform visualizationsContainer;

        public void Initialize()
        {
            foreach (var visualization in ExperimentManager.Instance.ExperimentMasterSO.Visualizations)
            {
                GameObject ui = Instantiate(visualizationUIPrefab, visualizationsUIContainer);
                GameObject visualizationObject = Instantiate(visualization.gameObject, visualizationsContainer);

                ui.GetComponent<VisualizationUI>().visualization = visualizationObject.GetComponent<Visualization>();
                ui.GetComponent<VisualizationUI>().Initialize();
            }
        }

        public void ToggleVisualization(string visualizationName, bool isVisible)
        {
            foreach (Transform vizUI in visualizationsUIContainer)
            {
                if (vizUI.GetComponent<VisualizationUI>().visualization.VisualizationName == visualizationName)
                {
                    if (isVisible)
                    {
                        vizUI.GetComponent<VisualizationUI>().OnToggleOn();
                    }
                    else
                    {
                        vizUI.GetComponent<VisualizationUI>().OnToggleOff();
                    }
                }
            }
        }
    }
}