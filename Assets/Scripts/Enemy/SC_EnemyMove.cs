using UnityEditorInternal;
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

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Move State Enter");

        rb = Owner.GetComponent<Rigidbody>();

        // 開始位置記録
        startPosition = Owner.transform.position;

        lastPosition = Owner.transform.position;
        stuckTimer = 0f;

        // ランダム方向（XZ平面）
        moveDirection = new Vector3
            (
            Random.Range(-1f, 1f),0f,Random.Range(-1f, 1f)
            ).normalized;

        animator = Owner.GetComponent<Animator>();

        animator.SetBool("bMove", true);
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Move State Exit");
        animator.SetBool("bMove", false);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Move State Update");

        if (rb == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player == null) return;

        Vector3 direction = player.transform.position - Owner.transform.position;
        direction.y = 0f;

        // 向き変更
        rb.MoveRotation(Quaternion.LookRotation(direction));

        // velocityで移動
        rb.linearVelocity = moveDirection * moveSpeed;

        // 移動距離チェック
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

        // 位置更新
        lastPosition = Owner.transform.position;

        if (distance >= moveDistance || stuckTimer >= stuckCheckTime)
        {
            // 停止
            rb.linearVelocity = Vector3.zero;

            // 次のステートへ
            Manager.TransitionToNext();
        }
    }
}
