using UnityEngine;
using UnityEngine.Events;
using System;


namespace ARLabs.Core
{
    [System.Serializable]
    public class ApparatusOperation
    {
        public string name;
        public Type targetApparatusType;
        public Apparatus actualTargetApparatus = null;
        public Action<Apparatus, Apparatus> operationEvent;
    }
}


