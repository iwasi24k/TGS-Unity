using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Split Falling Missile State")]
public class SC_BossSplitFallingMissileState : SC_EnemyBaceState
{
    [Tooltip("攻撃開始までの待ち時間"), SerializeField]
    private float startDelay = 0.5f;

    [Tooltip("発射演出後、落下ミサイルを生成するまでの待ち時間")]
    [SerializeField] private float fireDelay = 0.5f;

    [Tooltip("攻撃Stateを終了するまでの時間"), SerializeField]
    private float endDelay = 4.0f;


    [Tooltip("ミサイルを落下させる開始高度"), SerializeField]
    private float fallHeight = 10.0f;

    [Header("Fire Point")]
    [Tooltip("SC_EnemyStatusManager の firePointList の番号")]
    [SerializeField] private int firePointIndex = 12;

    private float timer;
    private float fireTimer;
    private float endTimer;
    private bool isFireFinished;

    private int launchCount;
    private int fallSpawnCount;

    private readonly List<float> fallSpawnTimers = new List<float>();

    private Animator animator;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer = 0f;
        fireTimer = 0f;
        endTimer = 0f;

        launchCount = 0;
        fallSpawnCount = 0;

        isFireFinished = false;
        fallSpawnTimers.Clear();

        animator = Owner.GetComponentInChildren<Animator>();
        animator.SetBool("tMissileShot", true);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        int missileCount = boss.GetSplitFallingMissileCount();

        // まだ全処理が終わっていない
        if (!isFireFinished)
        {
            // 攻撃開始待ち
            if (timer < startDelay) return;

            // ==============================
            // 1. 発射演出を一定間隔で出す
            // ==============================
            if (launchCount < missileCount)
            {
                fireTimer += Time.deltaTime;

                if (fireTimer >= boss.GetSplitFallingInterval())
                {
                    fireTimer = 0f;

                    // FirePointから上に飛ぶ演出
                    SpawnLaunchVisualMissile(Owner, Manager, boss);

                    // この発射に対応する落下ミサイルを予約
                    fallSpawnTimers.Add(fireDelay);

                    launchCount++;
                }
            }

            // ==============================
            // 2. 予約された落下ミサイルを遅延生成
            // ==============================
            for (int i = fallSpawnTimers.Count - 1; i >= 0; i--)
            {
                fallSpawnTimers[i] -= Time.deltaTime;

                if (fallSpawnTimers[i] <= 0f)
                {
                    SpawnSplitFallingMissile(Owner, boss);

                    fallSpawnTimers.RemoveAt(i);
                    fallSpawnCount++;
                }
            }

            // ==============================
            // 3. 全発射演出 + 全落下生成が終わったか
            // ==============================
            if (launchCount >= missileCount &&
                fallSpawnCount >= missileCount &&
                fallSpawnTimers.Count == 0)
            {
                isFireFinished = true;
                endTimer = 0f;
            }

            return;
        }

        // ==============================
        // 4. 全弾処理後、endDelayで次の攻撃へ
        // ==============================
        endTimer += Time.deltaTime;

        if (endTimer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }


    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        animator.SetBool("tMissileShot", false);
    }

    private void SpawnSplitFallingMissile(GameObject Owner, SC_BossAttackController boss)
    {
        SC_ObjectPool splitPool = boss.GetSplitFallingMissilePool();
        if (splitPool == null) return;

        float minRadius = boss.GetFallingMissileMinRadius();
        float maxRadius = boss.GetStageRadius();

        if (maxRadius < minRadius)
        {
            float temp = maxRadius;
            maxRadius = minRadius;
            minRadius = temp;
        }

        float angle = Random.Range(0f, 360f);
        float radius = Random.Range(minRadius, maxRadius);

        Vector3 offset =
            Quaternion.Euler(0f, angle, 0f) *
            Vector3.forward *
            radius;

        Vector3 targetPos = Owner.transform.position + offset;
        targetPos.y = 0f;

        Vector3 spawnPos =
            targetPos +
            Vector3.up * fallHeight;

        GameObject missileObj = splitPool.GetObject(
            spawnPos,
            Quaternion.identity
        );

        if (missileObj == null) return;

        SC_SplitFallingMissile missile =
            missileObj.GetComponent<SC_SplitFallingMissile>();

        if (missile != null)
        {
            missile.SetPool(splitPool);
            missile.SetWarningPool(boss.GetWarningCirclePool());
            missile.SetChildMissilePool(boss.GetStraightMissilePool());
            missile.SetChildWarningPool(boss.GetWarningRectanglePool());

            missile.OnGetFromPool();

            missile.Init(
                targetPos,
                fallHeight,
                boss.GetSplitFallingTime(),
                boss.GetSplitChildMissileCount(),
                boss.GetSplitChildMissileSpeed()
            );
        }
    }

    private void SpawnLaunchVisualMissile(
        GameObject Owner,
        SC_EnemyStatusManager Manager,
        SC_BossAttackController boss)
    {
        SC_ObjectPool launchPool = boss.GetLaunchVisualSplitFallingMissilePool();
        if (launchPool == null) return;

        Transform firePoint = Manager.GetFirePoint(firePointIndex);
        if (firePoint == null) return;

        // 発射エフェクト
        if (SC_EffectManager.Instance != null)
        {
            Vector3 effectPoint = firePoint.position;
            Quaternion launchRotation = Quaternion.LookRotation(Vector3.up);
            SC_EffectManager.Instance.PlayEffect("LaunchFire", effectPoint, launchRotation);
        }

        GameObject missileObj = launchPool.GetObject(
            firePoint.position,
            firePoint.rotation
        );

        if (missileObj == null) return;

        SC_LaunchVisualMissile missile =
            missileObj.GetComponent<SC_LaunchVisualMissile>();

        if (missile != null)
        {
            missile.SetPool(launchPool);
            missile.OnGetFromPool();
        }
    }

}
