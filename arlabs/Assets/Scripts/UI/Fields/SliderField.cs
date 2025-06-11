using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARLabs.UI
{
    public class SliderField : Field
    {
        public UnityEngine.UI.Slider slider;
        public TMPro.TMP_Text value;
        public SliderFieldInfo sliderInfo;

        public void Initialize(SliderFieldInfo _sliderFieldInfo)
        {
            sliderInfo.field = this;

            sliderInfo = _sliderFieldInfo;

            labelText.text = sliderInfo.Label;
            slider.minValue = sliderInfo.rangeMin;
            slider.maxValue = sliderInfo.rangeMax;
            slider.wholeNumbers = sliderInfo.wholeNumbers;
            slider.value = sliderInfo.value;

            value.gameObject.SetActive(sliderInfo.displayValue);
            value.text = slider.value.ToString("F2");

            // Apply readonly state
            slider.interactable = !sliderInfo.isReadOnly;

            _initialized = true;
        }

        public void OnChange()
        {
            if (sliderInfo.value != slider.value && _initialized)
            {
                sliderInfo.value = slider.value;
                value.text = slider.value.ToString("F2");
                sliderInfo.OnChange?.Invoke(slider.value);
            }
        }

        public void SetValue(float _value)
        {
            slider.value = _value;
            OnChange();
        }
    }
}
