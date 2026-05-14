using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // 敵管理
    private List<GameObject> enemies = new List<GameObject>();
    //プレイヤー
    private GameObject player;
    private Vector3 playerStartPos;

    private Renderer goalRenderer;
    private Vector3 goalDefaultScale;
    private bool isGoalActive = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerStartPos = player.transform.position;
        }

        // ゴール見た目取得
        goalRenderer = GetComponent<Renderer>();

        // 元の大きさ保存
        goalDefaultScale = transform.localScale;

        // 最初は灰色
        if (goalRenderer != null)
        {
            goalRenderer.material.color = Color.gray;
        }

        GenerateStage(currentStage);
    }

    void Update()
    {
        SortEnemiesByPlayerDistance();

        // 敵全滅でゴール解放
        if (!isGoalActive && GetEnemyCount() <= 0)
        {
            ActivateGoal();
        }
    }

    void ActivateGoal()
    {
        if (isGoalActive) return;

        isGoalActive = true;

        Debug.Log("ゴール解放！");

        // 黄色にする
        if (goalRenderer != null)
        {
            goalRenderer.material.color = Color.yellow;
        }

        // 少し大きくする
        transform.localScale =
            goalDefaultScale * 1.3f;
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

            // Enemyタグで自動登録
            if (obj.CompareTag("Enemy"))
            {
                enemies.Add(obj);
            }
        }
    }

    // -----------------------------
    // 敵管理API
    // -----------------------------

    // 生きてる敵リスト
    public List<GameObject> GetEnemies()
    {
        enemies.RemoveAll(e => e == null);
        return enemies;
    }

    // 敵数
    public int GetEnemyCount()
    {
        enemies.RemoveAll(e => e == null);
        return enemies.Count;
    }

    // 敵座標リスト
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

    private void SortEnemiesByPlayerDistance()
    {
        // Playerが見つからない
        if (player == null) return;

        // 消えた敵を削除
        enemies.RemoveAll(e => e == null);

        // Player座標
        Vector3 playerPos = player.transform.position;

        // Playerに近い順に並び替え
        enemies.Sort((a, b) =>
        {
            float distA =Vector3.SqrMagnitude(a.transform.position - playerPos);

            float distB =Vector3.SqrMagnitude(b.transform.position - playerPos);

            return distA.CompareTo(distB);
        });
    }

    public GameObject GetNearestEnemy()
    {
        // 内部でソート
        SortEnemiesByPlayerDistance();

        // デバッグ確認
        foreach (var enemy in enemies)
        {
            Debug.Log(enemy.name);
        }

        // 敵がいない
        if (enemies.Count == 0)
            return null;

        // 一番近い敵
        return enemies[0];
    }


    // -----------------------------
    // ステージ制御
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

        //ゴールの色を戻す
        isGoalActive = false;

        transform.localScale = goalDefaultScale;

        if (goalRenderer != null)
        {
            goalRenderer.material.color = Color.gray;
        }
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
            Debug.Log("ゲームクリア!");

            SceneManager.LoadScene("Scene_Result");

            yield break;
        }

        ResetPlayer();

        Refresh();

        yield return new WaitForSeconds(0.5f);
    }

    //Playerのポジションリセット
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

        Debug.Log("Playerリセット");
    }

    //確認用
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("何か触れた : " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Playerじゃない");
            return;
        }

        Debug.Log("Playerが触れた");
        Debug.Log("敵の数 : " + GetEnemyCount());

        if (GetEnemyCount() > 0)
        {
            Debug.Log("まだ敵がいる");
            return;
        }

        Debug.Log("次ステージへ");
        NextStage();
    }
}