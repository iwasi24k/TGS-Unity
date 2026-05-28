using UnityEngine;

[CreateAssetMenu(menuName = "Player/WeakAttack State")]
public class SC_PlayerWeakAttackState : SC_PlayerBaseState
{
    [Header("Weak Attack Settings")]
    [SerializeField] private float Duration = 0.5f;

    private float timer = 0;

    private bool once = false;

    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var Animator = owner.GetComponent<Animator>();
        Animator.SetTrigger("tWeakAttack");
        once = false;
    }
    public override void UpdateState(GameObject owner, PlayerState stateList)
    {
        //----------------------------------------------------//
        var Manager = owner.GetComponent<SC_PlayerStateManager>();
        var AttackManager = Manager.attackManager;
        //----------------------------------------------------//

        if (AttackManager == null)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= Duration)
        {
            timer = 0;
            Manager.ChangeState(stateList.Idle);
        }


        if (!once)
        {

            GameObject[] targets;

            // AttackArea
            targets = AttackManager.GetInAreaObjectByTag("Enemy");
            if (targets == null || targets.Length == 0) return;

            bool isHit = false;

            foreach (var target in targets)
            {
                if (target == null) continue; // ここが重要：null 要素をスキップ

                var Enemy = target.GetComponent<SC_EnemyStatusManager>();
                if (Enemy == null) continue;

                // ダメージ処理など
                Enemy.TakeDamage(AttackManager.GetWeakDamage(), owner.transform.position, false, AttackType.Weak1);
                isHit = true;
            }

            if (isHit)
            {
                AttackManager.IncrementCombo();
            }
            
            once = true;
        }
    }
    public override void FixedUpdateState(GameObject owner, PlayerState stateList)
    {

    }
    public override void Exit(GameObject owner, PlayerState stateList)
    {

    }
}