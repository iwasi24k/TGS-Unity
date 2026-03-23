using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("ターゲット")]
    private Transform player;

    [Header("弾")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float shotInterval = 1.0f;
    public int maxShotCount = 5;

    // 攻撃箇所のオフセット
    public Vector3 shotOffset = new Vector3(0f, 0f, 0f);

    [Header("移動")]
    public float moveSpeed = 2f;
    public float moveDuration = 1.5f;
    public float waitTime = 1.0f;

    public float rotationSpeed = 5f;

    [Header("HP")]
    public float maxHP = 100f;
    private float currentHP;
    public Slider hpSlider;
    public bool IsAlive => currentHP > 0;

    [Header("エフェクト")]
    public GameObject hitEffect;     // 被弾エフェクト
    public GameObject lowHpEffect;   // 低HPエフェクト
    public GameObject deathEffect;   // 死亡エフェクト

    private bool isLowHpEffectPlayed = false;

    [Header("死亡時の吹き飛び")]
    // 吹き飛ぶ力の強さ
    public float knockbackForce = 10f;

    // 敵を検知する範囲
    public float searchRadius = 10f;

    // 消えるまでの時間
    public float destroyDelay = 1.5f;

    // 速度判定用（この速度以下なら死亡処理を実行）
    public float deathVelocityThreshold = 0.5f;

    // 速度が閾値以下になるのを待つ最大時間（秒）
    public float maxWaitForLowVelocity = 3f;

    // 一度死亡処理を開始したら重複させないフラグ
    private bool isDying = false;

    [Header("Animator")]
    public Animator animator;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Playerタグが見つかりません！");
        }

        currentHP = maxHP;
        UpdateHPBar();
        StartCoroutine(EnemyRoutine());

        animator = GetComponent<Animator>();
    }

    IEnumerator EnemyRoutine()
    {
        while (IsAlive)
        {
            if (player != null)
            {
                Vector3 lookPos = player.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }

            int shotCount = Random.Range(1, maxShotCount + 1);

            for (int i = 0; i < shotCount; i++)
            {
                Shoot();
                yield return new WaitForSeconds(shotInterval);
            }

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            float timer = 0;
            if(timer == 0) animator.SetBool("bMove", true);
            else if(timer >= moveDuration) animator.SetBool("bMove", false);

            while (timer < moveDuration)
            {
                // 移動方向が有効ならその方向へ向く（スムーズ回転）
                if (randomDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(randomDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }

                transform.Translate(randomDir * moveSpeed * Time.deltaTime, Space.World);
                timer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    void Shoot()
    {
        animator.SetTrigger("tAttack");
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + (transform.forward) + shotOffset,
            Quaternion.identity
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }
    }

    public bool TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPBar();
        Debug.Log("TakeDamage呼ばれた HP:" + currentHP);

        //被弾エフェクト
        if (hitEffect != null)
        {
            Debug.Log("エフェクト発動！");
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        //lowエフェクト
        if (!isLowHpEffectPlayed && currentHP <= maxHP * 0.4f)
        {
            Debug.Log("エフェクト発動！");
            if (lowHpEffect != null)
            {
                Instantiate(lowHpEffect, transform.position, Quaternion.identity, transform);
            }
            isLowHpEffectPlayed = true;
        }

        if (currentHP <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    void UpdateHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }
    }

    void Die()
    {
        if (isDying) return;

        // Rigidbody を取得して速度判定する（無ければ即死）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            ExecuteDeath();
            return;
        }

        // 既に速度が閾値以下なら即死亡処理
        if (rb.linearVelocity.magnitude <= deathVelocityThreshold)
        {
            ExecuteDeath();
        }
        else
        {
            // 閾値以下になるのを待つコルーチンを開始
            StartCoroutine(WaitForLowVelocityAndDie(rb));
        }
    }

    // 速度が閾値以下になるのを待つ（もしくはタイムアウト）して死亡処理を実行
    IEnumerator WaitForLowVelocityAndDie(Rigidbody rb)
    {
        if (isDying) yield break;
        isDying = true;

        float timer = 0f;
        while (timer < maxWaitForLowVelocity)
        {
            if (rb == null || rb.linearVelocity.magnitude <= deathVelocityThreshold)
            {
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        ExecuteDeath();
    }

    // 実際の死亡処理（エフェクト、行動停止、消去コルーチン開始）
    void ExecuteDeath()
    {
        if (isDying) return;
        isDying = true;

        //死亡エフェクト
        if (deathEffect != null)
        {
            Debug.Log("エフェクト発動！");
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 行動停止
        StopAllCoroutines();

        Destroy(gameObject);
    }

    // 周囲の敵の中から一番近いものを探す
    Transform FindNearestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            // 自分自身は除外
            if (col.gameObject == gameObject) continue;

            // Enemyタグのみ対象
            if (col.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = col.transform;
                }
            }
        }

        return nearest;
    }

    public void BlowAway(Transform CurrentTransform)
    {
        Vector3 BlowDir = this.transform.position - CurrentTransform.position;

        BlowDir.Normalize();

        Transform nearest = FindNearestEnemy();
        if (nearest == null)
        {
            Debug.Log("BlowAway: ターゲットが見つかりません。正面方向へ吹き飛ばします。");
            Rigidbody rbNoTarget = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            // kinematic にしていると物理で動かない -> false にする
            rbNoTarget.isKinematic = false;
            rbNoTarget.useGravity = true;
            rbNoTarget.linearVelocity = Vector3.zero;
            rbNoTarget.AddForce(BlowDir * knockbackForce, ForceMode.Impulse);
            return;
        }

        Vector3 TargetPosition = nearest.position;

        // 水平面（y軸）で角度を計算するため、y成分を取り除く
        Vector3 flatBlow = Vector3.ProjectOnPlane(BlowDir, Vector3.up);
        Vector3 flatToTarget = Vector3.ProjectOnPlane(TargetPosition - transform.position, Vector3.up);

        if (flatBlow.sqrMagnitude < 0.0001f || flatToTarget.sqrMagnitude < 0.0001f)
        {
            Debug.Log("BlowAway: 方向ベクトルが小さすぎます。");
            Rigidbody rbSmall = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rbSmall.isKinematic = false;
            rbSmall.useGravity = true;
            rbSmall.linearVelocity = Vector3.zero;
            rbSmall.AddForce(BlowDir * knockbackForce, ForceMode.Impulse);
            return;
        }

        flatBlow.Normalize();
        flatToTarget.Normalize();

        // 2つのベクトル間の角度（度単位）
        float angle = Vector3.Angle(flatBlow, flatToTarget);

        // 45°以内か確認
        bool isWithin45 = angle <= 45f;
        Debug.Log($"BlowAway: 角度 = {angle}°, 45°以内 = {isWithin45}");

        // Rigidbody取得（無ければ追加）して実際に吹き飛ばす
        Rigidbody rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        // 物理で動かすため kinematic を解除
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;

        if (isWithin45)
        {
            // ターゲット方向へ少し持ち上げて吹き飛ばす
            Vector3 forceDir = flatToTarget;
            forceDir.y = 0.5f;
            rb.AddForce(forceDir.normalized * knockbackForce, ForceMode.Impulse);
            Debug.Log("BlowAway: ターゲット方向へ吹き飛ばします。");
        }
        else
        {
            // 45°を超えるなら正面方向へ吹き飛ばす（例）
            rb.AddForce(BlowDir * knockbackForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //CollisionEnemyのRigitbodyがkineticならば、BlowAwayを呼び出す
            Rigidbody collisionRb = collision.gameObject.GetComponent<Rigidbody>();
            if (collisionRb != null && collisionRb.isKinematic)
            {
                BlowAway(collision.transform);
            }
        }
    }
}