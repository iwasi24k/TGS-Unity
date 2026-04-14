using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Idle State")]
public class SC_EnemyIdle : SC_EnemyBaceState
{
    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Idle State Enter");
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Idle State Exit");

    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Idle State Update");

        owner.TransitionToNext();
    }
}
