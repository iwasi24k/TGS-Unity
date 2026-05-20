using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(menuName = "Enemy/BlownAway State")]
public class SC_EnemyBlownAway : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("吹き飛ばされる力"), SerializeField] private float blownAwayPower = 5.0f;
    [Tooltip("吹き飛ばされる方向"), SerializeField] private Vector3 blownAwayDirection = new Vector3(0, 0, 0);
    [Tooltip("この速度以下で終了"), SerializeField] private float endSpeed = 0.1f;
    [Tooltip("力の減衰速度"), SerializeField] private float decaySpeed = 5f;

    [Header("Uppercut Blow Away")]
    [Tooltip("アッパー時の横方向速度"), SerializeField] private float uppercutHorizontalSpeed = 8.0f;
    [Tooltip("アッパー時の上方向速度"), SerializeField] private float uppercutVerticalSpeed = 10.0f;

    [Header("Rotate Blow Away")]
    [Tooltip("回転吹っ飛びの初速"), SerializeField] private float rotateInitialSpeed = 8.0f;
    [Tooltip("回転吹っ飛びの右方向成分"), SerializeField] private float rotateRightRate = 0.5f;
    [Tooltip("左方向へ曲げる加速度"), SerializeField] private float rotateLeftAcceleration = 15.0f;
    [Tooltip("回転中の自転速度"), SerializeField] private float rotateSelfAngularSpeed = 10.0f;
    [Tooltip("回転の減衰速度"), SerializeField] private float rotateDecaySpeed = 25.0f;

    [Header("Bounce Settings")]
    [Tooltip("壁反射時の速度倍率"), SerializeField] private float wallBounceMultiplier = 1.3f;
    [Tooltip("反射後の最低速度"), SerializeField] private float minBounceSpeed = 6.0f;
    [Tooltip("反射後の最大速度"), SerializeField] private float maxBounceSpeed = 15.0f;
    [Tooltip("壁反射の連続発生防止時間"),SerializeField] private float wallBounceCooldown = 0.1f;

    private bool isRotateMove = false;
    private Vector3 rotateLeftDirection;
    private float rotateCurrentPower = 0f;
    private float wallBounceCooldownTimer = 0f;

    private Animator animator;

    private AttackType receivedAttackType;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.AddCombo();
        }

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        animator = Owner.GetComponent<Animator>();
        animator.SetBool("bHit", true);

        //HPと吹き飛ばされる力を連動する、HPが高いほど吹き飛ばされる力が弱くなる
        float hpRatio = (float)Manager.GetHP() / Manager.GetMaxHP();

        float adjustedPower = blownAwayPower * (1f - hpRatio);

        Vector3 velocity;

        // Uppercut の時だけ Y方向を固定値にする
        if (receivedAttackType == AttackType.Uppercut)
        {
            Debug.Log("Uppercut!!");
            velocity = blownAwayDirection * uppercutHorizontalSpeed;
            velocity.y = uppercutVerticalSpeed;
        }
        else if (receivedAttackType == AttackType.Rotate)
        {
            Debug.Log("Rotate!!");
            StartRotateMove(rb, blownAwayDirection);
            return;
        }
        else
        {
            velocity = blownAwayDirection.normalized * adjustedPower;
        }

        rb.linearVelocity = velocity;
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;

        animator.SetBool("bHit", false);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Manager.CheckCollisionWithOtherEnemies();

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 v = rb.linearVelocity;

        if (isRotateMove)
        {
            return;
        }

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
        //Debug.Log("BlownAway Power and Direction Set\n" + "Power: " + blownAwayPower + "Direction: " + blownAwayDirection);
    }

    // 回転移動を開始する
    private void StartRotateMove(Rigidbody rigidbody, Vector3 forwardDirection)
    {
        Rigidbody rb = rigidbody;
        isRotateMove = true;

        rotateCurrentPower = rotateInitialSpeed;

        forwardDirection.y = 0f;

        if (forwardDirection.sqrMagnitude <= 0.0001f)
        {
            forwardDirection = Vector3.forward;
        }

        forwardDirection.Normalize();

        // forwardDirection から見た右方向
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 左方向加速度用
        rotateLeftDirection = -rightDirection;

        // 少し右前に飛ばす
        Vector3 initialDirection = forwardDirection + rightDirection * rotateRightRate;
        initialDirection.y = 0f;
        initialDirection.Normalize();

        // 初速だけ設定する
        rb.linearVelocity = initialDirection * rotateInitialSpeed;
        // 敵自身を回転させる
        rb.angularVelocity = Vector3.up * rotateSelfAngularSpeed;
    }

    // 回転移動を終了する
    private void EndRotateMove(GameObject Owner)
    {
        Debug.Log("End Rotate Move");

        Rigidbody rb = Owner.GetComponent<Rigidbody>();

        isRotateMove = false;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public override void FixedUpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Rigidbody rb = Owner.GetComponent<Rigidbody>();

        if (wallBounceCooldownTimer > 0f)
        {
            wallBounceCooldownTimer -= Time.fixedDeltaTime;
        }

        if (!isRotateMove)
        {
            return;
        }

        if (rb == null)
        {
            isRotateMove = false;
            return;
        }

        float dt = Time.fixedDeltaTime;

        // 現在の速度
        Vector3 velocity = rb.linearVelocity;

        // 横方向速度だけを見る
        Vector3 horizontalVelocity = velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        // 速度がある程度ある時だけ左へ曲げる
        if (speed > endSpeed)
        {
            float powerRate = Mathf.Clamp01(speed / rotateInitialSpeed);

            rb.AddForce(rotateLeftDirection * rotateLeftAcceleration * powerRate, ForceMode.Acceleration);
        }

        // 速度を徐々に減衰させる
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, rotateDecaySpeed * dt);

        // 自転も減衰
        rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, Vector3.zero, rotateDecaySpeed * dt);

        // 止まったら終了
        if (rb.linearVelocity.magnitude <= endSpeed)
        {
            EndRotateMove(Owner);
            Manager.ReturnFromBlownAway();
        }
    }

    public override void OnCollisionEnterState(GameObject Owner, SC_EnemyStatusManager Manager, Collision collision)
    {
        if (!collision.gameObject.CompareTag("Wall"))
        {
            return;
        }

        if (wallBounceCooldownTimer > 0f)
        {
            return;
        }

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        if (collision.contactCount <= 0)
        {
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        Debug.Log($"Collision with Wall detected. Current velocity: {velocity}");

        if (velocity.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        if(isRotateMove)
        {
            isRotateMove = false;
             Debug.Log("Rotate Move Canceled by Collision");
        }

        Vector3 normal = collision.contacts[0].normal;

        Vector3 reflectedVelocity = Vector3.Reflect(velocity, normal);

        reflectedVelocity *= wallBounceMultiplier;

        if (reflectedVelocity.magnitude > maxBounceSpeed)
        {
            Debug.Log($"Bounce speed capped from {reflectedVelocity.magnitude} to {maxBounceSpeed}");
            reflectedVelocity = reflectedVelocity.normalized * maxBounceSpeed;
        }

        // 反射後の最低速度を保証
        if (reflectedVelocity.magnitude < minBounceSpeed)
        {
            reflectedVelocity = reflectedVelocity.normalized * minBounceSpeed;
        }

        rb.linearVelocity = reflectedVelocity;

        wallBounceCooldownTimer = wallBounceCooldown;

        Debug.Log($"Applying bounce velocity: {rb.linearVelocity}");
    }


}
