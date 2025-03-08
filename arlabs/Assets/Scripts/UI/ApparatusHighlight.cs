using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Lean.Gui;
using ARLabs.Core;

namespace ARLabs.UI
{
    /// <summary>
    /// The script that handles an apparatus' inspection UI
    /// </summary>

    public class ApparatusHighlight : MonoBehaviour
    {
        [SerializeField] private ApparatusManager apparatusManager;

        [SerializeField] private TMP_Text windowHead;
        [SerializeField] private LeanToggle highlightWindowToggle;
        [SerializeField] private LeanToggle repositionWindowToggle;
        
        public Apparatus current = null;

        //Call to highlight the selected apparatus
        public void Highlight(Apparatus apparatus)
        {   
            current = apparatus;

            windowHead.text = current.Head;
            

            highlightWindowToggle.TurnOn();
        }

        //Call to unhighlight and close the window
        public void UnHighlight()
        {
            current = null;
            highlightWindowToggle.TurnOff();
        }



        public void OnClickReposition()
        {
            highlightWindowToggle.TurnOff();
            repositionWindowToggle.TurnOn();
            apparatusManager.RepositionSelectedApparatus();
        }

        public void OnEndReposition()
        {
            highlightWindowToggle.TurnOn();
            repositionWindowToggle.TurnOff();
        }

        public void OnClickRemove()
        {
            highlightWindowToggle.TurnOff();
            repositionWindowToggle.TurnOff();
            current?.Delete();
        }



        private void LoadFields()
        {

        }
    }
}