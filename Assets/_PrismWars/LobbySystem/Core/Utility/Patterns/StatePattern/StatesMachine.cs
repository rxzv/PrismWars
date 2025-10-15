using System.Collections.Generic;

/// <summary>
/// State manager to handle basic states
/// </summary>
/// <typeparam name="TStateIDType">The type of the state base ID</typeparam>
public class StatesMachine<TContex> 
{
    public TContex Contex;

    public StateBase<TContex> CurrentState;
    public StateBase<TContex> PreviousState;
    public List<StateBase<TContex>> StateList;

    public StatesMachine(TContex contex)
    {
        Contex = contex;
    }


    public void RunStateMachine(StateBase<TContex> entryPoint)
    {
        CurrentState = entryPoint;
        CurrentState.OnEnter(Contex);
    }


    public void AddState(StateBase<TContex> state)
    {
        StateList = (StateList == null) ? new List<StateBase<TContex>>() : StateList;
        StateList.Add(state);
    }


    public void ChangeState(StateBase<TContex> state)
    {
        if (CurrentState == state) return;

        PreviousState = CurrentState;
        CurrentState.OnExit(Contex);
        CurrentState = state;
        CurrentState.OnEnter(Contex);

    }
}


