using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ARLabs.UI
{
    public class DropdownField : Field
    {
        public TMP_Dropdown dropdown;
        public DropdownFieldInfo dropdownInfo;

        public void Initialize(DropdownFieldInfo _dropdownFieldInfo)
        {
            dropdownInfo = _dropdownFieldInfo;

            labelText.text = dropdownInfo.Label;

            List<TMP_Dropdown.OptionData> options = new();
            foreach (string t in dropdownInfo.options)
            {
                TMP_Dropdown.OptionData newData = new();
                newData.text = t;
                options.Add(newData); 
            }
            dropdown.AddOptions(options);

            dropdown.value = Mathf.Clamp(dropdownInfo.value, 0, options.Count);
            Debug.Log(dropdownInfo.value);

            _initialized = true;
            dropdown.interactable = !dropdownInfo.isReadOnly;
        }

        public void OnChange()
        {
            if (dropdownInfo.value != dropdown.value && _initialized)
            {
                dropdownInfo.value = dropdown.value;
                dropdownInfo.OnChange?.Invoke(dropdown.value);
            }
        }
    }
}
