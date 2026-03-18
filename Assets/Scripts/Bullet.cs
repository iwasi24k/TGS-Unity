using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f; // ← 何秒で消えるか

    void Start()
    {
        // 一定時間後に自動で削除
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // プレイヤーならヒット
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("プレイヤーにヒット！");
        }

        // 当たったら消える
        Destroy(gameObject);
    }
}