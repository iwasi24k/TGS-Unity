using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/BlownAway State")]
public class SC_EnemyBlownAway : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("吹き飛ばされる力"), SerializeField] private float blownAwayPower = 5.0f;
    [Tooltip("吹き飛ばされる方向"), SerializeField] private Vector3 blownAwayDirection = new Vector3(0, 0, 0);
    [Tooltip("この速度以下で終了"), SerializeField] private float endSpeed = 0.1f;
    [Tooltip("力の減衰速度"), SerializeField] private float decaySpeed = 5f;
    [Tooltip("アッパー時の横方向速度"), SerializeField] private float uppercutHorizontalSpeed = 8.0f;
    [Tooltip("アッパー時の上方向速度"), SerializeField] private float uppercutVerticalSpeed = 10.0f;

    private AttackType receivedAttackType;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.AddCombo();
        }

        Debug.Log("BlownAway State Enter");

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        //HPと吹き飛ばされる力を連動する、HPが高いほど吹き飛ばされる力が弱くなる
        float hpRatio = (float)Manager.GetHP() / Manager.GetMaxHP();

        float adjustedPower = blownAwayPower * (1f - hpRatio);

        Debug.Log($"Enter dir={blownAwayDirection} power={adjustedPower}");

        Vector3 velocity;

        // Uppercut の時だけ Y方向を固定値にする
        if (receivedAttackType == AttackType.Uppercut)
        {
            Debug.Log("Uppercut!!");
            velocity = blownAwayDirection * uppercutHorizontalSpeed;
            velocity.y = uppercutVerticalSpeed;
        }
        else
        {
            velocity = blownAwayDirection.normalized * adjustedPower;
        }

        rb.linearVelocity = velocity;

        Debug.Log($"Enter velocity={rb.linearVelocity}");
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("BlownAway State Exit");

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("BlownAway State Update");

        Manager.CheckCollisionWithOtherEnemies();

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 v = rb.linearVelocity;

        float speed = v.magnitude;
        speed -= decaySpeed * Time.deltaTime;
        if (speed < 0f)
        {
            speed = 0f;
        }

        if (v.sqrMagnitude > 0.0001f)
        {
            rb.linearVelocity = v.normalized * speed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }

        // ほぼ止まったら終了
        if (rb.linearVelocity.magnitude <= endSpeed)
        {
            rb.linearVelocity = Vector3.zero;

            // 状態遷移の処理をここに追加する
            Manager.ReturnFromBlownAway();
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
    public void SetBlownAway(float power, Vector3 direction, AttackType attackType)
    {
        blownAwayPower = power;
        blownAwayDirection = direction.normalized;
        receivedAttackType = attackType;
        Debug.Log("BlownAway Power and Direction Set\n" + "Power: " + blownAwayPower + "Direction: " + blownAwayDirection);
    }

}
