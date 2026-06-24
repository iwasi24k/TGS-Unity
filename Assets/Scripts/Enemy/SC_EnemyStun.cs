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

        animator = Owner.GetComponentInChildren<Animator>();

        if (animator == null) return;

        AttackType attackType = Manager.GetStunAttackType();

        animator.SetBool("tKnockback_R", false);
        animator.SetBool("tKnockback_L", false);

        switch (attackType)
        {
            case AttackType.Weak1:
                animator.SetBool("tKnockback_R", true);
                break;

            case AttackType.Weak2:
                animator.SetBool("tKnockback_L", true);
                break;

            default:
                animator.SetBool("tKnockback_R", true);
                break;
        }
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
        if (animator == null) return;

        animator.SetBool("tKnockback_R", false);
        animator.SetBool("tKnockback_L", false);
    }
}