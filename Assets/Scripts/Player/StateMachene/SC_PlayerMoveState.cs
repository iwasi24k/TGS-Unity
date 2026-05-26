using UnityEngine;

[CreateAssetMenu(menuName = "Player/Move State")]
public class SC_PlayerMoveState : SC_PlayerBaseState
{
    [SerializeField] private float moveSpeed = 5f;

    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetBool("bWalk", true);
    }
    public override void UpdateState(GameObject owner, PlayerState stateList)
    {
        // Check if the player is still moving, if not, change to idle state
        var Manager = owner.GetComponent<SC_PlayerStateManager>();

        var animator = owner.GetComponent<Animator>();

        var MoveIA = Manager.moveInput;
        var MoveValue = MoveIA.action.ReadValue<Vector2>();
        if(MoveValue == Vector2.zero)
        {
            Manager.ChangeState(stateList.Idle);
            return;
        }

        var SprintIA = Manager.sprintInput;
        var SprintValue = SprintIA.action.ReadValue<float>();
        var SprintSC = owner.GetComponent<SC_PlayerSprintManager>();

        if(SprintIA.action.triggered)
        {
            animator.SetBool("bRun", true);
            SprintSC.TrySprint();
        }
        else 
        {
            animator.SetBool("bRun", false);
        }

        var ccon = Manager.cController;

        Vector3 move = new Vector3(MoveValue.x, 0, MoveValue.y).normalized;
        ccon.Move(move * moveSpeed * (SprintValue > 0.1f ? SprintSC.GetSprintSpeedMultiplier() : 1f) * Time.deltaTime);
    }
    public override void FixedUpdateState(GameObject owner, PlayerState stateList)
    {

    }
    public override void Exit(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetBool("bRun", false);
        animator.SetBool("bWalk", false);
    }
}
