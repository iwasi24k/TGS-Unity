using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] public GameObject[] Rock = new GameObject[4];
    [SerializeField] public float minDistance = 2.0f;
    [SerializeField] public float scaleMultiplier = 1.0f;

    [SerializeField] private int[] rockNumPerStage;
    [SerializeField] private int currentStage = 0;

    [SerializeField] public Vector3 fieldSize = new Vector3(10, 0, 10);

    private List<GameObject> rocks = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int currentRockNum = rockNumPerStage[currentStage];

        for (int i = 0; i < currentRockNum; i++)
        {
            GameObject prefab = Rock[Random.Range(0, Rock.Length)];

            Vector3 pos;
            int retry = 0;

            do
            {
                float x = Random.Range(-fieldSize.x / 2, fieldSize.x / 2);
                float z = Random.Range(-fieldSize.z / 2, fieldSize.z / 2);

                pos = transform.position + new Vector3(x, 0, z);

                retry++;

            } while (IsOverlapping(pos, prefab) && retry < 50);

            GameObject rock = Instantiate(prefab, pos, Quaternion.identity, transform);

            rock.transform.localScale = prefab.transform.localScale * scaleMultiplier;

            rocks.Add(rock);
        }
    }

    bool IsOverlapping(Vector3 pos, GameObject prefab)
    {
        Collider col = prefab.GetComponent<Collider>();
        if (col == null) return false;

        float radiusA = col.bounds.extents.magnitude * scaleMultiplier;

        foreach (GameObject rock in rocks)
        {
            if (rock == null) continue;

            Collider childCol = rock.GetComponent<Collider>();
            if (childCol == null) continue;

            float radiusB = childCol.bounds.extents.magnitude;

            float dist = Vector3.Distance(pos, rock.transform.position);

            if (dist < radiusA + radiusB)
            {
                return true;
            }
        }

        return false;
    }

    public void Refresh()
    {
        foreach (GameObject rock in rocks)
        {
            Destroy(rock);
        }

        rocks.Clear();

        Start();
    }

    public void NextStage()
    {
        currentStage++;

        // 配列の範囲チェック
        if (currentStage >= rockNumPerStage.Length)
        {
            currentStage = 0; // ループさせる（または止める）
        }

        Refresh();
    }
}
