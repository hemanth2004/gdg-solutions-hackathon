using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ARLabs.UI
{
    public enum FieldType
    {
        Button,
        Bool,
        Slider,
        Text,
        Dropdown
    };
        
    public class Field : MonoBehaviour
    {
        public TMP_Text labelText;
        public FieldType fieldType;

        protected bool _initialized = false;

    }
}
