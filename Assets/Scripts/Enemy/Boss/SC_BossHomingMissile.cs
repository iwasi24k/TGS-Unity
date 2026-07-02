using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Homing Missile State")]
public class SC_BossHomingMissileState : SC_EnemyBaceState
{
    [Tooltip("ミサイルを発射するまでの時間"), SerializeField] private float fireDelay = 0.5f;
    [Tooltip("攻撃Stateを終了してIdleに戻るまでの時間"), SerializeField] private float endDelay = 2.0f;
    [Tooltip("ミサイルを生成する高さ"), SerializeField] private float spawnHeight = 1.5f;
    [Tooltip("ボス中心からどれくらい離してミサイルを生成するか"), SerializeField] private float spawnRadius = 1.0f;

    private float timer;
    private bool fired;

    private Animator animator;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Homing Missile State Enter");
        timer = 0f;
        fired = false;

        animator = Owner.GetComponentInChildren<Animator>();
        animator.SetBool("tMissileShot", true);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (!fired && timer >= fireDelay)
        {
            fired = true;
            FireMissiles(Owner);
        }

        if (timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Homing Missile State Exit");

        animator.SetBool("tMissileShot", false);
    }

    private void FireMissiles(GameObject Owner)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        SC_ObjectPool pool = boss.GetHomingMissilePool();
        if (pool == null) return;

        Transform player = boss.GetPlayer();
        if (player == null) return;

        int missileCount = boss.GetHomingMissileCount();
        if (missileCount <= 0) return;

        for (int i = 0; i < missileCount; i++)
        {
            float angle = 360f / missileCount * i;

            Vector3 offset =
                Quaternion.Euler(0f, angle, 0f) *
                Vector3.forward *
                spawnRadius;

            Vector3 spawnPos =
                Owner.transform.position +
                offset +
                Vector3.up * spawnHeight;

            GameObject missileObj = pool.GetObject(
                spawnPos,
                Quaternion.identity
            );

            if (missileObj == null) continue;

            SC_HomingMissile missile =
                missileObj.GetComponent<SC_HomingMissile>();

            if (missile != null)
            {
                missile.SetPool(pool);
                missile.SetWarningPool(boss.GetWarningCirclePool());
                missile.OnGetFromPool();

                missile.SetUseLockOnMark(i == 0);

                missile.Init(
                    player,
                    boss.GetHomingMissileSpeed(),
                    boss.GetHomingTime()
                );
            }
        }
    }
}