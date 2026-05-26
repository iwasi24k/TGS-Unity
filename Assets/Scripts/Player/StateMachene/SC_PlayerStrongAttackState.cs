using UnityEngine;

[CreateAssetMenu(menuName = "Player/Strong Attack State")]
public class SC_PlayerStrongAttackState : SC_PlayerBaseState
{
    [Header("Strong Attack State")]
    [SerializeField] private float Duration = 0.8f;

    private float timer = 0;

    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetTrigger("tStrongAttack");
    }
    public override void UpdateState(GameObject owner, PlayerState stateList)
    {
        var Manager = owner.GetComponent<SC_PlayerStateManager>();
        timer += Time.deltaTime;
        if (timer >= Duration)
        {
            timer = 0;
            Manager.ChangeState(stateList.Idle);
        }
    }
    public override void FixedUpdateState(GameObject owner, PlayerState stateList)
    {
        
    }
    public override void Exit(GameObject owner, PlayerState stateList)
    {
    }

}
