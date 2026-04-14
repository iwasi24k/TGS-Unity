using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/BlownAway State")]
public class SC_EnemyBlownAway : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("‚«”ò‚Î‚³‚ê‚éŠî‘b‹——£"), SerializeField] private float blownAwayDistance = 5f;

    public override void Enter(SC_EnemyStatusManager owner)
    {
        Debug.Log("BlownAway State Enter");
    }

    public override void Exit(SC_EnemyStatusManager owner)
    {
        Debug.Log("BlownAway State Exit");
    }

    public override void UpdateState(SC_EnemyStatusManager owner)
    {
        Debug.Log("BlownAway State Update");
    }
}
