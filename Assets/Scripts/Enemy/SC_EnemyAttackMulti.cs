using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(menuName = "Enemy/AttackMulti State")]
public class SC_EnemyAttackMulti : SC_EnemyBaceState
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

    private float fireTimer;
    private float delayTimer;
    private bool isAttacking;
    private bool canFire;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
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
        if (!isAttacking) return;

        LockOwnerRotation(Owner);

        // 発射ディレイ
        if (!canFire)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer < attackStartDelay) return;
            canFire = true;
        }

        // 発射間隔
        fireTimer += Time.deltaTime;
        if (fireTimer < fireInterval) return;

        fireTimer = 0f;

        // 同時に bulletNum 個の弾を発射する
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

        Transform player = null;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        for (int i = 0; i < bulletNum; i++)
        {
            float angleOffset = 0f;

            if (bulletNum > 1)
            {
                angleOffset =
                    -spreadAngle * 0.5f +
                    (spreadAngle / (bulletNum - 1)) * i;
            }

            Quaternion rot =
                Owner.transform.rotation *
                Quaternion.Euler(0f, angleOffset, 0f);

            Transform attackPoint = Manager.GetAttackPoint();

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

            GameObject bulletObj = bulletPool.GetObject(spawnPos, rot);


            if (bulletObj == null) continue;

            SC_StraightMissile bullet =
                bulletObj.GetComponent<SC_StraightMissile>();

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

            Collider ownerCol = Owner.GetComponent<Collider>();
            Collider bulletCol = bulletObj.GetComponent<Collider>();

            if (ownerCol != null && bulletCol != null)
            {
                Physics.IgnoreCollision(ownerCol, bulletCol);
            }
        }
        

        // 1 回撃ったら次のステートへ
        isAttacking = false;
        Manager.TransitionToNext();
    }

    private void LockOwnerRotation(GameObject Owner)
    {
        Owner.transform.rotation = startRotation;

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
        }

    }
}