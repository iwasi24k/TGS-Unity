using System.Collections.Generic;
using UnityEngine;

public class SC_ObjectPool : MonoBehaviour
{
    [Tooltip("ƒv[ƒ‹‚·‚éPrefab"), SerializeField]
    private GameObject prefab;

    [Tooltip("Å‰‚É¶¬‚µ‚Ä‚¨‚­”"), SerializeField]
    private int initialCount = 30;

    [Tooltip("‘«‚è‚È‚­‚È‚Á‚½‚É’Ç‰Á¶¬‚·‚é‚©"), SerializeField]
    private bool canExpand = true;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private bool initialized = false;

    private void Awake()
    {
        // Inspector‚Åprefab‚ªİ’è‚³‚ê‚Ä‚¢‚éê‡‚¾‚¯©“®‰Šú‰»
        if (prefab != null)
        {
            Initialize(prefab, initialCount, canExpand);
        }
    }

    public void Initialize(GameObject prefab, int initialCount, bool canExpand = true)
    {
        if (initialized) return;

        this.prefab = prefab;
        this.initialCount = initialCount;
        this.canExpand = canExpand;

        if (this.prefab == null)
        {
            Debug.LogError($"{name} : Pool prefab is null");
            return;
        }

        for (int i = 0; i < this.initialCount; i++)
        {
            CreateObject();
        }

        initialized = true;
    }

    private GameObject CreateObject()
    {
        if (prefab == null)
        {
            Debug.LogError($"{name} : Cannot create pooled object because prefab is null");
            return null;
        }

        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);

        return obj;
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        if (!initialized)
        {
            Initialize(prefab, initialCount, canExpand);
        }

        if (pool.Count == 0)
        {
            if (canExpand)
            {
                CreateObject();
            }
            else
            {
                return null;
            }
        }

        GameObject obj = pool.Dequeue();

        obj.transform.SetParent(null);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform);

        pool.Enqueue(obj);
    }
}
