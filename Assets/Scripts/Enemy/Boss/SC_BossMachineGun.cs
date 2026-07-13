using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Machine Gun State")]
public class SC_BossMachineGunState : SC_EnemyBaceState
{
    [Tooltip("攻撃開始までの時間"), SerializeField]
    private float startDelay = 0.3f;

    [Tooltip("ミサイルを生成する高さ"), SerializeField]
    private float spawnHeight = 1.5f;

    private const int CircleFirePointMaxCount = 12;

    private float timer;
    private float fireTimer;
    private int firedCount;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer = 0f;
        fireTimer = 0f;
        firedCount = 0;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        if (timer < startDelay) return;

        fireTimer += Time.deltaTime;

        if (firedCount < boss.GetMachineGunBulletCount())
        {
            if (fireTimer >= boss.GetMachineGunInterval())
            {
                fireTimer = 0f;
                firedCount++;

                FireOneMissile(Owner, boss);
            }
        }
        else
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        
    }

    private void FireOneMissile(GameObject Owner, SC_BossAttackController boss)
    {
        SC_ObjectPool pool = boss.GetStraightMissilePool();
        if (pool == null) return;

        Transform player = boss.GetPlayer();

        Vector3 baseDir = Owner.transform.forward;

        if (player != null)
        {
            Vector3 toPlayer = player.position - Owner.transform.position;

            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                baseDir = toPlayer.normalized;
            }
        }

        Transform firePoint =
            GetNearestFirePointToPlayer(Owner, player);

        Vector3 spawnPos;

        if (firePoint != null)
        {
            spawnPos = firePoint.position;
        }
        else
        {
            spawnPos = Owner.transform.position;
            spawnPos.y = 0.0f;
            spawnPos += Vector3.up * spawnHeight;
        }

        float randomAngle = Random.Range(
            -boss.GetMachineGunRandomAngle(),
            boss.GetMachineGunRandomAngle()
        );

        Vector3 dir =
            Quaternion.Euler(0f, randomAngle, 0f) *
            baseDir;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Owner.transform.forward;
            dir.y = 0f;
        }

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Vector3.forward;
        }

        dir.Normalize();

        // 発射エフェクト
        if (SC_EffectManager.Instance != null)
        {
            Vector3 effectPoint = firePoint.position;
            SC_EffectManager.Instance.PlayEffect("LaunchFire", effectPoint, Quaternion.LookRotation(dir));
        }

        GameObject missileObj = pool.GetObject(
            spawnPos,
            Quaternion.LookRotation(dir)
        );

        if (missileObj == null) return;

        SC_StraightMissile missile =
            missileObj.GetComponent<SC_StraightMissile>();

        if (missile != null)
        {
            missile.SetPool(pool);
            missile.OnGetFromPool();

            missile.Init(
                dir,
                boss.GetMachineGunMissileSpeed(),
                0f
            );
        }
    }

    private Transform GetNearestFirePointToPlayer(
    GameObject Owner,
    Transform player
)
    {
        if (Owner == null) return null;
        if (player == null) return null;

        SC_EnemyStatusManager statusManager =
            Owner.GetComponent<SC_EnemyStatusManager>();

        if (statusManager == null) return null;

        Transform[] firePointList =
            statusManager.GetFirePointList();

        if (firePointList == null || firePointList.Length == 0)
        {
            return null;
        }

        int usableFirePointCount = Mathf.Min(CircleFirePointMaxCount, firePointList.Length);

        Transform nearestFirePoint = null;
        float nearestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < usableFirePointCount; i++)
        {
            Transform firePoint = firePointList[i];

            if (firePoint == null) continue;

            Vector3 diff =
                player.position -
                firePoint.position;

            diff.y = 0f;

            float distanceSqr = diff.sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestFirePoint = firePoint;
            }
        }

        return nearestFirePoint;
    }
}
