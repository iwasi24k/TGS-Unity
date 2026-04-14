using UnityEngine;

public abstract class SC_EnemyBaceState : ScriptableObject
{
    public abstract void Enter(GameObject Owner, SC_EnemyStatusManager Manager);
    public abstract void Exit(GameObject Owner, SC_EnemyStatusManager Manager);
    public abstract void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager);
}
