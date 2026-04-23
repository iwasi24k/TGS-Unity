using UnityEngine;

public class SC_BulletMulti : MonoBehaviour
{
    public float lifeTime = 3f;

    public float spreadMoveTime = 0.3f; // 拡散移動の時間
    public float spreadSpeed = 10f;     // 拡散移動の速度
    public float straightSpeed = 15f;   // 直進移動の速度

    private float timer = 0f;
    private Vector3 initialDirection;
    private Vector3 straightDirection;
    private bool straightDirectionSet = false;

    private Transform player;
    private Transform owner;

    // すでにヒットしたかどうか
    private bool hasHit = false;

    public void SetOwner(Transform t)
    {
        owner = t;
    }

    void Start()
    {
        initialDirection = transform.forward;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer < spreadMoveTime)
        {
            // 拡散移動
            transform.position += initialDirection * spreadSpeed * Time.deltaTime;
        }
        else
        {
            // プレイヤー方向へ直進
            if (!straightDirectionSet)
            {
                if (owner != null && player != null)
                {
                    straightDirection = (player.position - owner.position).normalized;
                }
                else
                {
                    straightDirection = transform.forward;
                }
                straightDirectionSet = true;
            }
            transform.position += straightDirection * straightSpeed * Time.deltaTime;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        // すでに当たっていたら何もしない
        if (hasHit) return;

        // プレイヤーに当たった場合
        if (collision.gameObject.CompareTag("Player"))
        {
            //体力を減らす処理
        }

        // ヒット済みにする（これが重要）
        hasHit = true;

        // 弾を削除
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // すでに当たっていたら何もしない
        if (hasHit) return;

        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("Field"))
        {
            return;
        }
        // プレイヤーに当たった場合
        if (other.gameObject.CompareTag("Player"))
        {
            //体力を減らす処理

        }

        // ヒット済みにする（これが重要）
        hasHit = true;

        // 弾を削除
        Destroy(gameObject);
    }
}