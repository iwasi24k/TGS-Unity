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
        //死亡エフェクト
        if (deathEffect != null)
        {
            Debug.Log("エフェクト発動！");
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 行動停止
        StopAllCoroutines();

        // 吹き飛び処理へ
        StartCoroutine(DieRoutine());
    }

    // 死亡時の吹き飛び処理
    IEnumerator DieRoutine()
    {
        // 一番近い敵を探す
        Transform targetEnemy = FindNearestEnemy();

        // Rigidbody取得（無ければ追加）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // 物理挙動を有効化
        rb.isKinematic = false;

        if (targetEnemy != null)
        {
            // ターゲット方向に向かうベクトル
            Vector3 direction = (targetEnemy.position - transform.position).normalized;

            // 少し上方向に持ち上げる
            direction.y = 0.5f;

            // 力を加える
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
        else
        {
            // 敵がいない場合は上に飛ばす
            rb.AddForce(Vector3.up * knockbackForce, ForceMode.Impulse);
        }

        // 一定時間後に削除
        yield return new WaitForSeconds(destroyDelay);

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

    public void BlowAway()
    {

    }
}