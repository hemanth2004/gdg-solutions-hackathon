using System.Collections;
using UnityEngine;
using ARLabs.Core;
using ARLabs.UI;

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
                Apparatus apparatus = FindApparatusToApply(apparatusId);

                FieldsList fieldsList = apparatus.Fields;

                foreach (var pair in fieldsList.SliderFields)
                {
                    if (pair.Key == fieldToSet)
                    {
                        pair.Value.field.SetValue(int.Parse(value));
                    }
                }
                foreach (var pair in fieldsList.DropdownFields)
                {
                    if (pair.Key == fieldToSet)
                    {
                        pair.Value.field.SetValue(int.Parse(value));
                    }
                }
                foreach (var pair in fieldsList.TextFields)
                {
                    if (pair.Key == fieldToSet)
                    {
                        pair.Value.field.SetValue(value);
                    }
                }
                foreach (var pair in fieldsList.BoolFields)
                {
                    if (pair.Key == fieldToSet)
                    {
                        pair.Value.field.SetValue(bool.Parse(value));
                    }
                }
                foreach (var pair in fieldsList.ButtonFields)
                {
                    if (pair.Key == fieldToSet)
                    {
                        pair.Value.field.Call();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}