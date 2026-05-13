using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AttackMelee State")]
public class SC_EnemyAttackMelee : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("攻撃判定の半径"), SerializeField] private float attackRadius = 1.2f;
    [Tooltip("攻撃判定の前方向オフセット"), SerializeField] private float attackForwardOffset = 1.0f;
    [Tooltip("攻撃判定の上方向オフセット"), SerializeField] private float attackUpOffset = 0.5f;
    [Tooltip("攻撃開始までのディレイ"), SerializeField] private float attackStartDelay = 0.5f;
    [Tooltip("攻撃後のディレイ"), SerializeField] private float attackEndDelay = 0.3f;

    private float delayTimer;
    private float endTimer;
    private bool isAttacking;
    private bool canAttack;
    private bool hasAttacked;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        delayTimer = 0f;
        endTimer = 0f;
        isAttacking = true;
        canAttack = false;
        hasAttacked = false;
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        isAttacking = false;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        // 攻撃ステート中でなければ処理しない
        if (!isAttacking) return;

        // 攻撃開始まで待つ
        if (!canAttack)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer < attackStartDelay) return;
            canAttack = true;
        }

        // 攻撃判定
        if (!hasAttacked)
        {
            Attack(Owner);
            hasAttacked = true;
        }

        // 攻撃後のディレイを計測
        endTimer += Time.deltaTime;
        if (endTimer < attackEndDelay) return;

        // 攻撃完了、次のステートへ
        Debug.Log("攻撃したよ");
        isAttacking = false;
        Manager.TransitionToNext();
    }

    private void Attack(GameObject Owner)
    {
        // 攻撃判定の中心位置を取得
        Vector3 attackPos = GetAttackCenter(Owner);

        // 攻撃範囲内にあるColliderを取得
        Collider[] hits = Physics.OverlapSphere(attackPos, attackRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];
            if (hit == null) continue;
            if (hit.gameObject == Owner) continue;
            if (hit.transform.IsChildOf(Owner.transform)) continue;
        }
    }

    // 攻撃判定の中心位置を取得
    private Vector3 GetAttackCenter(GameObject Owner)
    {
        return
            Owner.transform.position +
            Owner.transform.forward * attackForwardOffset +
            Owner.transform.up * attackUpOffset;
    }

    //攻撃判定を表示
    public void DrawAttackGizmo(GameObject Owner)
    {
        Vector3 center = GetAttackCenter(Owner);
        Vector3 size = Vector3.one * attackRadius * 2f;
       
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}