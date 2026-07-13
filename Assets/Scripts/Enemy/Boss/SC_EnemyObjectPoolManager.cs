using System.Collections.Generic;
using UnityEngine;

public class SC_EnemyObjectPoolManager : MonoBehaviour
{
    public static SC_EnemyObjectPoolManager Instance { get; private set; }

    public enum EnemyPoolType
    {
        BulletMulti,
        FallingMissile,
        HomingMissile,
        RapidMissile,
        SplitFallingMissile,
        StraightMissile,
        WarningCircle,
        WarningRectangle,
        WarningSector,
        ReflectableMissile,
        LaunchVisualFallingMissile,
        LaunchVisualSplitFallingMissile
    }

    [System.Serializable]
    public class PoolData
    {
        [Tooltip("Poolの種類")]
        public EnemyPoolType poolType;

        [Tooltip("生成するPrefab")]
        public GameObject prefab;

        [Tooltip("最初に生成しておく数")]
        public int initialCount = 30;

        [Tooltip("足りなくなった時に追加生成するか")]
        public bool canExpand = true;
    }

    [Header("Pool Settings")]
    [SerializeField] private List<PoolData> poolDataList = new List<PoolData>();

    private readonly Dictionary<EnemyPoolType, SC_ObjectPool> poolDictionary =
        new Dictionary<EnemyPoolType, SC_ObjectPool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("SC_EnemyObjectPoolManager が複数存在しています。重複した方を削除します。");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateAllPools();
    }

    private void CreateAllPools()
    {
        poolDictionary.Clear();

        foreach (PoolData data in poolDataList)
        {
            CreatePool(data);
        }
    }

    private void CreatePool(PoolData data)
    {
        if (data == null)
        {
            Debug.LogWarning("PoolData が null です。");
            return;
        }

        if (data.prefab == null)
        {
            Debug.LogWarning($"{data.poolType} の prefab が設定されていません。");
            return;
        }

        if (poolDictionary.ContainsKey(data.poolType))
        {
            Debug.LogWarning($"{data.poolType} の Pool が重複登録されています。");
            return;
        }

        GameObject poolObj = new GameObject($"Pool_{data.poolType}");
        poolObj.transform.SetParent(transform);

        SC_ObjectPool objectPool = poolObj.AddComponent<SC_ObjectPool>();

        objectPool.Initialize(
            data.prefab,
            data.initialCount,
            data.canExpand
        );

        poolDictionary.Add(data.poolType, objectPool);
    }

    public SC_ObjectPool GetPool(EnemyPoolType poolType)
    {
        if (poolDictionary.TryGetValue(poolType, out SC_ObjectPool pool))
        {
            return pool;
        }

        Debug.LogWarning($"{poolType} の Pool が見つかりません。");
        return null;
    }

    public GameObject GetObject(
        EnemyPoolType poolType,
        Vector3 position,
        Quaternion rotation
    )
    {
        SC_ObjectPool pool = GetPool(poolType);

        if (pool == null)
        {
            return null;
        }

        return pool.GetObject(position, rotation);
    }

    public bool HasPool(EnemyPoolType poolType)
    {
        return poolDictionary.ContainsKey(poolType);
    }
}
