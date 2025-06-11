using System.Collections;
using UnityEngine;
using ARLabs.Core;
using ARLabs.UI;
using System;

namespace ARLabs.AI
{
    public class ApparatusSetAction : IAction
    {
        public string apparatusId;
        public string fieldToSet;
        public string value;

        public static Apparatus FindApparatusToApply(string idToFind)
        {
            foreach(Apparatus apparatus in ExperimentManager.Instance.InstantiatedApparatus)
            {
                if (apparatus.ApparatusID == ushort.Parse(idToFind))
                    return apparatus;
            }
            return null;
        }

        public bool Execute()
        {
            try
            {
                Debug.Log($"Looking for apparatus with ID: {apparatusId}");
                Apparatus apparatus = FindApparatusToApply(apparatusId);
                
                if (apparatus == null)
                {
                    Debug.LogError($"No apparatus found with ID: {apparatusId}");
                    return false;
                }


                FieldsList fieldsList = apparatus.Fields;

                foreach (var pair in fieldsList.SliderFields)
                {
                    Debug.Log($"Slider field key: {pair.Key}");
                    
                    if (pair.Key.Equals(fieldToSet, StringComparison.OrdinalIgnoreCase))
                    {
                        int parsedValue = int.Parse(value);
                        pair.Value.SetValue(parsedValue);
                        return true;
                    }
                }
                foreach (var pair in fieldsList.DropdownFields)
                {
                    if (pair.Key.Equals(fieldToSet, StringComparison.OrdinalIgnoreCase))
                    {
                        pair.Value.SetValue(int.Parse(value));
                        return true;
                    }
                }
                foreach (var pair in fieldsList.TextFields)
                {
                    if (pair.Key.Equals(fieldToSet, StringComparison.OrdinalIgnoreCase))
                    {
                        pair.Value.SetValue(value);
                        return true;
                    }
                }
                foreach (var pair in fieldsList.BoolFields)
                {
                    if (pair.Key.Equals(fieldToSet, StringComparison.OrdinalIgnoreCase))
                    {
                        pair.Value.SetValue(bool.Parse(value));
                        return true;
                    }
                }
                foreach (var pair in fieldsList.ButtonFields)
                {
                    if (pair.Key.Equals(fieldToSet, StringComparison.OrdinalIgnoreCase))
                    {
                        pair.Value.Invoke();
                        return true;
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                Debug.LogError($"Error setting field {fieldToSet} to {value} for apparatus {apparatusId}: {e.Message} \n{e.StackTrace}");
                return false;
            }

        }
    }
}