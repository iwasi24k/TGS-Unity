using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SC_Field : MonoBehaviour
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

    private List<GameObject> objects = new List<GameObject>();

    // “GŠÇ—
    private List<GameObject> enemies = new List<GameObject>();
    //ƒvƒŒƒCƒ„[
    private GameObject player;
    private Vector3 playerStartPos;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerStartPos = player.transform.position;
        }

        GenerateStage(currentStage);
    }


    void GenerateStage(int stageIndex)
    {
        if (stages == null || stages.Length == 0) return;
        if (stageIndex < 0 || stageIndex >= stages.Length) return;

        enemies.Clear();

        StageData stage = stages[stageIndex];

        foreach (ObjData data in stage.objects)
        {
            Vector3 pos = transform.position + data.position;

            GameObject obj = Instantiate(data.prefab, pos, Quaternion.Euler(data.rotation), transform);

            obj.transform.localScale = data.scale;

            objects.Add(obj);

            // Enemyƒ^ƒO‚Å©“®“o˜^
            if (obj.CompareTag("Enemy"))
            {
                enemies.Add(obj);
            }
        }
    }

    // -----------------------------
    // “GŠÇ—API
    // -----------------------------

    // ¶‚«‚Ä‚é“GƒŠƒXƒg
    public List<GameObject> GetEnemies()
    {
        enemies.RemoveAll(e => e == null);
        return enemies;
    }

    // “G”
    public int GetEnemyCount()
    {
        enemies.RemoveAll(e => e == null);
        return enemies.Count;
    }

    // “GÀ•WƒŠƒXƒg
    public List<Vector3> GetEnemyPositions()
    {
        List<Vector3> list = new List<Vector3>();

        foreach (var e in enemies)
        {
            if (e != null)
                list.Add(e.transform.position);
        }

        return list;
    }

    // ˆê”Ô‹ß‚¢“G
    public GameObject GetNearestEnemy(Vector3 from)
    {
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            float dist = Vector3.SqrMagnitude(e.transform.position - from);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = e;
            }
        }

        return nearest;
    }

    // -----------------------------
    // ƒXƒe[ƒW§Œä
    // -----------------------------

    public void Refresh()
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
                Destroy(obj);
        }

        objects.Clear();
        enemies.Clear();

        GenerateStage(currentStage);
    }

    public void NextStage()
    {
        StartCoroutine(StageTransition());
    }

    IEnumerator StageTransition()
    {
        yield return new WaitForSeconds(1f);

        currentStage++;

        if (currentStage >= stages.Length)
        {
            currentStage = 0;
        }

        ResetPlayer();

        Refresh();

        yield return new WaitForSeconds(0.5f);
    }

    void ResetPlayer()
    {
        if (player == null) return;

        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = playerStartPos;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Debug.Log("PlayerƒŠƒZƒbƒg");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("‰½‚©G‚ê‚½ : " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Player‚¶‚á‚È‚¢");
            return;
        }

        Debug.Log("Player‚ªG‚ê‚½");
        Debug.Log("“G‚Ì” : " + GetEnemyCount());

        if (GetEnemyCount() > 0)
        {
            Debug.Log("‚Ü‚¾“G‚ª‚¢‚é");
            return;
        }

        Debug.Log("ŸƒXƒe[ƒW‚Ö");
        NextStage();
    }
}