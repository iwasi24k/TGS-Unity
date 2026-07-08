using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Falling Missile State")]
public class SC_BossFallingMissileState : SC_EnemyBaceState
{

    [Tooltip("攻撃開始までの待ち時間"), SerializeField] 
    private float startDelay = 0.5f;

    [Tooltip("攻撃Stateを終了してIdleに戻るまでの時間"), SerializeField] 
    private float endDelay = 2.0f;

    [Tooltip("ミサイルを落下させる開始高度"), SerializeField] 
    private float fallHeight = 10.0f;

    private float timer;
    private float fireTimer;
    private float endTimer;
    private int firedCount;
    private bool isFireFinished;

    private Animator animator;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Falling Missile State Enter");

        timer = 0f;
        fireTimer = 0f;
        endTimer = 0f;
        firedCount = 0;
        isFireFinished = false;

        animator = Owner.GetComponentInChildren<Animator>();
        animator.SetBool("tMissileShot", true);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        // まだ発射が終わっていない
        if (!isFireFinished)
        {
            // 攻撃開始待ち
            if (timer < startDelay) return;

            fireTimer += Time.deltaTime;

            if (firedCount < boss.GetFallingMissileCount())
            {
                if (fireTimer >= boss.GetFallingInterval())
                {
                    fireTimer = 0f;
                    firedCount++;

                    SpawnFallingMissile(Owner, boss);
                }
            }

            // 全弾発射完了
            if (firedCount >= boss.GetFallingMissileCount())
            {
                isFireFinished = true;
                endTimer = 0f;
            }

            return;
        }

        // 全弾発射後から endDelay を数える
        endTimer += Time.deltaTime;

        if (endTimer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Falling Missile State Exit");

        animator.SetBool("tMissileShot", false);
    }

    private void SpawnFallingMissile(GameObject Owner, SC_BossAttackController boss)
    {
        SC_ObjectPool fallingPool = boss.GetFallingMissilePool();
        if (fallingPool == null) return;

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

        targetPos.y = 0.0f;

        Vector3 spawnPos =
            targetPos +
            Vector3.up * fallHeight;

        GameObject missileObj = fallingPool.GetObject(
            spawnPos,
            Quaternion.identity
        );

        if (missileObj == null) return;

        SC_FallingMissile missile =
            missileObj.GetComponent<SC_FallingMissile>();

        Debug.Log($"Spawn Falling Missile at {targetPos}, spawnPos: {spawnPos}");

        if (missile != null)
        {
            missile.SetPool(fallingPool);
            missile.SetWarningPool(boss.GetWarningCirclePool());
            missile.OnGetFromPool();

            missile.Init(
                targetPos,
                fallHeight,
                boss.GetFallingTime()
            );
        }
    }
}
