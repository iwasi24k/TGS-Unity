using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Rapid Missile State")]
public class SC_BossRapidMissileState : SC_EnemyBaceState
{
    [Tooltip("ミサイルを生成するまでの時間"), SerializeField]
    private float fireDelay = 0.5f;

    [Tooltip("攻撃Stateを終了するまでの時間"), SerializeField]
    private float endDelay = 2.0f;

    [Header("Fire Point")]
    [Tooltip("SC_EnemyStatusManager の firePointList の番号")]
    [SerializeField] private int firePointIndex = 13;

    [Header("Missile Motion")]
    [Tooltip("最終的に外側へ広がる半径")]
    [SerializeField] private float spreadRadius = 3.0f;


    private float timer;
    private bool fired;

    private Animator animator;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
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

            FireRapidMissiles(Owner, Manager);

            timer= 0f;
        }

        if (fired && timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        animator.SetBool("tMissileShot", false);
    }

    private void FireRapidMissiles(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        SC_ObjectPool pool = boss.GetRapidMissilePool();
        if (pool == null) return;

        Transform player = boss.GetPlayer();
        if (player == null) return;

        int missileCount = boss.GetRapidMissileCount();
        if (missileCount <= 0) return;

        Transform firePoint = Manager.GetFirePoint(firePointIndex);

        Vector3 spawnPos = Owner.transform.position + Vector3.up * 1.5f;
        Quaternion spawnRot = Quaternion.identity;

        if (firePoint != null)
        {
            spawnPos = firePoint.position;
            spawnRot = firePoint.rotation;
        }
        else
        {
            Debug.LogWarning($"FirePoint が見つかりません。index = {firePointIndex}");
        }

        for (int i = 0; i < missileCount; i++)
        {
            float angle = 360f / missileCount * i;

            Vector3 radialDir =
                Quaternion.Euler(0f, angle, 0f) *
                Vector3.forward;

            Vector3 curveSpreadOffset = radialDir * spreadRadius;

            // 発射エフェクト
            if (SC_EffectManager.Instance != null)
            {
                Vector3 effectPoint = firePoint.position;
                Quaternion launchRotation = Quaternion.LookRotation(Vector3.up);
                SC_EffectManager.Instance.PlayEffect("LaunchFire", effectPoint, launchRotation);
            }

            GameObject missileObj = pool.GetObject(
                spawnPos,
                spawnRot
            );

            if (missileObj == null) continue;

            SC_RapidMissile missile =
                missileObj.GetComponent<SC_RapidMissile>();

            if (missile != null)
            {
                missile.SetPool(pool);
                missile.SetWarningPool(boss.GetWarningCirclePool());
                missile.OnGetFromPool();
                missile.SetUseLockOnMark(i == 0);

                missile.Init(player, 
                    boss.GetRapidMissileSpeed(), 
                    boss.GetRapidMissileStartDelay(), 
                    curveSpreadOffset);
            }
        }
    }

}