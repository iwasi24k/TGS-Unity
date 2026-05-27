using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Melee Wave State")]
public class SC_BossMeleeWaveState : SC_EnemyBaceState
{
    [Tooltip("UŒ‚”»’è‚ðo‚·‚Ü‚Å‚ÌŽžŠÔ"), SerializeField] private float attackDelay = 0.5f;
    [Tooltip("UŒ‚State‚ðI—¹‚µ‚ÄIdle‚É–ß‚é‚Ü‚Å‚ÌŽžŠÔ"), SerializeField] private float endDelay = 1.0f;

    private float timer;
    private bool attacked;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Melee Wave State Enter");

        timer = 0f;
        attacked = false;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (!attacked && timer >= attackDelay)
        {
            attacked = true;
            ExecuteMeleeWave(Owner);
        }

        if (timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Melee Wave State Exit");
    }

    private void ExecuteMeleeWave(GameObject Owner)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        GameObject meleeWavePrefab = boss.GetMeleeWavePrefab();
        if (meleeWavePrefab != null)
        {
            Vector3 spawnPos = Owner.transform.position + Vector3.up * 0.1f;

            GameObject effectObj = Instantiate(
                meleeWavePrefab,
                spawnPos,
                Quaternion.identity
            );

            SC_MeleeWave effect =
                effectObj.GetComponent<SC_MeleeWave>();

            if (effect != null)
            {
                effect.SetRadius(boss.GetMeleeWaveRadius());
            }
        }

        //Collider[] hits = Physics.OverlapSphere(
        //    Owner.transform.position,
        //    boss.GetMeleeWaveRadius()
        //);

        //foreach (Collider hit in hits)
        //{
        //    if (!hit.CompareTag("Player")) continue;
        //
        //    Rigidbody playerRb = hit.GetComponent<Rigidbody>();
        //    if (playerRb == null) continue;
        //
        //    Vector3 dir = hit.transform.position - Owner.transform.position;
        //    dir.y = 0f;
        //    dir.Normalize();
        //
        //    //playerRb.linearVelocity = Vector3.zero;
        //    //playerRb.AddForce(dir * boss.GetMeleeKnockBackPower(), ForceMode.Impulse);
        //}
    }
}
