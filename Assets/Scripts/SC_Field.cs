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

        public Vector3 goalPosition;
        public Vector3 playerStartPosition;
    }

    [SerializeField] private StageData[] stages;
    [SerializeField] private int currentStage = 0;

    // 生成したObject管理
    private List<GameObject> objects =
        new List<GameObject>();

    //==================================================
    // EnemyManager
    //==================================================
    [SerializeField]
    private SC_EnemyManager enemyManager;

    //==================================================
    // Goal
    //==================================================
    [SerializeField]
    private SC_Goal goal;

    //==================================================
    // Player
    //==================================================
    [SerializeField] int playerHealEffect = 10;     //TODO: スクリプト分離
    private GameObject player;

    private Vector3 playerStartPos;

    void Start()
    {
        player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerStartPos =
                player.transform.position;
        }

        if(enemyManager == null)
        {
            enemyManager = FindFirstObjectByType<SC_EnemyManager>();
        }

        if(goal == null)
        {
            goal = FindFirstObjectByType<SC_Goal>();
        }

        // Goal初期化
        goal.Setup(this);

        GenerateStage(currentStage);
    }

    void Update()
    {
        // 敵全滅でゴール解放
        if (enemyManager.GetEnemyCount() <= 0)
        {
            goal.ActivateGoal();
        }
    }

    void GenerateStage(int stageIndex)
    {
        if (stages == null ||
            stages.Length == 0)
        {
            return;
        }

        if (stageIndex < 0 ||
            stageIndex >= stages.Length)
        {
            return;
        }

        // Enemy初期化
        enemyManager.ClearEnemies();

        StageData stage = stages[stageIndex];

        //ゴールの位置設定
        goal.transform.position =
            transform.position +
            stage.goalPosition;

        // プレイヤー初期位置設定
        playerStartPos =
            transform.position +
            stage.playerStartPosition;

        foreach (ObjData data in stage.objects)
        {
            Vector3 pos =
                transform.position +
                data.position;

            GameObject obj =
                Instantiate(
                    data.prefab,
                    pos,
                    Quaternion.Euler(data.rotation),
                    transform);

            obj.transform.localScale =
                data.scale;

            objects.Add(obj);

            // Enemy登録
            if (obj.CompareTag("Enemy"))
            {
                //enemyManager.AddEnemy(obj);
            }
        }
    }

    // -----------------------------
    // ステージ制御
    // -----------------------------

    public void Refresh()
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        objects.Clear();

        enemyManager.ClearEnemies();

        //SC_EnemyStartGate.ResetGate();

        GenerateStage(currentStage);

        // Goalを閉じる
        goal.CloseGoal();
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

            SceneManager.LoadScene(
                "Scene_Result");

            yield break;
        }


        Refresh();

        ResetPlayer();

        yield return new WaitForSeconds(0.5f);
    }

    // Playerのポジションリセット
    void ResetPlayer()
    {
        if (player == null) return;

        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position =
            playerStartPos;

        if (controller != null)
        {
            controller.enabled = true;
        }

        var playerHP = player.GetComponent<SC_PlayerHP>();
        if (playerHP != null)
        {
            playerHP.Heal(playerHealEffect);    //TODO: スクリプト分離
        }

        Debug.Log("Playerリセット");
    }
}