using UnityEngine;

[CreateAssetMenu(menuName = "Player/Strong Attack State")]
public class SC_PlayerStrongAttackState : SC_PlayerBaseState
{
    [Header("Strong Attack State")]
    [SerializeField] private float Duration = 0.8f;

    private float timer = 0;
    private bool once = false;

    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetTrigger("tStrongAttack");

        once = false;
    }
    public override void UpdateState(GameObject owner, PlayerState stateList)
    {
        //----------------------------------------------------//
        var Manager = owner.GetComponent<SC_PlayerStateManager>();
        var AttackManager = Manager.attackManager;
        //----------------------------------------------------//

        timer += Time.deltaTime;
        if (timer >= Duration)
        {
            timer = 0;
            Manager.ChangeState(stateList.Idle);
        }

        if (!once)
        {

            GameObject[] gameObjects = AttackManager.GetInAreaObjectByTag("Enemy");

            if (gameObjects == null || gameObjects.Length == 0) return;

            foreach (var gameObject in gameObjects)
            {
                var Enemy = gameObject.GetComponent<SC_EnemyStatusManager>();

                if (!AttackManager.IsNextAttackStrong())
                {
                    Debug.Log("Straight Attack");
                    Enemy.TakeDamage(AttackManager.GetStraightDamage(), owner.transform.position, true, AttackType.Strong);
                    continue;
                }
                switch (AttackManager.GetCurrentComboCount())
                {
                    case 0:
                        Debug.Log("Straight Attack");
                        Enemy.TakeDamage(AttackManager.GetStraightDamage(), owner.transform.position, true, AttackType.Strong);
                        break;
                    case 1:
                        Debug.Log("Rotate Attack");
                        Enemy.TakeDamage(AttackManager.GetRotateDamage(), owner.transform.position, true, AttackType.Rotate);
                        break;
                    case 2:
                        Debug.Log("Uppercut Attack");
                        Enemy.TakeDamage(AttackManager.GetUppercutDamage(), owner.transform.position, true, AttackType.Uppercut);
                        break;
                }
            }

            AttackManager.ResetCombo();
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
