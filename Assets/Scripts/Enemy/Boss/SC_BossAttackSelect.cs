using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Attack Select State")]
public class SC_BossAttackSelectState : SC_EnemyBaceState
{
    [Tooltip("‹ßÚ”g“®UŒ‚State‚ÌStateList”Ô†"), SerializeField] private int meleeStateIndex = 2;
    [Tooltip("’Ç]Œ^ƒ~ƒTƒCƒ‹UŒ‚State‚ÌStateList”Ô†"), SerializeField] private int homingMissileStateIndex = 3;
    [Tooltip("—‰ºŒ^ƒ~ƒTƒCƒ‹UŒ‚State‚ÌStateList”Ô†"), SerializeField] private int fallingMissileStateIndex = 4;
    [Tooltip("G‹›“G¢Š«State‚ÌStateList”Ô†"), SerializeField] private int summonStateIndex = 5;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Attack Select State Enter");

        int random = Random.Range(0, 2);

        switch (random)
        {
            case 0:
                Manager.ChangeState(meleeStateIndex);
                break;

            case 1:
                Manager.ChangeState(homingMissileStateIndex);
                break;

            case 2:
                Manager.ChangeState(fallingMissileStateIndex);
                break;

            case 3:
                Manager.ChangeState(summonStateIndex);
                break;
        }
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Attack Select State Exit");
    }
}