using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Stun State")]
public class SC_EnemyStun : SC_EnemyBaceState
{
    [Tooltip("スタン時間")]
    [SerializeField] private float stunTime = 0.5f;

    [Tooltip("スタン中に速度を止めるか")]
    [SerializeField] private bool stopVelocity = true;

    private float timer;

    private Animator animator;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer = 0f;

        Rigidbody rb = Owner.GetComponent<Rigidbody>();

        if (rb != null && stopVelocity)
        {
            rb.linearVelocity = Vector3.zero;
        }

        animator = Owner.GetComponent<Animator>();
        animator.SetBool("bHit", true);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (timer >= stunTime)
        {
            Manager.ReturnFromStun();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        animator.SetBool("bHit", false);
    }
}