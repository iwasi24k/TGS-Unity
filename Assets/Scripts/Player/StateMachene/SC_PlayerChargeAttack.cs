using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Player/Charge Attack State")]
public class SC_PlayerChargeAttack : SC_PlayerBaseState
{
    [SerializeField] private float chargeSpeed = 5f;
    [SerializeField] private float maxChargeDistance = 5f;
    [SerializeField] private float maxChargeTime = 1f;

    private Vector3 _startPosition;
    private bool _isCharging;
    private float _startTime;

    private bool _wasHit;
    public bool GetWasHit() => _wasHit;

    public override void Enter(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetBool("bCharge", false);
        animator.SetBool("bStraight", true);
        SC_SEManager.Instance.PlaySE("charge attack");
        var footPrinter = owner.GetComponent<SC_FootPrinter>();
        if (footPrinter)
        {
            footPrinter.isDrag = true;
        }
        var playerHP = owner.GetComponent<SC_PlayerHP>();
        if (playerHP != null)
        {
            playerHP.SetStar(true);
        }

        _startPosition = owner.transform.position;
        _isCharging = true;
        _wasHit = false;
        _startTime = Time.time;

        var targetSC = owner.GetComponent<SC_PlayerTarget>();
        if (targetSC.GetCurrentTarget())
        {
            owner.transform.LookAt(targetSC.GetCurrentTarget().transform.position);
        }
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
            ChargeAttackExe(owner, stateList, targets, "Enemy");
            _isCharging = false;
            return;
        }

        targets = attackManager.GetInAreaObjectByTag("Door");
        if (targets != null && targets.Length > 0)
        {
            ChargeAttackExe(owner, stateList, targets, "Door");
            _isCharging = false;
            return;
        }

        float step = chargeSpeed * Time.deltaTime;
        cCon.Move(owner.transform.forward * step);

        float traveled = Vector3.Distance(_startPosition, owner.transform.position);
        if (traveled >= maxChargeDistance)
        {
            targets = attackManager.GetInAreaObjectByTag("Enemy");
            ChargeAttackExe(owner, stateList, targets, "Enemy");
            _isCharging = false;
        }

        if(_startTime != -1f && Time.time - _startTime >= maxChargeTime)
        {
            targets = attackManager.GetInAreaObjectByTag("Enemy");
            ChargeAttackExe(owner, stateList, targets, "Enemy");
            _isCharging = false;
        }
    }

    public override void FixedUpdateState(GameObject owner, PlayerState stateList)
    {
        SC_EffectManager.Instance.PlayEffect("DustSmoke", owner.transform.position, owner.transform.rotation);
    }

    public override void Exit(GameObject owner, PlayerState stateList)
    {
        var animator = owner.GetComponent<Animator>();
        animator.SetBool("bStraight", false);
        _wasHit = false;
        _startTime = -1f;

        var footPrinter = owner.GetComponent<SC_FootPrinter>();
        if (footPrinter)
        {
            footPrinter.isDrag = false;
        }

        var playerHP = owner.GetComponent<SC_PlayerHP>();
        if (playerHP != null)
        {
            playerHP.SetStar(false);
        }

    }

    private void ChargeAttackExe(GameObject owner, PlayerState stateList, GameObject[] targets, string tag)
    {
        var manager = owner.GetComponent<SC_PlayerStateManager>();
        var attackManager = manager.attackManager;

        // Attack handle

        if (targets == null || targets.Length == 0) return;

        
        foreach (var target in targets)
        {
            switch(tag)
            {
                case "Enemy":
                    var Enemy = target.GetComponent<SC_EnemyStatusManager>();

                    Debug.Log("Charge Attack");
                    Enemy.TakeDamage(attackManager.GetChargeDamage(), owner.transform.position, true, AttackType.Strong);
                    _wasHit = true;
                    break;

                case "Door":
                    var door = target.GetComponent<SC_Door>();
                    if (door != null)
                    {
                        Debug.Log("Door Attack");
                        door.BlowAwayByPlayer(owner.transform);
                        _wasHit = true;
                    }
                    break;

                default:
                    Debug.LogWarning($"Unknown tag: {tag}");
                    break;
            }

        }

        if(_wasHit)
        {
            SC_SEManager.Instance.PlaySE("blow_2");
        }
        else
        {
            SC_SEManager.Instance.PlaySE("blow_3");
        }


        attackManager.ResetCombo();
    }

}
