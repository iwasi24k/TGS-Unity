using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Move Melee State")]
public class SC_EnemyMoveMelee : SC_EnemyBaceState
{
    [Header("Move")]
    [Tooltip("ˆÚ“®‘¬“x"), SerializeField]
    private float moveSpeed = 3.0f;

    [Tooltip("‚±‚Ì‹——£ˆÈ“à‚É“ü‚Á‚½‚çUŒ‚State‚ÖˆÚs")]
    [SerializeField]
    private float attackRange = 1.5f;

    [Tooltip("ƒvƒŒƒCƒ„[‚ğ’T‚·ƒ^ƒO")]
    [SerializeField]
    private string playerTag = "Player";

    [Header("Stuck Check")]
    [Tooltip("‚±‚Ì•b”“®‚©‚È‚¯‚ê‚Î‹l‚Ü‚èˆµ‚¢")]
    [SerializeField]
    private float stuckCheckTime = 1.0f;

    [Tooltip("‚±‚Ì‹——£ˆÈ‰º‚È‚ç“®‚¢‚Ä‚È‚¢ˆµ‚¢")]
    [SerializeField]
    private float stuckThreshold = 0.1f;

    private Rigidbody rb;
    private Animator animator;
    private Transform player;

    private Vector3 lastPosition;
    private float stuckTimer;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        rb = Owner.GetComponent<Rigidbody>();
        animator = Owner.GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            player = null;
        }

        lastPosition = Owner.transform.position;
        stuckTimer = 0f;

        if (animator != null)
        {
            animator.SetBool("bMove", true);
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (rb == null) return;
        if (player == null)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 toPlayer = player.position - Owner.transform.position;
        toPlayer.y = 0f;

        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer <= attackRange)
        {
            rb.linearVelocity = Vector3.zero;
            Manager.TransitionToNext();
            return;
        }

        if (toPlayer.sqrMagnitude <= 0.0001f)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 moveDir = toPlayer.normalized;

        rb.linearVelocity = moveDir * moveSpeed;

        rb.MoveRotation(
            Quaternion.LookRotation(moveDir)
        );

        CheckStuck(Owner, Manager);
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (animator != null)
        {
            animator.SetBool("bMove", false);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player = null;
    }

    private void CheckStuck(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        float movedDistance =
            Vector3.Distance(lastPosition, Owner.transform.position);

        if (movedDistance < stuckThreshold)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = Owner.transform.position;

        if (stuckTimer >= stuckCheckTime)
        {
            // ‹l‚Ü‚Á‚½‚çˆê’UŸ‚ÌState‚Ö
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }

            Manager.TransitionToNext();
        }
    }
}
