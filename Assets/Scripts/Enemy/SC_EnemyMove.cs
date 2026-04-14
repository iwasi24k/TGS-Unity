using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Move State")]
public class SC_EnemyMove : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("ˆÚ“®‘¬“x"), SerializeField] private int moveSpeed = 3;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Move State Enter");
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Move State Exit");
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Move State Update");
    }
}
