using UnityEngine;
using System;

namespace ARLabs.Chemistry
{
    [Serializable]
    public enum MatterState
    {
        Liquid,
        Solid,
        Gas
    }

    [Serializable]
    public enum SolidType
    {
        Amorphous,
        Crystalline
    }

    [Serializable]
    public class MetterDetails { }

    [Serializable]
    public class SolidDetails : MetterDetails
    {
        public SolidType solidType;
    }

    [Serializable]
    public class Matter
    {
        public string formula;
        public Color color;
        public MatterState state;
        public float mass;
        public float volume;
        public float temperature;
        public MetterDetails matterDetails;
    }
}


