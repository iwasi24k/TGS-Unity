using UnityEngine;


[CreateAssetMenu(menuName = "Enemy/BlownAway State")]
public class SC_EnemyBlownAway : SC_EnemyBaceState
{
    [Header("Settings")]
    [Tooltip("吹き飛ばされる力基数"), SerializeField] private float blownAwayPowerBase = 1.0f;
    [Tooltip("受け取った吹き飛ばし力に掛ける倍率"), SerializeField] private float blownAwayPowerMultiplier = 5.0f;
    [Tooltip("吹き飛ばされる方向"), SerializeField] private Vector3 blownAwayDirection = new Vector3(0, 0, 0);
    [Tooltip("この速度以下で終了"), SerializeField] private float endSpeed = 0.1f;
    [Tooltip("力の減衰速度"), SerializeField] private float decaySpeed = 5.0f;
    [Tooltip("最低でもBlownAway状態を維持する時間"), SerializeField] private float minBlownAwayTime = 0.3f;
    [Tooltip("BlownAway状態を維持できる最大時間"), SerializeField] private float maxBlownAwayTime = 5.0f;

    [Header("Bounce Settings")]
    [Tooltip("壁反射時の速度倍率"), SerializeField] private float wallBounceMultiplier = 1.3f;
    [Tooltip("反射後の最低速度"), SerializeField] private float minBounceSpeed = 6.0f;
    [Tooltip("反射後の最大速度"), SerializeField] private float maxBounceSpeed = 15.0f;
    [Tooltip("壁反射の連続発生防止時間"),SerializeField] private float wallBounceCooldown = 0.1f;

    [Header("EffectOption")]
    [Tooltip("エフェクトのキー"), SerializeField] private string effectKey;
    [Tooltip("再生する距離間隔"), SerializeField] private float effectInterval = 1.0f;

    private float blownAwayPower = 0f;
    private bool isRotateMove = false;
    private float wallBounceCooldownTimer = 0f;
    private float timer;

    private Animator animator;

    private AttackType receivedAttackType;
    private float effectAccumulator = 0f;
    private Vector3 effectLastPos = Vector3.zero;


    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer = 0f;
        wallBounceCooldownTimer = 0f;

        effectAccumulator = 0f;
        effectLastPos = Owner.transform.position;

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.AddCombo();
        }

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        animator = Owner.GetComponentInChildren<Animator>();
        animator.SetBool("bBlownAway", true);

        //HPと吹き飛ばされる力を連動する、HPが高いほど吹き飛ばされる力が弱くなる
        float hpRatio = (float)Manager.GetHP() / Manager.GetMaxHP();

        //ダメージ計算あり
        //float adjustedPower = blownAwayPowerBase + blownAwayPower * (1f - hpRatio) * blownAwayPowerMultiplier;

        //ダメージ計算なし
        float adjustedPower = blownAwayPowerBase + (1f - hpRatio) * blownAwayPowerMultiplier;


        Vector3 velocity;

        receivedAttackType = Manager.GetStunAttackType();

        if(receivedAttackType == AttackType.Door)
        {
            velocity = blownAwayDirection.normalized * blownAwayPowerMultiplier;
        }
        else
        {
            velocity = blownAwayDirection.normalized * adjustedPower;
        }

        rb.linearVelocity = velocity;

        Manager.SetBlowAwayFollowPartsEnabled(true);
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;

        animator.SetBool("bBlownAway", false);

        Manager.SetBlowAwayFollowPartsEnabled(false);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Manager.CheckCollisionWithOtherEnemies();

        Rigidbody rb = Owner.GetComponent<Rigidbody>();
        if (rb == null) return;

        // BlownAway中は常に時間を進める
        timer += Time.deltaTime;

        // 最大時間を超えたら強制終了
        if (timer >= maxBlownAwayTime)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            isRotateMove = false;
            Manager.ReturnFromBlownAway();
            return;
        }

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

        if (timer < minBlownAwayTime)
        {
            return;
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
    public void SetBlownAway(float power, Vector3 direction)
    {
        blownAwayPower = power;
        blownAwayDirection = direction.normalized;
        //Debug.Log("BlownAway Power and Direction Set\n" + "Power: " + blownAwayPower + "Direction: " + blownAwayDirection);
    }

    public override void FixedUpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Rigidbody rb = Owner.GetComponent<Rigidbody>();

        if (wallBounceCooldownTimer > 0f)
        {
            wallBounceCooldownTimer -= Time.fixedDeltaTime;
        }

        if(effectInterval > 0f && SC_EffectManager.Instance != null)
        {
            Vector3 currentPos = Owner.transform.position;
            float delta = Vector3.Distance(currentPos, effectLastPos);
            effectAccumulator += delta;
            effectLastPos = currentPos;

            if (effectAccumulator >= effectInterval)
            {
                // 現在位置でエフェクトを再生
                SC_EffectManager.Instance.PlayEffect(effectKey, currentPos);
                effectAccumulator = 0f;
            }
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

        // 止まったら終了
        if (rb.linearVelocity.magnitude <= endSpeed)
        {
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

        // 壁との接触位置でSEを再生
        Vector3 hitPosition = collision.GetContact(0).point;
        Manager.PlayRandomEnemyCollisionSE(hitPosition);

        if (isRotateMove)
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
