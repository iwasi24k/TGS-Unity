using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Down State")]
public class SC_BossDownState : SC_EnemyBaceState
{
    [Tooltip("Downしている時間"), SerializeField] private float downTime = 3.0f;
    [Tooltip("Down終了後に必ず使う近接波動StateのStateList番号"), SerializeField] private int meleeWaveStateIndex = 2;
    [Tooltip("Down中にRigidbodyの速度を止めるか"), SerializeField] private bool stopVelocityOnEnter = true;

    private float timer;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Down State Entered");
        timer = 0f;

        if (stopVelocityOnEnter)
        {
            Rigidbody rb = Owner.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (timer >= downTime)
        {
            Manager.ChangeState(meleeWaveStateIndex);
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Down State Exited");
    }
}
