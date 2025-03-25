using UnityEngine;

namespace ARLabs.Core
{
    public class Visualization : MonoBehaviour
    {
        private bool _isVisualizing = false;

        public bool IsVisualizing => _isVisualizing;

        public virtual void BeginVisualization()
        {
            _isVisualizing = true;
        }

        public virtual void EndVisualization()
        {
            _isVisualizing = false;
        }
    }
}
