using KG.StateMachine;
//using Mono.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private StateMachine<GameManager> _stateMachine;

    private void Start()
    {
        _stateMachine = new StateMachine<GameManager>(this);
        //_stateMachine.AddState(new Instructions());
        //_stateMachine.AddState(new Idle());
        //_stateMachine.AddState(new DetectingPlanes());
    }
}
