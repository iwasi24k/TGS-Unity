using UnityEngine;

public abstract class SC_EnemyBaceState : ScriptableObject
{
    public abstract void Enter(GameObject Owner, SC_EnemyStatusManager Manager);
    public abstract void Exit(GameObject Owner, SC_EnemyStatusManager Manager);
    public abstract void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager);

    public virtual void FixedUpdateState(GameObject Owner, SC_EnemyStatusManager Manager) { }

    public virtual void OnCollisionEnterState(GameObject Owner, SC_EnemyStatusManager Manager, Collision collision) { }

    public virtual void OnDrawGizmosSelectedState(GameObject Owner,SC_EnemyStatusManager Manager) { }
}
