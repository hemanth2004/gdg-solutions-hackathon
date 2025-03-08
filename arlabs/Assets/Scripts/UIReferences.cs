using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Gui;
using TMPro;
using UnityEngine.UI;

public class UIReferences : MonoBehaviour
{
    [Header("Main")]
    public LeanToggle buttonArray;

    [Header("HighlightWindow")]
    public TMP_Text highlightWindowNameHead;
    public LeanToggle highlightWindow;
    public Transform highligthtWindowFieldParent;
    public void UpdateHighlightFieldLayoutGroup()
    {
        highligthtWindowFieldParent.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
        StartCoroutine(ReloadLayout());
    }

    IEnumerator ReloadLayout()
    {
        yield return new WaitForEndOfFrame();
        highligthtWindowFieldParent.GetComponent<VerticalLayoutGroup>().childControlWidth = true;
    }

    public LeanToggle visualizationWindow;
    public LeanToggle electronToggle, potentialToggle;
    
}
