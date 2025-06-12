using System;
using System.Collections.Generic;

namespace ARLabs.AI
{
    [Serializable]
    public class AIResponse
    {
        // public Sequence sequence;
        public string xml;
    }

    [Serializable]
    public class Sequence
    {
        public List<string> vis;
        public List<string> audio;
    }
}
