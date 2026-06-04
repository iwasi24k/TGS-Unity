using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Rapid Missile State")]
public class SC_BossRapidMissileState : SC_EnemyBaceState
{
    [Tooltip("ミサイルを生成するまでの時間"), SerializeField]
    private float fireDelay = 0.5f;

    [Tooltip("攻撃Stateを終了するまでの時間"), SerializeField]
    private float endDelay = 2.0f;

    [Tooltip("ミサイルを生成する高さ"), SerializeField]
    private float spawnHeight = 1.5f;

    [Tooltip("ボス中心からどれくらい離して生成するか"), SerializeField]
    private float spawnRadius = 1.0f;

    private float timer;
    private bool fired;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer = 0f;
        fired = false;

        Animator animator = Owner.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("tRapidMissile");
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (!fired && timer >= fireDelay)
        {
            fired = true;
            FireRapidMissiles(Owner);
        }

        if (timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
    }

    private void FireRapidMissiles(GameObject Owner)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        SC_ObjectPool pool = boss.GetRapidMissilePool();
        if (pool == null) return;

        Transform player = boss.GetPlayer();
        if (player == null) return;

        int missileCount = boss.GetRapidMissileCount();
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

            SC_RapidMissile missile =
                missileObj.GetComponent<SC_RapidMissile>();

            if (missile != null)
            {
                missile.SetPool(pool);
                missile.OnGetFromPool();

                missile.Init(
                    player,
                    boss.GetRapidMissileSpeed(),
                    boss.GetRapidMissileStartDelay()
                );
            }
        }
    }
}