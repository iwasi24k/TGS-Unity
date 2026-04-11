using UnityEngine;

public abstract class SC_EnemyBaceState : ScriptableObject
{ 
    public abstract void Enter(SC_EnemyStatusManager owner);
    public abstract void Exit(SC_EnemyStatusManager owner);
    public abstract void UpdateState(SC_EnemyStatusManager owner);
}
