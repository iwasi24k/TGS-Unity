using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [System.Serializable]
    public class ObjData
    {
        public GameObject prefab;
        public Vector3 position;
        public Vector3 scale = Vector3.one;
        public Vector3 rotation = Vector3.zero;
    }

    [System.Serializable]
    public class StageData
    {
        public ObjData[] objects;
    }

    [SerializeField] private StageData[] stages;
    [SerializeField] private int currentStage = 0;

    private List<GameObject> rocks = new List<GameObject>();

    void Start()
    {
        GenerateStage(currentStage);
    }

    void GenerateStage(int stageIndex)
    {
        if (stages == null || stages.Length == 0) return;

        StageData stage = stages[stageIndex];

        foreach (ObjData data in stage.objects)
        {
            Vector3 pos = transform.position + data.position;

            GameObject rock = Instantiate(data.prefab, pos, Quaternion.Euler(data.rotation), transform);

            rock.transform.localScale = data.scale;

            rocks.Add(rock);
        }
    }

    public void Refresh()
    {
        foreach (GameObject rock in rocks)
        {
            Destroy(rock);
        }

        rocks.Clear();

        GenerateStage(currentStage);
    }

    public void NextStage()
    {
        currentStage++;

        if (currentStage >= stages.Length)
        {
            currentStage = 0;
        }

        Refresh();
    }
}
