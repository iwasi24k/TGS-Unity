using UnityEngine;

public enum BossSummonMode
{
    AllElements,
    OneRandomElement,
    OneByIndex
}

[CreateAssetMenu(
    fileName = "SO_BossSummonState",
    menuName = "Boss/State/Summon")]
public class SC_BossSummonState : SC_EnemyBaceState
{
    [Tooltip("雑魚敵を召喚するまでの時間"), SerializeField] private float summonDelay = 0.5f;
    [Tooltip("攻撃Stateを終了してIdleに戻るまでの時間"), SerializeField] private float endDelay = 1.5f;

    [System.Serializable]
    public class SummonEnemyData
    {
        [Tooltip("召喚する子エネミーPrefab")]
        public GameObject enemyPrefab;

        [Tooltip("召喚する子エネミーのHP")]
        public int hp = 5;

        [Tooltip("召喚する数")]
        public int summonCount = 1;

        [Tooltip("ボスからどれくらい離れて出すか")]
        public float summonRadius = 3.0f;

        [Tooltip("召喚する高さ")]
        public float spawnHeight = 0.0f;

        [Tooltip("ランダム配置するか")]
        public bool randomPosition = true;

        [Tooltip("固定角度配置する場合の中心角度")]
        public float centerAngle = 0.0f;

        [Tooltip("固定角度配置する場合の角度範囲")]
        public float angleRange = 360.0f;
    }

    [Header("Summon Mode")]
    [SerializeField]
    private BossSummonMode summonMode = BossSummonMode.AllElements;

    [SerializeField]
    private int summonElementIndex = 0;

    [Header("Summon")]
    [SerializeField]
    private SummonEnemyData[] summonEnemyList;

    private float timer;
    private bool summoned;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Summon State Enter");

        timer = 0f;
        summoned = false;
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        if (!summoned && timer >= summonDelay)
        {
            summoned = true;
            SummonEnemies(Owner);
        }

        if (timer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Summon State Exit");
    }

    private void SummonEnemies(GameObject Owner)
    {
        if (Owner == null) return;
        if (summonEnemyList == null) return;
        if (summonEnemyList.Length == 0) return;

        switch (summonMode)
        {
            case BossSummonMode.AllElements:
                SummonAllElements(Owner);
                break;

            case BossSummonMode.OneRandomElement:
                SummonOneRandomElement(Owner);
                break;

            case BossSummonMode.OneByIndex:
                SummonOneByIndex(Owner, summonElementIndex);
                break;
        }
    }

    private void SummonElement(GameObject Owner, SummonEnemyData data)
    {
        if (data == null) return;
        if (data.enemyPrefab == null) return;
        if (data.summonCount <= 0) return;

        for (int i = 0; i < data.summonCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(Owner, data, i);

            GameObject enemyObj = Instantiate(
                data.enemyPrefab,
                spawnPos,Quaternion.
                identity);

            SC_EnemyStatusManager enemyManager =
                enemyObj.GetComponent<SC_EnemyStatusManager>();

            if (enemyManager != null)
            {
                enemyManager.SetHP(data.hp);
            }
        }
    }

    private void SummonAllElements(GameObject Owner)
    {
        for (int i = 0; i < summonEnemyList.Length; i++)
        {
            SummonElement(Owner, summonEnemyList[i]);
        }
    }

    private void SummonOneRandomElement(GameObject Owner)
    {
        int index = Random.Range(0, summonEnemyList.Length);

        SummonElement(Owner, summonEnemyList[index]);
    }

    private void SummonOneByIndex(GameObject Owner, int index)
    {
        if (index < 0) return;
        if (index >= summonEnemyList.Length) return;

        SummonElement(Owner, summonEnemyList[index]);
    }

    private Vector3 GetSpawnPosition(
    GameObject Owner,
    SummonEnemyData data,
    int index)
    {
        Vector3 basePos = Owner.transform.position;

        Vector3 dir;

        if (data.randomPosition)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;

            dir = new Vector3(
                randomCircle.x,
                0.0f,
                randomCircle.y
            );
        }
        else
        {
            float angle = data.centerAngle;

            if (data.summonCount > 1)
            {
                float t = (float)index / (data.summonCount - 1);
                angle = data.centerAngle - data.angleRange * 0.5f + data.angleRange * t;
            }

            dir = Quaternion.Euler(0.0f, angle, 0.0f) * Owner.transform.forward;
            dir.y = 0.0f;
            dir.Normalize();
        }

        Vector3 spawnPos = basePos + dir * data.summonRadius;
        spawnPos.y = data.spawnHeight;

        return spawnPos;
    }


}