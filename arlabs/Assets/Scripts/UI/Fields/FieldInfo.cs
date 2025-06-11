using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using AYellowpaper.SerializedCollections;

namespace ARLabs.UI
{

    [Serializable]
    public class FieldsList
    {
        [SerializedDictionary("Key", "Conf")]
        public SerializedDictionary<string, SliderFieldInfo> SliderFields = new();
        [SerializedDictionary("Key", "Conf")]
        public SerializedDictionary<string, DropdownFieldInfo> DropdownFields = new();
        [SerializedDictionary("Key", "Conf")]
        public SerializedDictionary<string, TextFieldInfo> TextFields = new();
        [SerializedDictionary("Key", "Conf")]
        public SerializedDictionary<string, BoolFieldInfo> BoolFields = new();
        [SerializedDictionary("Key", "Conf")]
        public SerializedDictionary<string, ButtonFieldInfo> ButtonFields = new();
    }

    public interface IFieldInfo
    {
        bool hideField { get; set; }
        string Label { get; set; }
        [HideInInspector] FieldType FieldType { get; set; }
        Action<object> OnChange { get; set; }
        bool isReadOnly { get; set; }
    }

    [Serializable]
    public class SliderFieldInfo : IFieldInfo
    {

        public SliderField field;

        public bool hideField { get; set; }
        public string label;
        [HideInInspector] public FieldType fieldType;
        public Action<object> onChange;
        public bool isReadOnly { get; set; }

        public float value;
        public float rangeMin, rangeMax;
        public bool wholeNumbers;
        public bool displayValue;

        // Implementation of FieldInfo properties
        public string Label
        {
            get => label;
            set => label = value;
        }

        [HideInInspector]
        public FieldType FieldType
        {
            get => FieldType.Slider;
            set { }
        }

        public Action<object> OnChange
        {
            get => onChange;
            set => onChange = value;
        }

        public void SetValue(float value)
        {
            if (value > rangeMax || value < rangeMin)
            {
                Debug.LogError($"Value {value} is out of range for field {label}");
                return;
            }
            this.value = value;
            field?.SetValue(value);
        }
    }

    [Serializable]
    public class DropdownFieldInfo : IFieldInfo
    {
        public DropdownField field;

        public bool hideField { get; set; }
        public string label;
        [HideInInspector] public FieldType fieldType;
        public Action<object> onChange;
        public bool isReadOnly { get; set; }

        public List<string> options = new List<string>();
        public int value;

        // Implementation of FieldInfo properties
        public string Label
        {
            get => label;
            set => label = value;
        }

        [HideInInspector]
        public FieldType FieldType
        {
            get => FieldType.Dropdown;
            set { }
        }

        public Action<object> OnChange
        {
            get => onChange;
            set => onChange = value;
        }

        public void SetValue(int value)
        {
            if (value < 0 || value >= options.Count)
            {
                Debug.LogError($"Value {value} is out of range for field {label}");
                return;
            }
            this.value = value;
            field?.SetValue(value);
        }
    }

    [Serializable]
    public class TextFieldInfo : IFieldInfo
    {
        public TextField field;

        public bool hideField { get; set; }
        public string label;
        [HideInInspector] public FieldType fieldType;
        public Action<object> onChange;
        public bool isReadOnly { get; set; }

        public string placeholder;
        public string value;

        // Implementation of FieldInfo properties
        public string Label
        {
            get => label;
            set => label = value;
        }

        [HideInInspector]
        public FieldType FieldType
        {
            get => FieldType.Text;
            set { }
        }

        public Action<object> OnChange
        {
            get => onChange;
            set => onChange = value;
        }

        public void SetValue(string value)
        {
            this.value = value;
            field?.SetValue(value);
        }
    }

    [Serializable]
    public class BoolFieldInfo : IFieldInfo
    {
        public BoolField field;

        public bool hideField { get; set; }
        public string label;
        [HideInInspector] public FieldType fieldType;
        public Action<object> onChange;
        public bool isReadOnly { get; set; }

        public bool value;

        // Implementation of FieldInfo properties
        public string Label
        {
            get => label;
            set => label = value;
        }

        [HideInInspector]
        public FieldType FieldType
        {
            get => FieldType.Bool;
            set { }
        }

        public Action<object> OnChange
        {
            get => onChange;
            set => onChange = value;
        }

        public void SetValue(bool value)
        {
            this.value = value;
            field?.SetValue(value);
        }
    }

    [Tooltip("Subscribe to button click by subscribing to the onChange of type Action<object>")]

    [Serializable]
    public class ButtonFieldInfo : IFieldInfo
    {
        public ButtonField field;

        public bool hideField { get; set; }
        public string label;
        [HideInInspector] public FieldType fieldType;
        public Action<object> onChange;
        public bool isReadOnly { get; set; }

        public string buttonHead;

        // Implementation of FieldInfo properties
        public string Label
        {
            get => label;
            set => label = value;
        }

        [HideInInspector]
        public FieldType FieldType
        {
            get => FieldType.Button;
            set { }
        }

        public Action<object> OnChange
        {
            get => onChange;
            set => onChange = value;
        }

        public void Invoke()
        {
            OnChange?.Invoke(0);
        }
    }   


}
