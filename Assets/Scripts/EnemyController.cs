using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    enum State
    {
        Shooting,
        Moving,
        Waiting,
        Dying,
        Dead
    }

    private State state;

    [Header("ターゲット")]
    private Transform player;

    [Header("弾")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float shotInterval = 1.0f;
    public int maxShotCount = 5;
    public Vector3 shotOffset = Vector3.zero;

    [Header("移動")]
    public float moveSpeed = 2f;
    public float moveDuration = 1.5f;
    public float waitTime = 1.0f;
    public float rotationSpeed = 5f;

    private Vector3 moveDir;

    [Header("HP")]
    public float maxHP = 100f;
    private float currentHP;
    public Slider hpSlider;
    public bool IsAlive => currentHP > 0;

    [Header("エフェクト")]
    public GameObject hitEffect;
    public GameObject lowHpEffect;
    public GameObject deathEffect;

    private bool isLowHpEffectPlayed = false;

    [Header("吹き飛び")]
    public float knockbackForce = 10f;
    public float searchRadius = 10f;

    [Header("Animator")]
    public Animator animator;

    // ===== ステート制御 =====
    private float timer;
    private int shotCount;
    private int shotFired;

    // ===== ノックバック =====
    private bool isKnockback = false;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0.3f;
    private Vector3 knockbackDir;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        currentHP = maxHP;
        UpdateHPBar();

        animator = GetComponent<Animator>();

        ChangeState(State.Shooting);
    }

    void Update()
    {
        // ★ノックバック最優先
        if (isKnockback)
        {
            UpdateKnockback();
            return;
        }

        if (state == State.Dead) return;

        if (state == State.Dying)
        {
            UpdateDying();
            return;
        }

        LookAtPlayer();

        switch (state)
        {
            case State.Shooting:
                UpdateShooting();
                break;

            case State.Moving:
                UpdateMoving();
                break;

            case State.Waiting:
                UpdateWaiting();
                break;
        }
    }

    // =========================
    // ステート制御
    // =========================

    void ChangeState(State newState)
    {
        state = newState;
        timer = 0f;

        switch (state)
        {
            case State.Shooting:
                shotCount = Random.Range(1, maxShotCount + 1);
                shotFired = 0;
                break;

            case State.Moving:
                moveDir = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;

                if (animator != null)
                    animator.SetBool("bMove", true);
                break;

            case State.Waiting:
                if (animator != null)
                    animator.SetBool("bMove", false);
                break;

            case State.Dying:
                if (deathEffect != null)
                    Instantiate(deathEffect, transform.position, Quaternion.identity);
                break;

            case State.Dead:
                Destroy(gameObject);
                break;
        }
    }

    // =========================
    // 各状態処理
    // =========================

    void UpdateShooting()
    {
        timer += Time.deltaTime;

        if (timer >= shotInterval)
        {
            Shoot();
            shotFired++;
            timer = 0f;

            if (shotFired >= shotCount)
                ChangeState(State.Moving);
        }
    }

    void UpdateMoving()
    {
        timer += Time.deltaTime;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        if (timer >= moveDuration)
            ChangeState(State.Waiting);
    }

    void UpdateWaiting()
    {
        timer += Time.deltaTime;

        if (timer >= waitTime)
            ChangeState(State.Shooting);
    }

    void UpdateDying()
    {
        timer += Time.deltaTime;

        if (timer >= 1.0f)
            ChangeState(State.Dead);
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }

    // =========================
    // 攻撃
    // =========================

    void Shoot()
    {
        if (animator != null)
            animator.SetTrigger("tAttack");

        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + transform.forward + shotOffset,
            Quaternion.identity
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = transform.forward * bulletSpeed;
    }

    // =========================
    // ダメージ
    // =========================

    public bool TakeDamage(float damage)
    {
        if (state == State.Dead || state == State.Dying)
            return false;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPBar();

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        if (!isLowHpEffectPlayed && currentHP <= maxHP * 0.4f)
        {
            if (lowHpEffect != null)
                Instantiate(lowHpEffect, transform.position, Quaternion.identity, transform);

            isLowHpEffectPlayed = true;
        }

        if (currentHP <= 0)
        {
            BlowAway(this.transform);
            return true;
        }

        return false;
    }

    void UpdateHPBar()
    {
        if (hpSlider != null)
            hpSlider.value = currentHP / maxHP;
    }

    // =========================
    // 吹き飛び（45°判定）
    // =========================
    public void BlowAway(Transform CurrentTransform)
    {
        if (isKnockback) return;

        isKnockback = true;

        // CharacterController無効化
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        // ★後ろ45°の敵を探す
        Transform target = FindEnemyInBack45(CurrentTransform);

        if (target != null)
        {
            knockbackDir = (target.position - transform.position);
        }
        else
        {
            knockbackDir = -transform.forward;
        }

        knockbackDir.y = 0f;
        knockbackDir.Normalize();

        knockbackTimer = 0f;
    }

    void UpdateKnockback()
    {
        knockbackTimer += Time.deltaTime;

        float speed = knockbackForce * 5f;

        // ★強制移動
        transform.position += knockbackDir * speed * Time.deltaTime;

        // 減速
        knockbackForce = Mathf.Lerp(knockbackForce, 0f, Time.deltaTime * 5f);

        if (knockbackTimer >= knockbackDuration)
        {
            isKnockback = false;
            ChangeState(State.Dying);
        }
    }

    // =========================
    // 後ろ45°の敵検索
    // =========================

    Transform FindEnemyInBack45(Transform CuurentTransform)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius);

        Transform best = null;
        float minDist = Mathf.Infinity;

        Vector3 back = this.transform.position - CuurentTransform.position;
        back.y = 0f;

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector3 to = col.transform.position - transform.position;
            to.y = 0f;

            if (to.sqrMagnitude < 0.0001f) continue;

            float angle = Vector3.Angle(back, to);

            if (angle <= 45f)
            {
                float dist = to.magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    best = col.transform;
                }
            }
        }

        return best;
    }


    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("aaaa");

            EnemyController EC = collision.gameObject.GetComponent<EnemyController>();
            if (EC != null || EC.isKnockback)
            {
                BlowAway(collision.transform);
            }

        }

    }

}