using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SC_SEManager : MonoBehaviour
{
    public static SC_SEManager Instance { get; private set; }

    [Serializable]
    public struct SEConfig
    {
        [Tooltip("SEを識別するキー")]
        public string seKey;

        public AudioClip audioClip;

        [Range(0f, 1f)]
        public float volume;

        [Range(-3f, 3f)]
        public float pitch;

        [Tooltip("0なら2D、1なら完全な3Dサウンド")]
        [Range(0f, 1f)]
        public float spatialBlend;

        [Min(0)]
        public int defaultCapacity;

        [Min(1)]
        public int maxCapacity;
    }

    [SerializeField]
    private List<SEConfig> _seConfigs = new List<SEConfig>();

    private readonly Dictionary<string, SEConfig> _configDictionary =
        new Dictionary<string, SEConfig>();

    private readonly Dictionary<string, ObjectPool<GameObject>> _seDictionary =
        new Dictionary<string, ObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
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
        foreach (SEConfig config in _seConfigs)
        {
            if (string.IsNullOrEmpty(config.seKey))
            {
                Debug.LogWarning("SEのキーが設定されていません。");
                continue;
            }

            if (config.audioClip == null)
            {
                Debug.LogWarning(
                    $"SEキー '{config.seKey}' にAudioClipが設定されていません。");
                continue;
            }

            if (_configDictionary.ContainsKey(config.seKey))
            {
                Debug.LogError(
                    $"SEキー '{config.seKey}' が重複しています。");
                continue;
            }

            string key = config.seKey;
            int defaultCapacity = Mathf.Max(0, config.defaultCapacity);
            int maxCapacity = Mathf.Max(1, config.maxCapacity);

            _configDictionary.Add(key, config);

            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreateInstance(key),
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxCapacity
            );

            _seDictionary.Add(key, pool);

            // プールを事前生成する
            var warmUpList = new List<GameObject>();

            for (int i = 0; i < defaultCapacity; i++)
            {
                warmUpList.Add(pool.Get());
            }

            foreach (GameObject obj in warmUpList)
            {
                pool.Release(obj);
            }
        }
    }

    private GameObject CreateInstance(string key)
    {
        SEConfig config = _configDictionary[key];

        var obj = new GameObject($"PooledSE_{key}");
        obj.transform.SetParent(transform);

        AudioSource audioSource = obj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = config.audioClip;
        audioSource.volume = config.volume;
        audioSource.pitch = config.pitch;
        audioSource.spatialBlend = config.spatialBlend;

        SC_PooledSE pooledSE = obj.AddComponent<SC_PooledSE>();

        pooledSE.RegisterReturnAction(targetObj =>
        {
            if (_seDictionary.TryGetValue(key, out var pool))
            {
                pool.Release(targetObj);
            }
        });

        obj.SetActive(false);
        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnReleaseToPool(GameObject obj)
    {
        if (obj.TryGetComponent<AudioSource>(out var audioSource))
        {
            audioSource.Stop();
        }

        obj.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }

    /// <summary>
    /// 指定位置でSEを再生します。
    /// </summary>
    public GameObject PlaySE(string key, Vector3 position)
    {
        if (!_seDictionary.TryGetValue(key, out var pool))
        {
            Debug.LogError(
                $"SEキー '{key}' に対応するSEが登録されていません。");
            return null;
        }

        GameObject obj = pool.Get();
        obj.transform.position = position;

        SC_PooledSE pooledSE = obj.GetComponent<SC_PooledSE>();
        pooledSE.Play();

        return obj;
    }

    /// <summary>
    /// 2Dサウンドなど、位置を指定しないSEを再生します。
    /// </summary>
    public GameObject PlaySE(string key)
    {
        return PlaySE(key, Vector3.zero);
    }

    /// <summary>
    /// 音量とピッチを一時的に指定して再生します。
    /// </summary>
    public GameObject PlaySE(
        string key,
        Vector3 position,
        float volume,
        float pitch = 1f)
    {
        if (!_seDictionary.TryGetValue(key, out var pool))
        {
            Debug.LogError(
                $"SEキー '{key}' に対応するSEが登録されていません。");
            return null;
        }

        GameObject obj = pool.Get();
        obj.transform.position = position;

        SC_PooledSE pooledSE = obj.GetComponent<SC_PooledSE>();
        pooledSE.Play(volume, pitch);

        return obj;
    }

    /// <summary>
    /// 再生中のSEを途中で停止し、プールへ返します。
    /// </summary>
    public void StopSE(GameObject seObject)
    {
        if (seObject == null)
        {
            return;
        }

        if (seObject.TryGetComponent<SC_PooledSE>(out var pooledSE))
        {
            pooledSE.StopAndReturn();
        }
    }
}
