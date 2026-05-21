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

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Homing Missile State Enter");
        timer = 0f;
        fired = false;
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
            Manager.ChangeState(0);
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Homing Missile State Exit");
    }

    private void FireMissiles(GameObject Owner)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;
        if (boss.GetHomingMissilePrefab() == null) return;
        if (boss.GetPlayer() == null) return;

        for (int i = 0; i < boss.GetHomingMissileCount(); i++)
        {
            float angle = 360f / boss.GetHomingMissileCount() * i;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * spawnRadius;

            Vector3 spawnPos = Owner.transform.position + offset + Vector3.up * spawnHeight;

            GameObject missileObj = Instantiate(
                boss.GetHomingMissilePrefab(),
                spawnPos,
                Quaternion.identity
            );

            SC_HomingMissile missile = missileObj.GetComponent<SC_HomingMissile>();
            if (missile != null)
            {
                missile.Init(
                    boss.GetPlayer(),
                    boss.GetHomingMissileSpeed(),
                    boss.GetHomingTime()
                );
            }
        }
    }
}