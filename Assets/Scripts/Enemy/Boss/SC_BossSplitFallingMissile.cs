using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Split Falling Missile State")]
public class SC_BossSplitFallingMissileState : SC_EnemyBaceState
{
    [Tooltip("攻撃Stateを終了するまでの時間"), SerializeField]
    private float endDelay = 4.0f;

    [Tooltip("ミサイルを落下させる開始高度"), SerializeField]
    private float fallHeight = 10.0f;

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
        fireTimer += Time.deltaTime;

        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        if (firedCount < boss.GetSplitFallingMissileCount())
        {
            if (fireTimer >= boss.GetSplitFallingInterval())
            {
                fireTimer = 0f;
                firedCount++;

                SpawnSplitFallingMissile(Owner, boss);
            }
        }

        if (timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
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
}
