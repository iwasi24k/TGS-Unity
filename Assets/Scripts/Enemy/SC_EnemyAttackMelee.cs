using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AttackMelee  State")]
public class SC_EnemyAttackMelee : SC_EnemyBaceState
{
    [Header("Attack")]
    [Tooltip("攻撃判定が出るまでの時間")]
    [SerializeField] private float attackStartDelay = 0.3f;

    [Tooltip("攻撃後、次のStateへ戻るまでの時間")]
    [SerializeField] private float attackEndDelay = 0.8f;

    [Tooltip("攻撃範囲"), SerializeField]
    private Vector3 AttackAreaSize = new Vector3(2f, 2f, 2f);

    [Tooltip("攻撃範囲の前方向補正"), SerializeField]
    private float attackForwardOffset = 0.1f;

    [Tooltip("攻撃範囲の高さ補正"), SerializeField]
    private float attackHeightOffset = 0.2f;

    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private int damage = 1;

    private Collider[] overlapCollision = new Collider[16];

    [Tooltip("プレイヤーを探すタグ")]
    [SerializeField] private string playerTag = "Player";

    private float timer;
    private bool attacked;

    private Rigidbody rb;
    private Animator animator;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer = 0f;
        attacked = false;

        rb = Owner.GetComponent<Rigidbody>();
        animator = Owner.GetComponentInChildren<Animator>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetTrigger("tAxeAttack");
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (!attacked && timer >= attackStartDelay)
        {
            attacked = true;
            AttackPlayer(Owner);
        }

        if (timer >= attackEndDelay)
        {
            Manager.TransitionToNext();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        
    }

    private void AttackPlayer(GameObject Owner)
    {
        int count = SearchInArea(Owner, AttackAreaSize);

        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapCollision[i];

            if (col == null) continue;

            SC_PlayerHP playerHP = col.GetComponent<SC_PlayerHP>();

            if (playerHP == null)
            {
                playerHP = col.GetComponentInParent<SC_PlayerHP>();
            }

            if (playerHP != null)
            {
                playerHP.TakeDamage(damage);
                return; // 1回だけダメージ
            }
        }
    }

    private int SearchInArea(GameObject Owner, Vector3 areaSize)
    {
        Vector3 center =
            Owner.transform.forward * (areaSize.z * 0.5f + attackForwardOffset) +
            Owner.transform.up * (areaSize.y * 0.5f + attackHeightOffset) +
            Owner.transform.position;

        Quaternion rotation = Owner.transform.rotation;

        int count = Physics.OverlapBoxNonAlloc(
            center,
            areaSize * 0.5f,
            overlapCollision,
            rotation
        );

        return count;
    }

    public override void OnDrawGizmosSelectedState(GameObject Owner,SC_EnemyStatusManager Manager)
    {
        if (Owner == null) return;

        Vector3 center =
            Owner.transform.forward * (AttackAreaSize.z * 0.5f + attackForwardOffset) +
            Owner.transform.up * (AttackAreaSize.y * 0.5f + attackHeightOffset) +
            Owner.transform.position;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.color = Color.red;

        Gizmos.matrix = Matrix4x4.TRS(
            center,
            Owner.transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            AttackAreaSize
        );

        Gizmos.matrix = oldMatrix;
    }
}
