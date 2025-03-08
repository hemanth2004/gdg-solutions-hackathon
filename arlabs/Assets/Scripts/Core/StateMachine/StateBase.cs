namespace KG.StateMachine {
    public class StateBase<T> {
        public virtual void OnEnter(T entity) { }
        public virtual void OnExit(T entity) { }
        public virtual void OnSuspend(T entity) { }
        public virtual void OnUnsuspend(T entity) { }
        public virtual void RecieveContext(object context) { }
        
        public virtual void OnUpdate(T entity) { }
    }
}