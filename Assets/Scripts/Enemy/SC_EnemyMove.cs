using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Move State")]
public class SC_EnemyMove : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("移動速度"), SerializeField] private int moveSpeed = 3;
    [Tooltip("移動距離"), SerializeField] private float moveDistance = 3f;

    [Tooltip("この秒数動かなければアウト"), SerializeField] private float stuckCheckTime = 1.0f;
    [Tooltip("この距離以下なら動いてない扱い"), SerializeField] private float stuckThreshold = 0.1f;

    private Animator animator;

    private Vector3 moveDirection;
    private Vector3 startPosition;
    private Rigidbody rb;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    private SC_MoveLookTarget moveLookTarget;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        rb = Owner.GetComponent<Rigidbody>();

        startPosition = Owner.transform.position;

        lastPosition = Owner.transform.position;
        stuckTimer = 0f;

        moveDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            moveDirection = Owner.transform.forward;
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                moveDirection = Vector3.forward;
            }

            moveDirection.Normalize();
        }

        animator = Owner.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("bMove", true);
        }

        moveLookTarget = Manager.GetMoveLookTarget();

        if (moveLookTarget != null)
        {
            moveLookTarget.SetLookDirection(moveDirection);
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (animator != null)
        {
            animator.SetBool("bMove", false);
        }

        if (moveLookTarget != null)
        {
            moveLookTarget.ClearLookDirection();
        }

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (rb == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 direction = player.transform.position - Owner.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            rb.MoveRotation(Quaternion.LookRotation(direction.normalized));
        }

        rb.linearVelocity = moveDirection * moveSpeed;

        if (moveLookTarget != null)
        {
            moveLookTarget.SetLookDirection(moveDirection);
        }

        float distance = Vector3.Distance(startPosition, Owner.transform.position);

        float movedDistance = Vector3.Distance(lastPosition, Owner.transform.position);

        if (movedDistance < stuckThreshold)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = Owner.transform.position;

        if (distance >= moveDistance || stuckTimer >= stuckCheckTime)
        {
            rb.linearVelocity = Vector3.zero;
            Manager.TransitionToNext();
        }
    }
}
