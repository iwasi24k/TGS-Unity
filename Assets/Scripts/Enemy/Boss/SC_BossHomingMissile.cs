using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Homing Missile State")]
public class SC_BossHomingMissileState : SC_EnemyBaceState
{
    [Tooltip("ƒ~ƒTƒCƒ‹‚ğ¶¬‚·‚é‚Ü‚Å‚ÌŠÔ"), SerializeField]
    private float fireDelay = 0.5f;

    [Tooltip("UŒ‚State‚ğI—¹‚·‚é‚Ü‚Å‚ÌŠÔ"), SerializeField]
    private float endDelay = 2.0f;

    [Header("Fire Point")]
    [Tooltip("SC_EnemyStatusManager ‚Ì firePointList ‚Ì”Ô†")]
    [SerializeField] private int firePointIndex = 13;

    [Header("Missile Motion")]
    [Tooltip("P‚Ìœ‚Ì‚æ‚¤‚ÉL‚ª‚éŠÔ")]
    [SerializeField] private float curveTime = 0.8f;

    [Tooltip("Å‰‚É‚Ç‚ê‚­‚ç‚¢ã‚Ö‚¿ã‚°‚é‚©")]
    [SerializeField] private float curveUpHeight = 5.0f;

    [Tooltip("ÅI“I‚ÉŠO‘¤‚ÖL‚ª‚é”¼Œa")]
    [SerializeField] private float spreadRadius = 3.0f;

    [Tooltip("‹Èü’†‚Ì‰ñ“]‘¬“x")]
    [SerializeField] private float rotateSpeed = 720.0f;

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

            FireMissiles(Owner, Manager);

            timer = 0f;
        }

        if (fired && timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Homing Missile State Exit");

        animator.SetBool("tMissileShot", false);
    }

    private void FireMissiles(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null) return;

        SC_ObjectPool pool = boss.GetHomingMissilePool();
        if (pool == null) return;

        Transform player = boss.GetPlayer();
        if (player == null) return;

        int missileCount = boss.GetHomingMissileCount();
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
            Debug.LogWarning($"FirePoint ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñBindex = {firePointIndex}");
        }

        for (int i = 0; i < missileCount; i++)
        {
            float angle = 360f / missileCount * i;

            Vector3 radialDir =
                Quaternion.Euler(0f, angle, 0f) *
                Vector3.forward;

            Vector3 curveControlOffset =
                Vector3.up * curveUpHeight;

            Vector3 curveEndOffset =
                Vector3.up * curveUpHeight +
                radialDir * spreadRadius;

            GameObject missileObj = pool.GetObject(
                spawnPos,
                spawnRot
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
                    boss.GetHomingTime(),   
                    curveTime,   
                    curveControlOffset,    
                    curveEndOffset,
                    rotateSpeed);

            }
        }
    }
}