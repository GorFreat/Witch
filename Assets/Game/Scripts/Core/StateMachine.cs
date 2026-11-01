using UnityEngine;

public class StateMachine
{
    public State currentState {  get; private set; }

    public void Initialize(State initialState)
    {
        currentState = initialState;
        initialState?.EnterState();
    }
    
    public void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }
}
