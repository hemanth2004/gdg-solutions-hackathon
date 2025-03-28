using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

//Simply re-styles a gameObject name in the Hiearchy window to be black and all caps.
//Allows us to seperate our gameObjects and not lose our minds.

[InitializeOnLoad]
public static class HierarchySectionHeader
{
    static HierarchySectionHeader()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject != null && gameObject.name.StartsWith("//", System.StringComparison.Ordinal))
        {
            Color bg = new Color(0.175f,0.175f,0.175f);
            EditorGUI.DrawRect(selectionRect, bg);
            EditorGUI.DropShadowLabel(selectionRect, gameObject.name.Replace("/", "").ToUpperInvariant());
        }
    }
}
#endif