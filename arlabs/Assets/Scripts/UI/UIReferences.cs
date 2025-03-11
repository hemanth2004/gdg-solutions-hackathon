using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lean.Gui;

namespace ARLabs.Core
{
    public class UIReferences : MonoBehaviour
    {
        #region Singleton
        public static UIReferences Instance;
        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(this); }
        }
        #endregion

        [Header("StartScreen")]
        public TMP_Text BreadcrumbText;
        public GameObject LoadingScreen;
        [Header("Apparatus")]
        public GameObject ApparatusUIPrefab;
        public Transform apparatusListParent;
        public Transform apparatusFieldListParent;
        public LeanToggle apparatusMenuWindow;
        public LeanToggle repositionWindow;
        [Header("Common")]
        public LeanToggle placingWindow;
        public LeanToggle highlightWindow;
        public TMP_Text higlightWindowName;
        public TMP_Text placingWindowName;
        public TMP_Text repositionWindowName;
        [Header("Fields")]
        public GameObject TextFieldPrefab;
        public GameObject BoolFieldPrefab;
        public GameObject DropdownFieldPrefab;
        public GameObject SliderFieldPrefab;
        public GameObject ButtonFieldPrefab;

    }
}
