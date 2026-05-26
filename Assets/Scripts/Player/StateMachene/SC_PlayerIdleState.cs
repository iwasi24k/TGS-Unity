using UnityEngine;

[CreateAssetMenu(menuName = "Player/Idle State")]
public class SC_PlayerIdleState : SC_PlayerBaseState
{
    public override void Enter(GameObject owner, PlayerState stateList)
    {
        
    }

    public override void UpdateState(GameObject owner, PlayerState stateList)
    {
        var Manager = owner.GetComponent<SC_PlayerStateManager>();

        // State transition check
        var walkIA = Manager.moveInput;
        var walkValue = walkIA.action.ReadValue<Vector2>();

        if (walkValue.magnitude > 0.1f)
        {
            Manager.ChangeState(stateList.Move);
            return;
        }


        var attackIA = Manager.weakAttackInput;
        var attackValue = attackIA.action.ReadValue<float>();

        if (attackValue > 0.1f)
        {
            Manager.ChangeState(stateList.WeakAttack);
            return;
        }


        var strongAttackIA = Manager.strongAttackInput;
        var strongAttackValue = strongAttackIA.action.ReadValue<float>();

        if (strongAttackValue > 0.1f)
        {
            Manager.ChangeState(stateList.StrongAttack);
            return;
        }
    }

    public override void FixedUpdateState(GameObject owner, PlayerState stateList)
    {

    }

    public override void Exit(GameObject owner, PlayerState stateList)
    {

    }
}
