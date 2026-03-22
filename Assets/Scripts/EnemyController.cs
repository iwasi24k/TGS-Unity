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
                // プレイヤーの位置を取得
                Vector3 lookPos = player.position;

                // 上下の回転を防ぐためにY座標を固定
                lookPos.y = transform.position.y;

                // プレイヤーの方向を向く
                transform.LookAt(lookPos);
            }

            // 発射する弾数をランダムで決定（1〜maxShotCount）
            int shotCount = Random.Range(1, maxShotCount + 1);

            // 決めた数だけ弾を発射
            for (int i = 0; i < shotCount; i++)
            {
                Shoot();

                // 次の発射まで待機
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
                // ワールド座標で移動（回転の影響を受けない）
                transform.Translate(randomDir * moveSpeed * Time.deltaTime, Space.World);

                // 経過時間を加算
                timer += Time.deltaTime;

                // 次のフレームまで待機
                yield return null;
            }

            // 行動の区切りとして少し待機
            yield return new WaitForSeconds(waitTime);
        }
    }

    void Shoot()
    {
        // 弾のPrefabが設定されていない場合は何もしない
        if (bulletPrefab == null) return;

        // 弾を生成（敵の前方に少しずらして出す）
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + transform.forward,
            Quaternion.identity
        );

        // 弾のRigidbodyを取得
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // Rigidbodyがある場合のみ速度を設定
        if (rb != null)
        {
            // 前方向に速度を与える
            rb.linearVelocity = transform.forward * bulletSpeed;
        }
    }

    // 外部（弾など）から呼ばれるダメージ処理
    public void TakeDamage(float damage)
    {
        // HPを減らす
        currentHP -= damage;

        // HPが0未満や最大値を超えないように制限
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        // HPバーを更新
        UpdateHPBar();

        // HPが0になったら死亡処理
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
            // 現在HPを0〜1の割合に変換してSliderに反映
            hpSlider.value = currentHP / maxHP;
        }
    }

    // 敵が倒されたときの処理
    void Die()
    {
        // 敵オブジェクトを削除
        Destroy(gameObject);
    }
}