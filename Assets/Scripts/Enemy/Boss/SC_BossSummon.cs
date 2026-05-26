using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Summon State")]
public class SC_BossSummonState : SC_EnemyBaceState
{
    [Tooltip("ŽG‹›“G‚ð¢Š«‚·‚é‚Ü‚Å‚ÌŽžŠÔ"), SerializeField] private float summonDelay = 0.5f;
    [Tooltip("UŒ‚State‚ðI—¹‚µ‚ÄIdle‚É–ß‚é‚Ü‚Å‚ÌŽžŠÔ"), SerializeField] private float endDelay = 1.5f;

    private float timer;
    private bool summoned;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Summon State Enter");

        timer = 0f;
        summoned = false;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (!summoned && timer >= summonDelay)
        {
            summoned = true;
            SummonEnemies(Owner);
        }

        if (timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Summon State Exit");
    }

    private void SummonEnemies(GameObject Owner)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;
        if (boss.GetSmallEnemyPrefab() == null) return;

        for (int i = 0; i < boss.GetSummonCount(); i++)
        {
            float angle = 360f / boss.GetSummonCount() * i;

            Vector3 offset = Quaternion.Euler(0, angle, 0)
                * Vector3.forward
                * boss.GetSummonRadius();

            Vector3 spawnPos = Owner.transform.position + offset;

            spawnPos.y = 0f;

            GameObject enemyObj = Instantiate(
                boss.GetSmallEnemyPrefab(),
                spawnPos,
                Quaternion.identity
            );

            SC_EnemyStatusManager enemyStatus =
                enemyObj.GetComponent<SC_EnemyStatusManager>();

            if (enemyStatus != null)
            {
                enemyStatus.SetHP(5);
            }
        }
    }
}