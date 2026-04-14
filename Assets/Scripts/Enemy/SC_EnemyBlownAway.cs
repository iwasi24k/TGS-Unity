using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/BlownAway State")]
public class SC_EnemyBlownAway : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("吹き飛ばされる力"), SerializeField] private float blownAwayPower = 5f;
    [Tooltip("吹き飛ばされる反作用力"), SerializeField] private float blownAwayReactionPower = 2f;
    [Tooltip("吹き飛ばされる方向"), SerializeField] private Vector3 blownAwayDirection = new Vector3(0, 0, 0);
    [Tooltip("力の減衰速度"), SerializeField] private float decaySpeed = 5f;

    public override void Enter(SC_EnemyStatusManager owner)
    {
        Debug.Log("BlownAway State Enter");
    }

    public override void Exit(SC_EnemyStatusManager owner)
    {
        Debug.Log("BlownAway State Exit");
    }

    public override void UpdateState(SC_EnemyStatusManager owner)
    {
        Debug.Log("BlownAway State Update");

        blownAwayPower -= decaySpeed * Time.deltaTime;

        if (blownAwayPower < 0f)
        {
            blownAwayPower = 0f;
        }

        owner.transform.position += blownAwayDirection * blownAwayPower * Time.deltaTime;

        if (blownAwayPower <= 0f)
        {
            owner.
        }
    }

    // 吹き飛ばされる力を設定するメソッド
    public void SetPower(float power)
    {
        blownAwayPower = power;
    }

    // 吹き飛ばされる方向を設定するメソッド
    public void SetDirection(Vector3 direction)
    {
        blownAwayDirection = direction.normalized;
    }

    // 吹き飛ばされる力と方向を同時に設定するメソッド
    public void SetBlownAway(float power, Vector3 direction)
    {
        blownAwayPower = power;
        blownAwayDirection = direction.normalized;
    }
}
