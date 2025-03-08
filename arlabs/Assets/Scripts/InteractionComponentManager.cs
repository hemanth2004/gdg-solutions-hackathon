using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class InteractionComponentManager : MonoBehaviour
{
    //the NAME of the interaction object must be of the form "INTERACTION.X"
    //where X is the name of the component that handles interaction (casesensi)
    public string INTERACTION_PREFIX = "INTERACTION";

    [UDictionary.Split(70, 30)]
    public UDictionary1 interactionTypes;
    [Serializable]
    public class UDictionary1 : UDictionary<string, Type> { }

    //private Dictionary<string, Type> interactionTypes = new Dictionary<string, Type>();

    private void Awake()
    {
        //Add all entries for interaction
        interactionTypes.Add("BatteryOn", typeof(BatteryOn));
    }

    //chat gpt magic
    public bool TrySetVariable(GameObject gameObject, string typeName, string variableName, object value)
    {
        if (interactionTypes.TryGetValue(typeName, out Type componentType))
        {
            Component component = gameObject.GetComponent(componentType) as Component;
            if (component != null)
            {
                Type variableType = component.GetType();

                FieldInfo fieldInfo = variableType.GetField(variableName);

                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(component, value);
                    return true;
                }
                else
                {
                    Debug.LogError($"Variable '{variableName}' not found in component.");
                }
            }
            else
            {
                Debug.LogError($"Component type '{typeName}' not found on the provided GameObject.");
            }
        }
        else
        {
            Debug.LogError($"Component type '{typeName}' not found in dictionary.");
        }

        return false;
    }

    public bool TryCallFunction(GameObject gameObject, string typeName, string functionName)
    {
        if (interactionTypes.TryGetValue(typeName, out Type componentType))
        {
            Component component = gameObject.GetComponent(componentType) as Component;
            if (component != null)
            {
                MethodInfo methodInfo = componentType.GetMethod(functionName);

                if (methodInfo != null)
                {
                    methodInfo.Invoke(component, null);
                    return true;
                }
                else
                {
                    Debug.LogError($"Function '{functionName}' not found in component.");
                }
            }
            else
            {
                Debug.LogError($"Component type '{typeName}' not found on the provided GameObject.");
            }
        }
        else
        {
            Debug.LogError($"Component type '{typeName}' not found in dictionary.");
        }

        return false;
    }
}