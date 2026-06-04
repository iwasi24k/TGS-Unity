using UnityEngine;

[CreateAssetMenu(menuName = "Player/Move State")]
public class SC_PlayerMoveState : SC_PlayerBaseState
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

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
        if (MoveValue == Vector2.zero)
        {
            Manager.ChangeState(stateList.Idle);
            return;
        }

        var SprintIA = Manager.sprintInput;
        var SprintValue = SprintIA.action.ReadValue<float>();
        var SprintSC = owner.GetComponent<SC_PlayerSprintManager>();

        if (SprintIA.action.triggered)
        {
            animator.SetBool("bRun", true);
            SprintSC.TrySprint();
        }
        else
        {
            animator.SetBool("bRun", false);
        }

        var ccon = Manager.cController;

        // カメラのXZ平面を基準に移動方向を計算
        var cam = Camera.main;
        Vector3 move;
        if (cam != null)
        {
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            move = (camRight * MoveValue.x + camForward * MoveValue.y);
            if (move.sqrMagnitude > 1f) move.Normalize();
        }
        else
        {
            // カメラが無ければワールド基準
            move = new Vector3(MoveValue.x, 0, MoveValue.y).normalized;
        }

        // 移動方向へ身体を向ける（スムーズ回転）
        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        float speedMultiplier = (SprintValue > 0.1f ? SprintSC.GetSprintSpeedMultiplier() : 1f);
        ccon.Move(move * moveSpeed * speedMultiplier * Time.deltaTime);
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
