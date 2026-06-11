using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Player/Charge Attack State")]
public class SC_PlayerChargeAttack : SC_PlayerBaseState
{
    [SerializeField] private float chargeSpeed = 5f;
    [SerializeField] private float maxChargeDistance = 5f;

    private Vector3 _startPosition;
    private bool _isCharging;

    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetBool("bCharge", false);
        animator.SetBool("bStraight", true);

        _startPosition = owner.transform.position;
        _isCharging = true;
    }

    public override void UpdateState(GameObject owner, PlayerState stateList)
    {

        var manager = owner.GetComponent<SC_PlayerStateManager>();
        if (!manager)
        {
            _isCharging = false;
            return;
        }

        var attackManager = manager.attackManager;
        if (!attackManager)
        {
            _isCharging = false;
            return;
        }

        var cCon = manager.cController;
        if (!cCon)
        {
            _isCharging = false;
            return;
        }

        if (!_isCharging)
        {
            manager.ChangeState(stateList.Idle);
            return;
        }

        GameObject[] targets = attackManager.GetInAreaObjectByTag("Enemy");
        if (targets != null && targets.Length > 0)
        {
            ChargeAttackExe(owner, stateList);
            _isCharging = false;
            return;
        }

        float step = chargeSpeed * Time.deltaTime;
        cCon.Move(owner.transform.forward * step);

        float traveled = Vector3.Distance(_startPosition, owner.transform.position);
        if (traveled >= maxChargeDistance)
        {
            ChargeAttackExe(owner, stateList);
            _isCharging = false;
        }

    }

    public override void FixedUpdateState(GameObject owner, PlayerState stateList)
    {

    }

    public override void Exit(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetBool("bStraight", false);
    }

    private void ChargeAttackExe(GameObject owner, PlayerState stateList)
    {
        var manager = owner.GetComponent<SC_PlayerStateManager>();
        var attackManager = manager.attackManager;

        // Attack handle
        GameObject[] targets = attackManager.GetInAreaObjectByTag("Enemy");

        if (targets == null || targets.Length == 0) return;

        foreach (var target in targets)
        {
            var Enemy = target.GetComponent<SC_EnemyStatusManager>();

            Debug.Log("Straight Attack");
            Enemy.TakeDamage(attackManager.GetStraightDamage(), owner.transform.position, true, AttackType.Strong);
            break;
        }

        attackManager.ResetCombo();
    }

}
