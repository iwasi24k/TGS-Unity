using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Idle State")]
public class SC_EnemyIdle : SC_EnemyBaceState
{
    public override void Enter(SC_EnemyStatusManager owner)
    {
        Debug.Log("Idle State Enter");
    }

    public override void Exit(SC_EnemyStatusManager owner)
    {
        Debug.Log("Idle State Exit");
    }

    public override void UpdateState(SC_EnemyStatusManager owner)
    {
        Debug.Log("Idle State Update");
    }
}
