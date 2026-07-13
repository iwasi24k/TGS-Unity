using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack State")]
public class SC_EnemyAttack : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("弾数"), SerializeField] private int bulletNum = 3;
    [Tooltip("発射までのディレイ"), SerializeField] private float attackStartDelay = 0.5f;
    [Tooltip("弾速"), SerializeField] private float bulletSpeed = 10f;
    [Tooltip("発射間隔"), SerializeField] private float fireInterval = 0.2f;
    [Tooltip("拡散角度"), SerializeField] private float spreadAngle = 30f;
    [Tooltip("前方向オフセット"), SerializeField] private float spawnForwardOffset = 1.5f;
    [Tooltip("上方向オフセット"), SerializeField] private float spawnUpOffset = 0.5f;
    [Tooltip("左右オフセット"), SerializeField] private float spawnRightOffset = 0f;

    private Animator animator;
    private Rigidbody rb;
    private Quaternion startRotation;

    private int firedBulletCount;
    private float fireTimer;
    private float delayTimer;
    private bool isAttacking;
    private bool canFire;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        firedBulletCount = 0;
        fireTimer = 0f;
        delayTimer = 0f;
        isAttacking = true;
        canFire = false;
        rb = Owner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
        }
        startRotation = Owner.transform.rotation;

        animator = Owner.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Root Motionで勝手に回転する場合の対策
            animator.applyRootMotion = false;
            animator.SetTrigger("tGunAttack");
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        isAttacking = false;

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        // 攻撃終了なら何もしない
        if (!isAttacking) return;

        SC_EnemyAttackPoolProvider poolProvider = Owner.GetComponent<SC_EnemyAttackPoolProvider>();

        if (poolProvider == null)
        {
            isAttacking = false;
            Manager.TransitionToNext();
            return;
        }

        SC_ObjectPool bulletPool = poolProvider.GetBulletMultiPool();

        if (bulletPool == null)
        {
            isAttacking = false;
            Manager.TransitionToNext();
            return;
        }

        // 発射ディレイを進める
        if (!canFire)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer < attackStartDelay) return;
            canFire = true;
        }

        // 発射間隔を進める
        fireTimer += Time.deltaTime;
        if (fireTimer < fireInterval) return;

        // 全弾撃ち終わったら次のステートへ
        if (firedBulletCount >= bulletNum)
        {
            isAttacking = false;
            Manager.TransitionToNext();
            return;
        }

        fireTimer = 0f;

        // 拡散角度を計算
        float angleOffset = 0f;
        if (bulletNum > 1)
        {
            angleOffset = -spreadAngle * 0.5f + (spreadAngle / (bulletNum - 1)) * firedBulletCount;
        }

        // 発射方向と生成位置
        Quaternion rot = Quaternion.Euler(0f, angleOffset, 0f) * Owner.transform.rotation;
        Transform attackPoint = Manager.GetFirePoint(0);

        Vector3 spawnPos;

        if (attackPoint != null)
        {
            spawnPos = attackPoint.position;
        }
        else
        {
            spawnPos =
                Owner.transform.position +
                Owner.transform.forward * spawnForwardOffset +
                Owner.transform.up * spawnUpOffset +
                Owner.transform.right * spawnRightOffset;
        }

        // 発射エフェクト
        if (SC_EffectManager.Instance != null)
        {
            SC_EffectManager.Instance.PlayEffect("LaunchFire", spawnPos, rot);
        }

        GameObject bulletObj = bulletPool.GetObject(spawnPos, rot);

        if (bulletObj == null) return;

        SC_ReflectableMissile bullet =
            bulletObj.GetComponent<SC_ReflectableMissile>();

        if (bullet != null)
        {
            bullet.SetPool(bulletPool);
            bullet.OnGetFromPool();

            Vector3 dir = rot * Vector3.forward;

            bullet.Init(
                dir,
                bulletSpeed
            );
        }

        // 敵と弾の衝突を無視
        Collider ownerCol = Owner.GetComponent<Collider>();

        if (bullet != null)
        {
            bullet.SetIgnoredOwner(ownerCol);
        }

        // 弾を前に飛ばす
        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = rot * Vector3.forward * bulletSpeed;
        }

        firedBulletCount++;
    }
}