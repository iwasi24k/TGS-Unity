using UnityEngine;

public class SC_EnemyAttackPoolProvider : MonoBehaviour
{
    [Tooltip("子敵のMulti弾用Pool。Scene上の子敵なら直接指定可能"), SerializeField]
    private SC_ObjectPool bulletMultiPool;

    [Tooltip("直接指定されていない場合、この名前のPoolをScene内から探す"), SerializeField]
    private string bulletMultiPoolObjectName = "PF_BulletMulti Pool";

    private void Awake()
    {
        if (bulletMultiPool != null) return;

        GameObject poolObj = GameObject.Find(bulletMultiPoolObjectName);

        if (poolObj == null)
        {
            Debug.LogWarning("BulletMulti用Poolが見つかりません : " + bulletMultiPoolObjectName);
            return;
        }

        bulletMultiPool = poolObj.GetComponent<SC_ObjectPool>();

        if (bulletMultiPool == null)
        {
            Debug.LogWarning("見つけたObjectにSC_ObjectPoolが付いていません : " + bulletMultiPoolObjectName);
        }
    }

    public SC_ObjectPool GetBulletMultiPool()
    {
        return bulletMultiPool;
    }
}
