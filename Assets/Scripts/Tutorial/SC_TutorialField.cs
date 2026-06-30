using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_TutorialField : MonoBehaviour
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

    [Header("Stage Data")]
    [SerializeField]
    private StageData[] stages;

    [SerializeField]
    private int currentStage = 0;

    // =========================================
    // Stage Objects
    // =========================================
    private List<GameObject> objects =
        new List<GameObject>();

    // =========================================
    // Enemy Manager
    // =========================================
    [SerializeField]
    private SC_EnemyManager enemyManager;

    // =========================================
    // Player
    // =========================================
    [SerializeField]
    private GameObject player;

    private Vector3 playerStartPos;

    // =========================================
    private void Start()
    {
        if (player == null)
        {
            player =
                GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerStartPos =
                player.transform.position;
        }

        if (enemyManager == null)
        {
            enemyManager =
                FindFirstObjectByType<SC_EnemyManager>();
        }

        GenerateStage(currentStage);
    }

    // =========================================
    // Stage Generate
    // =========================================
    private void GenerateStage(int stageIndex)
    {
        if (stages == null)
        {
            return;
        }

        if (stageIndex < 0 ||
            stageIndex >= stages.Length)
        {
            return;
        }

        if (enemyManager != null)
        {
            enemyManager.ClearEnemies();
        }

        StageData stage =
            stages[stageIndex];

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

        }
    }

    // =========================================
    // Refresh
    // =========================================
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

        if (enemyManager != null)
        {
            enemyManager.ClearEnemies();
        }

        SC_EnemyStartGate.ResetGate();

        GenerateStage(currentStage);
    }

    // =========================================
    // Next Stage
    // =========================================
    public void NextStage()
    {
        StartCoroutine(StageTransition());
    }

    private IEnumerator StageTransition()
    {
        yield return new WaitForSeconds(1f);

        currentStage++;

        if (currentStage >= stages.Length)
        {
            Debug.Log("Tutorial Complete");
            yield break;
        }

        ResetPlayer();

        Refresh();
    }

    // =========================================
    // Reset Stage
    // =========================================
    public void ResetStage()
    {
        Debug.Log(
            $"Tutorial Stage {currentStage} Reset");

        ResetPlayer();

        Refresh();
    }

    // =========================================
    // Reset Player
    // =========================================
    private void ResetPlayer()
    {
        if (player == null)
        {
            return;
        }

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

        SC_PlayerAttackManager attackManager =
            player.GetComponent<SC_PlayerAttackManager>();

        if (attackManager != null)
        {
            attackManager.ResetCombo();
        }

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetCombo();
        }
    }

    // =========================================
    // Getter
    // =========================================
    public int GetCurrentStage()
    {
        return currentStage;
    }

    public int GetObjectCount()
    {
        objects.RemoveAll(o => o == null);

        return objects.Count;
    }

    public int GetEnemyCount()
    {
        if (enemyManager == null)
        {
            return 0;
        }

        return enemyManager.GetEnemyCount();
    }
}