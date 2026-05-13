using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveMelee State")]
public class SC_MoveMelee : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("移動速度"), SerializeField] private float moveSpeed = 3f;
    [Tooltip("攻撃ステートに移行する距離"), SerializeField] private float attackDistance = 2f;

    private Rigidbody rb;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        rb = Owner.GetComponent<Rigidbody>();
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        // ステート終了時に停止
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (rb == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // プレイヤーへの方向を取得
        Vector3 direction = player.transform.position - Owner.transform.position;
        direction.y = 0f;

        // プレイヤーとの距離を確認
        float distance = direction.magnitude;

        // 攻撃できる間合いに入ったら次のステートへ
        if (distance <= attackDistance)
        {
            rb.linearVelocity = Vector3.zero;
            Manager.TransitionToNext();
            return;
        }

        // 方向がない場合は処理しない
        if (direction.sqrMagnitude <= 0.001f) return;

        // 向き変更
        rb.MoveRotation(Quaternion.LookRotation(direction));

        // プレイヤーを追従して移動
        rb.linearVelocity = direction.normalized * moveSpeed;
    }
}