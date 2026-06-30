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
        var attackManager = owner.GetComponent<SC_PlayerAttackManager>();
        animator.SetTrigger("tStraight");
        once = false;
        Debug.Log("Enter Strong Attack State");
    }
    public override void UpdateState(GameObject owner, PlayerState stateList)
    {
        //----------------------------------------------------//
        var Manager = owner.GetComponent<SC_PlayerStateManager>();
        var AttackManager = Manager.attackManager;
        var animator = owner.GetComponent<Animator>();
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
            {
                if (gameObjects != null && gameObjects.Length != 0)
                {
                    foreach (var gameObject in gameObjects)
                    {
                        var Enemy = gameObject.GetComponent<SC_EnemyStatusManager>();

                        if(!Enemy)
                        {
                            continue;
                        }

                        Debug.Log("Straight Attack");
                        Enemy.TakeDamage(AttackManager.GetStraightDamage(), owner.transform.position, true, AttackType.Strong);
                    }
                }
            }

            gameObjects = AttackManager.GetInAreaObjectByTag("Bullet");
            {
                if (gameObjects != null && gameObjects.Length != 0)
                {
                    foreach (var gameObject in gameObjects)
                    {
                        var Missile = gameObject.GetComponent<SC_ReflectableMissile>();
                        
                        if (Missile == null)
                        {
                            continue;
                        }

                        Debug.Log("Straight Attack");
                        Missile.ReflectByPlayer(owner.transform);
                    }
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
