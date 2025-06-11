using UnityEngine;
using System.Xml;

namespace ARLabs.AI
{
    public class ActionHandler : MonoBehaviour
    {
        public static bool HandleAction(string actionText)
        {
            try
            {
                Debug.Log($"Processing action: {actionText}");
                
                // Parse the XML action element
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(actionText);
                
                // The actionText should be a complete <Action>...</Action> element
                XmlNode actionNode = doc.SelectSingleNode("Action");
                if (actionNode == null)
                {
                    Debug.LogError("No Action element found in action XML");
                    return false;
                }
                
                // Check if this action contains a Visualization element
                XmlNode visualizationNode = actionNode.SelectSingleNode("Visualization");
                if (visualizationNode != null)
                {
                    return HandleVisualizationAction(visualizationNode);
                }
                
                // Check if this action contains a SetField element
                XmlNode setFieldNode = actionNode.SelectSingleNode("SetField");
                if (setFieldNode != null)
                {
                    return HandleSetFieldAction(setFieldNode);
                }
                
                Debug.LogWarning($"Unknown action type in: {actionText}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing action XML: {e.Message}");
                return false;
            }
        }
        
        private static bool HandleVisualizationAction(XmlNode visualizationNode)
        {
            try
            {
                string name = GetAttribute(visualizationNode, "name");
                string value = GetAttribute(visualizationNode, "value");
                
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                {
                    Debug.LogError("Visualization action missing required attributes (name, value)");
                    return false;
                }
                
                VisualizationAction action = new VisualizationAction
                {
                    visualizationName = name,
                    stateToSet = value
                };
                
                bool success = action.Execute();
                Debug.Log($"Visualization action executed: {name} = {value}, Success: {success}");
                return success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error executing visualization action: {e.Message}");
                return false;
            }
        }
        
        private static bool HandleSetFieldAction(XmlNode setFieldNode)
        {
            try
            {
                string apparatusId = GetAttribute(setFieldNode, "apparatusId");
                string fieldToSet = GetAttribute(setFieldNode, "fieldToSet");
                string value = GetAttribute(setFieldNode, "value");
                
                if (string.IsNullOrEmpty(apparatusId) || string.IsNullOrEmpty(fieldToSet) || string.IsNullOrEmpty(value))
                {
                    Debug.LogError("SetField action missing required attributes (apparatusId, fieldToSet, value)");
                    return false;
                }
                
                ApparatusSetAction action = new ApparatusSetAction
                {
                    apparatusId = apparatusId,
                    fieldToSet = fieldToSet,
                    value = value
                };
                
                bool success = action.Execute();
                Debug.Log($"SetField action executed: Apparatus {apparatusId}, Field {fieldToSet} = {value}, Success: {success}");
                return success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error executing set field action: {e.Message}");
                return false;
            }
        }
        
        private static string GetAttribute(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = node.Attributes?[attributeName];
            return attribute?.Value ?? "";
        }
    }

    public interface IAction
    {
        bool Execute();
    }
}
