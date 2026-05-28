using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Machine Gun State")]
public class SC_BossMachineGunState : SC_EnemyBaceState
{
    [Tooltip("攻撃開始までの時間"), SerializeField]
    private float startDelay = 0.3f;

    [Tooltip("ミサイルを生成する高さ"), SerializeField]
    private float spawnHeight = 1.5f;

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

        Vector3 spawnPos = Owner.transform.position;
        spawnPos.y = 0.0f;
        spawnPos += Vector3.up * spawnHeight;

        Vector3 baseDir = Owner.transform.forward;

        Transform player = boss.GetPlayer();

        if (player != null)
        {
            Vector3 toPlayer =
                player.position -
                Owner.transform.position;

            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                baseDir = toPlayer.normalized;
            }
        }

        float randomAngle = Random.Range(
            -boss.GetMachineGunRandomAngle(),
            boss.GetMachineGunRandomAngle()
        );

        Vector3 dir =
            Quaternion.Euler(0f, randomAngle, 0f) *
            baseDir;

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Owner.transform.forward;
        }

        dir.Normalize();

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
}
