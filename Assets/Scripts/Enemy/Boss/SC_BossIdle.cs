using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Idle State")]
public class SC_BossIdleState : SC_EnemyBaceState
{
    [Tooltip("Idleó‘Ô‚Å‘Ò‹@‚·‚éŽžŠÔ"), SerializeField] private float waitTime = 2.0f;
    [Tooltip("UŒ‚‘I‘ðState‚ÌStateList”Ô†"), SerializeField] private int attackSelectStateIndex = 1;

    private float timer;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Idle State Enter");
        timer = 0f;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (timer >= waitTime)
        {
            Manager.ChangeState(attackSelectStateIndex);
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Idle State Exit");
    }
}