using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.EventSystem;

namespace ARLabs.Core
{
    public enum Subject
    {
        NONE = 0,
        Physics,
        Chemistry,
    }

    public enum Topic
    {
        NONE = 0,
        Electricity,
        Magnetism,
        Mechanics,
        SaltAnalysis,
        Other
    }

    [CreateAssetMenu(fileName = "Experiment Master", menuName = "AR Labs/Experiment Masters/Base Experiment Master")]
    public class ExperimentMasterSO : ScriptableObject
    {
        [SerializeField] private Subject _subject;
        [SerializeField] private Topic _topic;
        [SerializeField] private string _experimentName;
        [SerializeField, Range(6, 12)] private int _class = 11;
        [SerializeField] private List<Apparatus> _requiredApparatus = new List<Apparatus>();
        [SerializeField] private List<Apparatus> _instantiatedApparatus = new List<Apparatus>();
        [SerializeField] private GameEventSO _experimentCompletionEvent;


        public List<Apparatus> RequiredApparatus => _requiredApparatus;
        public GameEventSO CompletionEvent => _experimentCompletionEvent;
        public Subject Subject => _subject;
        public Topic Topic => _topic;
        public int Class => _class;
        public string ExperimentName => _experimentName;

    }
}
