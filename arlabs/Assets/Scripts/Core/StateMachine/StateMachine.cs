using System.Collections.Generic;
using UnityEngine;

namespace KG.StateMachine
{
    public class StateMachine<T>
    {
        private bool _suspended;
        private readonly T _owner;
        private StateBase<T> _currentState;
        private StateBase<T> _previousState;
        private Dictionary<string, StateBase<T>> _allStates;

        public StateMachine(T owner)
        {
            _owner = owner;
            _allStates = new Dictionary<string, StateBase<T>>();
        }

        public void AddState(StateBase<T> state)
        {
            _allStates.Add(GetStateName(state), state);
        }

        public void RemoveState(StateBase<T> state)
        {
            _allStates.Remove(GetStateName(state));
        }

        public bool IsInState<TState>()
        {
            return _currentState is TState;
        }

        public bool WasInState<TState>()
        {
            return _previousState is TState;
        }

        public void GoToState<TState>()
        {
            string stateType = typeof(TState).ToString();

            _previousState = _currentState;

            if (_currentState != null)
            {
                _currentState.OnExit(_owner);
            }

            if (_allStates.TryGetValue(stateType, out _currentState))
            {
                _currentState.OnEnter(_owner);
                return;
            }

            Debug.LogError($"{stateType} Is Not Available");
        }

        public void GoToState<TState>(object context)
        {
            string stateType = typeof(TState).ToString();

            _previousState = _currentState;

            if (_currentState != null)
            {
                _currentState.OnExit(_owner);
            }

            if (_allStates.TryGetValue(stateType, out _currentState))
            {
                _currentState.RecieveContext(context);
                _currentState.OnEnter(_owner);
                return;
            }

            Debug.LogError($"{stateType} Is Not Available");
        }

        public void Update()
        {
            if (_suspended)
                return;

            _currentState.OnUpdate(_owner);
        }

        public void SuspendUpdate()
        {
            _currentState.OnSuspend(_owner);
            _suspended = true;
        }

        public void UnsuspendUpdate()
        {
            _currentState.OnUnsuspend(_owner);
            _suspended = false;
        }

        public string GetCurrentStateName()
        {
            return _suspended ? "SUSPENDED" : GetStateName(_currentState);
        }

        public string GetPreviousStateName()
        {
            return _previousState == null ? "NULL" : GetStateName(_previousState);
        }

        private string GetStateName(StateBase<T> state)
        {
            return state.GetType().ToString();
        }
    }
}
