using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Lean.Gui;
using ARLabs.Core;

namespace ARLabs.UI
{
    public class OperationContext : MonoBehaviour
    {
        public static OperationContext Instance;

        [SerializeField] private LeanToggle _window;
        [SerializeField] private OperationContextItem _itemPrefab;
        [SerializeField] private Transform _contentParent;

        private void Awake()
        {
            Instance = this;
        }

        private Apparatus _fromApparatus;
        private List<ApparatusOperation> _operations = new();

        public void StartMenu(List<ApparatusOperation> operations, Apparatus fromApparatus)
        {
            _operations = operations;
            _fromApparatus = fromApparatus;
            foreach (var operation in operations)
            {
                OperationContextItem itemObject = Instantiate(_itemPrefab, _contentParent);
                itemObject.Initialize(operation.name);
            }
            _window.TurnOn();
        }

        public void ClickOperation(string name)
        {
            foreach (var operation in _operations)
            {
                if (operation.name == name)
                {
                    operation.operationEvent?.Invoke(_fromApparatus, operation.actualTargetApparatus);
                }
            }
        }

        public void EndMenu()
        {
            foreach (Transform child in _contentParent)
            {
                Destroy(child.gameObject);
            }
            _window.TurnOff();
        }
    }
}
