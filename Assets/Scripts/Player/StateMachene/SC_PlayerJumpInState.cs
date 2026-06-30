using UnityEngine;

[CreateAssetMenu(menuName = "Player/JumpIn State")]
public class SC_PlayerJumpInState : SC_PlayerBaseState
{
    private GameObject target;
    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var Manager = owner.GetComponent<SC_PlayerStateManager>();
        var animator = owner.GetComponent<Animator>();

        animator.SetBool("bBoost", true);

        target = Manager.attackManager.GetJumpInTarget();

        if (target == null)
        {
            Manager.ChangeState(stateList.Idle);
        }
    }

    public override void Exit(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();

        animator.SetBool("bBoost", false);
    }

    public override void FixedUpdateState(GameObject owner, PlayerState stateList)
    {
        
    }

    public override void UpdateState(GameObject owner, PlayerState stateList)
    {
        var Manager = owner.GetComponent<SC_PlayerStateManager>();
        var attackManager = Manager.attackManager;

        if (target == null)
        {
            Manager.ChangeState(stateList.Idle);
            return;
        }

        //UŒ‚‰Â”\‘ÎÛ‚ªUŒ‚”ÍˆÍ“à‚Éû‚Ü‚é‚Ü‚ÅˆÚ“®
        var direction = target.transform.position - owner.transform.position;
        
        if(attackManager.CheckObjectInAttackArea("Enemy"))
        {
            // UŒ‚”ÍˆÍ“à‚É“ü‚Á‚½‚çUŒ‚
            attackManager.AttackTransitionCheck(stateList);

        }
        else
        {
            // UŒ‚”ÍˆÍ“à‚É“ü‚é‚Ü‚ÅˆÚ“®
            direction.Normalize();
            Manager.cController.Move(direction * Time.deltaTime * attackManager.GetJumpInSpeed());
        }
    }
}
