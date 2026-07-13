using UnityEngine;

public class SC_EnemyAttackPoolProvider : MonoBehaviour
{
    [Tooltip("éqìGÇÃMultiíeópPooléÌóﬁ")]
    [SerializeField]
    private SC_EnemyObjectPoolManager.EnemyPoolType bulletMultiPoolType =
        SC_EnemyObjectPoolManager.EnemyPoolType.BulletMulti;

    public SC_ObjectPool GetBulletMultiPool()
    {
        if (SC_EnemyObjectPoolManager.Instance == null)
        {
            Debug.LogWarning("SC_EnemyObjectPoolManager Ç™SceneÇ…Ç†ÇËÇ‹ÇπÇÒÅB");
            return null;
        }

        return SC_EnemyObjectPoolManager.Instance.GetPool(bulletMultiPoolType);
    }
}
