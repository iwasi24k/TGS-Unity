using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/BlownAway State")]
public class SC_EnemyBlownAway : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("吹き飛ばされる力"), SerializeField] private float blownAwayPower = 5f;
    [Tooltip("吹き飛ばされる方向"), SerializeField] private Vector3 blownAwayDirection = new Vector3(0, 0, 0);
    [Tooltip("この速度以下で終了"), SerializeField] private float endSpeed = 0.1f;
    [Tooltip("力の減衰速度"), SerializeField] private float decaySpeed = 5f;
    [Tooltip("跳ね返りで残る力の割合"), SerializeField] private float blownAwayReactionPower = 1.0f;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("BlownAway State Enter");

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.linearVelocity = blownAwayDirection.normalized * blownAwayPower;
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

    // 跳ね返す関数
    public void Bounce(GameObject Owner, Vector3 hitNormal)
    {
        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 velocity = rb.linearVelocity;

        // XZ平面だけ使う
        velocity.y = 0f;
        hitNormal.y = 0f;

        if (velocity.sqrMagnitude <= 0.0001f) return;
        if (hitNormal.sqrMagnitude <= 0.0001f) return;

        velocity.Normalize();
        hitNormal.Normalize();

        // XZ平面で反射
        Vector3 reflectDir = Vector3.Reflect(velocity, hitNormal).normalized;
        reflectDir.y = 0f;

        // 元のXZ速度を取得
        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0f;
        float currentSpeed = currentVelocity.magnitude;

        // 跳ね返り後の速度
        float newSpeed = currentSpeed * Mathf.Clamp01(blownAwayReactionPower);

        rb.linearVelocity = new Vector3(reflectDir.x * newSpeed,0f,reflectDir.z * newSpeed);

        blownAwayDirection = new Vector3(reflectDir.x, 0f, reflectDir.z);
        blownAwayPower = newSpeed;

        Debug.Log("BlownAway Bounce");
    }
}
