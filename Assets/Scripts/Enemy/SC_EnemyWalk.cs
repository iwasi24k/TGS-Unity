using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Walk State")]
public class SC_EnemyWalk : SC_EnemyBaceState
{
    [Header("Walk State Settings")]
    [Tooltip("ï‡çsë¨ìx"),SerializeField] private float walkSpeed = 2f;
    [Tooltip("ï‡çsîºåa"), SerializeField] private float walkRadius = 5f;

    public override void Enter(SC_EnemyStatusManager owner)
    {
        Debug.Log("Walk State Enter");
    }

    public override void Exit(SC_EnemyStatusManager owner)
    {
        Debug.Log("Walk State Exit");
    }

    public override void UpdateState(SC_EnemyStatusManager owner)
    {
        Debug.Log("Walk State Update");
    }
}
