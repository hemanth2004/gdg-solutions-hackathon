using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using ARLabs.Core;

namespace ARLabs.UI
{
    public class OperationContextItem : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _text;

        public void Initialize(string text)
        {
            _text.text = text;
        }

        public void OnClick()
        {
            OperationContext.Instance.ClickOperation(_text.text);
            OperationContext.Instance.EndMenu();
        }
    }
}
