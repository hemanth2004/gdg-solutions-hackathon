using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ARLabs.UI
{
    public class ButtonField : Field
    {
        public TMP_Text buttonText;
        public ButtonFieldInfo buttonInfo;

        public void Initialize(ButtonFieldInfo _buttonFieldInfo)
        {
            buttonInfo = _buttonFieldInfo;

            labelText.text = buttonInfo.Label;
            buttonText.text = buttonInfo.buttonHead;
            _initialized = true;
        }

        public void OnClick()
        {
            if(_initialized)
                buttonInfo.Invoke();
        }
    }
}
