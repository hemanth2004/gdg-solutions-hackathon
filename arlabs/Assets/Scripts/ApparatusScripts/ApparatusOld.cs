using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class ApparatusOld : MonoBehaviour
{
    [SerializeField] private Transform uiFieldsParent;

    private UserJourneyManager journeyManager;
    private List<Transform> uiFields = new List<Transform>();

    private bool _highlighted = false;
    public bool isHighlighted
    {
        get { return _highlighted; }
        set 
        { 
            _highlighted = value;
            gameObject.GetComponent<Outline>().enabled = value;


            if (uiFields.Count > 0)
            {
                if (value)
                    foreach (Transform t in uiFields)
                    {
                        t.parent = journeyManager.uiReferences.highligthtWindowFieldParent;
                        t.localScale = Vector3.one;
                        t.localEulerAngles = Vector3.zero;
                    }
                else
                    foreach (Transform t in uiFields)
                        t.parent = uiFieldsParent;

                journeyManager.uiReferences.UpdateHighlightFieldLayoutGroup();
            }
        }
    }

    private void Start()
    {
        Outline s = gameObject.AddComponent<Outline>() as Outline;
        s.OutlineWidth = 8f;
        s.OutlineColor = Color.white;
        s.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        s.enabled = false;

        journeyManager = GameObject.FindAnyObjectByType<UserJourneyManager>();

        if (uiFieldsParent != null)
        {
            if (uiFieldsParent.childCount > 0)
            {
                foreach (Transform t in uiFieldsParent)
                    uiFields.Add(t);
            }
        }

        
    }
    public void Remove()
    {

    }

    #region Fields

    #endregion


}
