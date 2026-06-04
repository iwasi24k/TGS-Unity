using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Idle State")]
public class SC_EnemyIdle : SC_EnemyBaceState
{
    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {

    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {

    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Manager.TransitionToNext();
    }
}
