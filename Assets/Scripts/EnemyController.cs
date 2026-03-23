using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Sliderを使うために必要

public class EnemyController : MonoBehaviour
{
    [Header("ターゲット")]
    // プレイヤーのTransform（位置や向きを取得するために使う）
    private Transform player;

    [Header("弾")]
    // 発射する弾のPrefab
    public GameObject bulletPrefab;

    // 弾の移動速度
    public float bulletSpeed = 10f;

    // 弾を発射する間隔（秒）
    public float shotInterval = 1.0f;

    // 1回の攻撃で発射する弾の最大数
    public int maxShotCount = 5;

    [Header("移動")]
    // 敵の移動スピード
    public float moveSpeed = 2f;

    // どれくらいの時間移動し続けるか
    public float moveDuration = 1.5f;

    // 攻撃と移動の後に待機する時間
    public float waitTime = 1.0f;

    [Header("HP")]
    // 最大HP
    public float maxHP = 100f;

    // 現在のHP（外から直接いじらないのでprivate）
    private float currentHP;

    // HPゲージとして使うSlider
    public Slider hpSlider;

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

    void Start()
    {
        // タグを使ってシーン内のプレイヤーを取得する
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // プレイヤーが見つかった場合
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            // タグが設定されていない場合のエラー表示
            Debug.LogError("Playerタグが見つかりません！");
        }

        // HPを最大値で初期化
        currentHP = maxHP;

        // HPバーを初期状態に更新
        UpdateHPBar();

        // 敵の行動ループを開始
        StartCoroutine(EnemyRoutine());
    }

    IEnumerator EnemyRoutine()
    {
        // 無限ループ（敵が生きている限り繰り返す）
        while (true)
        {
            // プレイヤーが存在する場合、そちらを向く
            if (player != null)
            {
                Vector3 lookPos = player.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }

            // 発射する弾数をランダムで決定（1〜maxShotCount）
            int shotCount = Random.Range(1, maxShotCount + 1);

            // 決めた数だけ弾を発射
            for (int i = 0; i < shotCount; i++)
            {
                Shoot();
                yield return new WaitForSeconds(shotInterval);
            }

            // ランダムな移動方向を決定（前後左右）
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;

            float timer = 0;

            // 一定時間だけ移動する
            while (timer < moveDuration)
            {
                transform.Translate(randomDir * moveSpeed * Time.deltaTime, Space.World);
                timer += Time.deltaTime;
                yield return null;
            }

            // 行動の区切りとして少し待機
            yield return new WaitForSeconds(waitTime);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + transform.forward,
            Quaternion.identity
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }
    }

    // 外部（弾など）から呼ばれるダメージ処理
    public void TakeDamage(float damage)
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
        }
    }

    // HPバー（Slider）の見た目を更新する
    void UpdateHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }
    }

    // 敵が倒されたときの処理
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
}