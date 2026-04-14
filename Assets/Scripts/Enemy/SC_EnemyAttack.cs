using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack State")]
public class SC_EnemyAttack : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("”­Ë‚Á‚·‚é‹Ê‚Ì”"), SerializeField] private int bulletNum = 3;

    public override void Enter(SC_EnemyStatusManager owner)
    {
        Debug.Log("Attack State Enter");
    }

    public override void Exit(SC_EnemyStatusManager owner)
    {
        Debug.Log("Attack State Exit");
    }

    public override void UpdateState(SC_EnemyStatusManager owner)
    {
        Debug.Log("Attack State Update");
    }
}
