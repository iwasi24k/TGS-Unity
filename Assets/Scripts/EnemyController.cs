using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("ターゲット")]
    private Transform player;

    [Header("弾")]
    public GameObject bulletPrefab;
    //発射される弾の速度
    public float bulletSpeed = 10f;
    //弾と弾の間
    public float shotInterval = 1.0f;

    // 🔥追加：最大発射数
    public int maxShotCount = 5;

    [Header("移動")]
    public float moveSpeed = 2f;
    //どのくらい動き続けるか(長くすれば移動距離が増える)
    public float moveDuration = 1.5f;
    //撃つ、動く、「待つ」、また撃つの「待つ」の時間
    public float waitTime = 1.0f;

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

        StartCoroutine(EnemyRoutine());
    }

    IEnumerator EnemyRoutine()
    {
        while (true)
        {
            // プレイヤー方向を見る
            if (player != null)
            {
                Vector3 lookPos = player.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }

            // 🔥ランダムな発射数を決定（1〜maxShotCount）
            int shotCount = Random.Range(1, maxShotCount + 1);

            // 弾発射
            for (int i = 0; i < shotCount; i++)
            {
                Shoot();
                yield return new WaitForSeconds(shotInterval);
            }

            // ランダム移動
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;

            float timer = 0;
            while (timer < moveDuration)
            {
                transform.Translate(randomDir * moveSpeed * Time.deltaTime, Space.World);
                timer += Time.deltaTime;
                yield return null;
            }

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
}