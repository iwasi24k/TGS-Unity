using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SC_EffectManager : MonoBehaviour
{
    public static SC_EffectManager Instance { get; private set; }

    [Serializable]
    public struct EffectConfig
    {
        public string effectKey;            // Unique identifier for the effect
        public GameObject effectPrefab;     // Prefab of the effect to be pooled
        public int defaultCapacity;         // Initial pool size
        public int maxCapacity;             // Maximum pool size
    }

    [SerializeField] private List<EffectConfig> _effectConfigs = new List<EffectConfig>();
    private Dictionary<string, EffectConfig> _configDictionary = new Dictionary<string, EffectConfig>();
    private Dictionary<string, ObjectPool<GameObject>> _effectDictionary = new Dictionary<string, ObjectPool<GameObject>>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        foreach(var config in _effectConfigs)
        {
            if (config.effectPrefab == null || string.IsNullOrEmpty(config.effectKey)) continue;

            _configDictionary.Add(config.effectKey, config);

            string key = config.effectKey;
            var pool = new ObjectPool<GameObject>(
                    createFunc: () => CreateInstance(key),
                    actionOnGet: OnGetFromPool,
                    actionOnRelease: OnReleaseToPool,
                    actionOnDestroy: OnDestroyPoolObject,
                    collectionCheck: false,
                    defaultCapacity: config.defaultCapacity,
                    maxSize: config.maxCapacity
                );

            _effectDictionary.Add(key, pool);

            // Pre-warm the pool with the default capacity
            List<GameObject> warmUpList = new List<GameObject>();
            for(int i = 0; i < config.defaultCapacity ; i++)
            {
                warmUpList.Add(pool.Get());
            }

            // Return the pre-warmed instances back to the pool
            foreach (var obj in warmUpList)
            {
                pool.Release(obj);
            }
        }

    }

    // --- Callbacks for pool control ---
    private GameObject CreateInstance(string key)
    {
        GameObject obj = Instantiate(_configDictionary[key].effectPrefab, transform);

        // Check for SC_PooledEffect component attachment
        var pooledEffect = obj.GetComponent<SC_PooledEffect>();
        if(pooledEffect == null)
        {
            pooledEffect = obj.AddComponent<SC_PooledEffect>();
        }

        pooledEffect.RegisterReturnAction((targetObj) =>{
            if(_effectDictionary.TryGetValue(key,out var pool))
            {
                pool.Release(targetObj);
            }
        });

        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);
        if(obj.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Play(true);
        }
    }

    private void OnReleaseToPool(GameObject obj)
    {
        if(obj.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        obj.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        // Instances that exceed the maximum capacity will be destroyed
        Destroy(obj);
    }

    // --- Public API for external calls ---
    /// <summary>
    /// Retrieve an effect from the pool and play them at the specified position and rotation
    /// </summary>
    public GameObject PlayEffect(string key, Vector3 position, Quaternion rotation)
    {
        if(_effectDictionary.TryGetValue(key, out var pool))
        {
            GameObject obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        Debug.LogError($"Effect with key '{key}' not found in the pool.");
        return null;
    }

    public GameObject PlayEffect(string key, Vector3 Position)
    {
        return PlayEffect(key, Position, Quaternion.identity);
    }

}
