using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack State")]
public class SC_EnemyAttack : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("弾数"), SerializeField] private int bulletNum = 3;
    [Tooltip("弾プレハブ"), SerializeField] private GameObject bulletPrefab;
    [Tooltip("発射までのディレイ"), SerializeField] private float attackStartDelay = 0.5f;
    [Tooltip("弾速"), SerializeField] private float bulletSpeed = 10f;
    [Tooltip("発射間隔"), SerializeField] private float fireInterval = 0.2f;
    [Tooltip("拡散角度"), SerializeField] private float spreadAngle = 30f;
    [Tooltip("前方向オフセット"), SerializeField] private float spawnForwardOffset = 1.5f;
    [Tooltip("上方向オフセット"), SerializeField] private float spawnUpOffset = 0.5f;
    [Tooltip("左右オフセット"), SerializeField] private float spawnRightOffset = 0f;

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
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        isAttacking = false;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        // 攻撃終了なら何もしない
        if (!isAttacking) return;

        // プレハブ未設定なら何もしない
        if (bulletPrefab == null) return;

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
        Vector3 spawnPos =
            Owner.transform.position +
            Owner.transform.forward * spawnForwardOffset +
            Owner.transform.up * spawnUpOffset +
            Owner.transform.right * spawnRightOffset;

        // 弾生成
        GameObject bulletObj = Object.Instantiate(bulletPrefab, spawnPos, rot);

        // 敵と弾の衝突を無視
        Collider ownerCol = Owner.GetComponent<Collider>();
        Collider bulletCol = bulletObj.GetComponent<Collider>();
        if (ownerCol != null && bulletCol != null)
        {
            Physics.IgnoreCollision(ownerCol, bulletCol);
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