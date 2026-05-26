using UnityEngine;

[CreateAssetMenu(menuName = "Player/WeakAttack State")]
public class SC_PlayerWeakAttackState : SC_PlayerBaseState
{
    [Header("Weak Attack Settings")]
    [SerializeField] private float Duration = 0.5f;

    private float timer = 0;

    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetTrigger("tWeakAttack");
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