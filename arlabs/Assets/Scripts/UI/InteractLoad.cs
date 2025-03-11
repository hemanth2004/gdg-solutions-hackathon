using UnityEngine;

namespace ARLabs.Core
{
    public class InteractLoad : MonoBehaviour
    {
        public UnityEngine.UI.Image bottom, top;

        private void Start()
        {
            SetActive(false);
        }

        public void SetActive(bool active)
        {
            bottom.gameObject.SetActive(active);
            top.gameObject.SetActive(active);
        }

        public void SetValue(float value)
        {
            top.fillAmount = value;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}