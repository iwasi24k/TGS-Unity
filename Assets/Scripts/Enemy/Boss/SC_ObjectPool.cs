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

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialCount; i++)
        {
            CreateObject();
        }
    }

    private GameObject CreateObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
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
        obj.transform.position = position;
        obj.transform.rotation = rotation;
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
