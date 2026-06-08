using UnityEngine;

public abstract class SC_PlayerBaseState:ScriptableObject
{
    public abstract void Enter(GameObject owner, PlayerState stateList);
    public abstract void UpdateState(GameObject owner, PlayerState stateList);
    public abstract void FixedUpdateState(GameObject owner, PlayerState stateList);
    public abstract void Exit(GameObject owner, PlayerState stateList);
}
