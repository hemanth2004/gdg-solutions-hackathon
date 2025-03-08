using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ARLabs.UI
{
    public class BoolField : Field
    {
        public Lean.Gui.LeanToggle toggle;
        public BoolFieldInfo boolInfo;

        public void Initialize(BoolFieldInfo _boolFieldInfo)
        {
            boolInfo = _boolFieldInfo;

            labelText.text = boolInfo.Label;
            toggle.On = boolInfo.value;
            _initialized = true;
        }

        public void OnChange()
        {
            if (boolInfo.value != toggle.On && _initialized)
            {
                boolInfo.value = toggle.On;
                boolInfo.OnChange?.Invoke(boolInfo.value);
            }
        }
    }
}
