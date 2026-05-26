using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Falling Missile State")]
public class SC_BossFallingMissileState : SC_EnemyBaceState
{
    [Tooltip("攻撃Stateを終了してIdleに戻るまでの時間"), SerializeField] private float endDelay = 2.0f;
    [Tooltip("ミサイルを落下させる開始高度"), SerializeField] private float fallHeight = 10.0f;

    private float timer;
    private float fireTimer;
    private int firedCount;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Falling Missile State Enter");
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

        if (firedCount < boss.GetFallingMissileCount())
        {
            if (fireTimer >= boss.GetFallingInterval())
            {
                fireTimer = 0f;
                firedCount++;

                SpawnFallingMissile(Owner, boss);
            }
        }

        if (timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Falling Missile State Exit");
    }

    private void SpawnFallingMissile(GameObject Owner, SC_BossAttackController boss)
    {
        if (boss.GetFallingMissilePrefab() == null) return;

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

        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;

        Vector3 targetPos = Owner.transform.position + offset;
        targetPos.y = Owner.transform.position.y;

        targetPos.y = 0.0f;

        GameObject missileObj = Instantiate(
            boss.GetFallingMissilePrefab(),
            targetPos + Vector3.up * fallHeight,
            Quaternion.identity
        );

        SC_FallingMissile missile = missileObj.GetComponent<SC_FallingMissile>();
        if (missile != null)
        {
            missile.Init(
                targetPos,
                fallHeight,
                boss.GetFallingTime(),
                boss.GetWarningMarkPrefab()
            );
        }
    }
}
