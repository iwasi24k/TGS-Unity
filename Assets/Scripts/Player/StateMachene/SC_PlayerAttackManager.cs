using UnityEngine;

public enum AttackType
{
    Weak1,
    Weak2,
    Strong,
    Uppercut,
    Rotate,
    Door,
}

public class SC_PlayerAttackManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private SC_PlayerStateManager stateManager;
    [SerializeField] private SC_PlayerTarget sctarget;

    [Header("Attack Settings")]
    [Tooltip("攻撃範囲"), SerializeField] private Vector3 AttackAreaSize = new Vector3(2f, 2f, 2f);
    [Tooltip("飛びつきの範囲")]
    [SerializeField] private Vector3 JumpInAreaSize = new Vector3(3f, 2f, 5f);
    [Tooltip("ターゲット時の飛びつきの範囲")]
    [SerializeField] private Vector3 TargetingJumpInAreaSize = new Vector3(3f, 2f, 7f);
    [SerializeField] private float jumpInSpeed = 2f;
    [SerializeField] private float snapDistance = 0.5f;

    [Header("Damage Settings")]
    [SerializeField] private int weakDamage = 1;
    public int GetWeakDamage() => weakDamage;
    [SerializeField] private int straightDamage = 3;
    public int GetStraightDamage() => straightDamage;
    [SerializeField] private int rotateDamage = 4;
    public int GetRotateDamage() => rotateDamage;
    [SerializeField] private int uppercutDamage = 5;
    public int GetUppercutDamage() => uppercutDamage;
    [SerializeField] private int chargeDamage = 5;
    public int GetChargeDamage() => chargeDamage;

    [SerializeField, Tooltip("コンボのリセット時間（秒）")] private float comboResetTime = 2f;

    private float currentComboTime; // コンボリセットのタイマー
    private int currentComboCount = 0;
    private bool NextAttackIsStrong = false;
    private readonly Collider[] overlapCollision = new Collider[32];
    private Collider closestTarget = null;

    public void Start()
    {
        if(stateManager == null)
        {
            stateManager = GetComponent<SC_PlayerStateManager>();
        }

        if (sctarget == null)
        {
            sctarget = GetComponent<SC_PlayerTarget>();
        }
    }

    public void Update()
    {
        // コンボリセットのタイマー
        if (currentComboCount > 0)
        {
            currentComboTime -= Time.deltaTime;
            if (currentComboTime <= 0f)
            {
                ResetCombo();
            }
        }
    }

    // + ==============================================================================
    public float GetJumpInSpeed()
    {
        return jumpInSpeed;
    }

    public float GetSnapDistance()
    {
        return snapDistance;
    }

    public bool IsNextAttackStrong()
    {
        return NextAttackIsStrong;
    }

    public void AttackTransitionCheck(PlayerState statList, bool Strong)
    {
        NextAttackIsStrong = Strong;

        if (JumpInToTarget("Enemy"))
        {
            stateManager.ChangeState(statList.JumpIn);
        }
        else
        {
            if (NextAttackIsStrong || currentComboCount >= 3)
            {
                stateManager.ChangeState(statList.StrongAttack);
            }
            else
            {
                stateManager.ChangeState(statList.WeakAttack);
            }
        }
    }

    // タグで攻撃範囲内のオブジェクトを取得するメソッド
    public GameObject[] GetInAreaObjectByTag(string tag)
    {
        int count = SerchInArea(AttackAreaSize);
        if (count > 0)
        {
            return GetGameObjectsFromColliders(count, tag);
        }

        //どこにもいない
        return null;
    }

    public GameObject GetJumpInTarget()
    {
        //CollisionからGameObjectへの変換
        if (closestTarget != null)
        {
            return closestTarget.gameObject;
        }
        return null;
    }

    public bool CheckObjectInAttackArea(string tag)
    {
        int count = SerchInArea(AttackAreaSize);
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                var col = overlapCollision[i];
                if (col != null && col.CompareTag(tag))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // - ==============================================================================

    // Collider配列からGameObject配列を生成するヘルパーメソッドを追加
    private GameObject[] GetGameObjectsFromColliders(int count, string tag)
    {

        // まず一致数を数える（List を追加で using しなくても済むように2パス）
        int matchCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (overlapCollision[i] != null && overlapCollision[i].CompareTag(tag))
            {
                matchCount++;
            }
        }

        if (matchCount == 0) return null;

        GameObject[] result = new GameObject[matchCount];
        int idx = 0;
        for (int i = 0; i < count; i++)
        {
            if (overlapCollision[i] != null && overlapCollision[i].CompareTag(tag))
            {
                result[idx++] = overlapCollision[i].gameObject;
            }
        }
        return result;
    }

    private int SerchInArea(Vector3 AreaSize)
    {
        var center = transform.forward * (AreaSize.z * 0.5f + 0.1f) + transform.up * (AreaSize.y * 0.5f + 0.2f) + transform.position;
        var size = AreaSize;
        var rotation = transform.rotation;
        int count = Physics.OverlapBoxNonAlloc(center, size * 0.5f, overlapCollision, rotation);
        return count;
    }

    private bool JumpInToTarget(string tag)
    {
        if (stateManager == null || stateManager.cController == null)
        {
            Debug.LogWarning("StateManager or CharacterController is not assigned.");
            return false;
        }

        int count;
        // 攻撃範囲内をチェック
        count = SerchInArea(AttackAreaSize);
        if (count > 0)
        {
            // 攻撃範囲内にターゲットがいる場合は飛びつかない
            for (int i = 0; i < count; i++)
            {
                var col = overlapCollision[i];
                if (col != null && col.CompareTag(tag))
                {
                    return false;
                }
            }
        }

        // 飛びつき範囲内をチェック

        closestTarget = null;
        float closestDistance = Mathf.Infinity;

        //Debug.Log("飛びつき範囲をチェック: sctarget >" + sctarget + "/ sctarget.GetCurrentTarget() >" + sctarget.GetCurrentTarget());
        if (sctarget != null && sctarget.GetCurrentTarget() != null)
        {
            //ターゲット方向に身体を向ける
            Debug.Log("ターゲットに向く");
            this.transform.rotation = Quaternion.LookRotation(sctarget.GetCurrentTarget().transform.position);

            // 3) ターゲット優先の探索（TargetingJumpInAreaSize）
            count = SerchInArea(TargetingJumpInAreaSize);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var col = overlapCollision[i];
                    if (col == null || !col.CompareTag(tag)) continue;

                    // 優先: 現在のターゲットなら決定して抜ける
                    if (col.gameObject == sctarget.GetCurrentTarget())
                    {
                        closestTarget = col;
                        break;
                    }

                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = col;
                    }
                }
            }
        }
        else
        {
            // 4) ターゲットが居なければ通常の飛びつき範囲を探索
            if (closestTarget == null)
            {
                count = SerchInArea(JumpInAreaSize);
                if (count > 0)
                {
                    closestDistance = Mathf.Infinity;
                    for (int i = 0; i < count; i++)
                    {
                        var col = overlapCollision[i];
                        if (col == null || !col.CompareTag(tag)) continue;

                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTarget = col;
                        }
                    }
                }
            }
        }

        if (closestTarget != null)
        {
            return true;
        }
        return false;
    }

    // Combo関連のメソッド
    public void IncrementCombo()
    {
        currentComboCount++;
        currentComboTime = comboResetTime; // コンボリセットのタイマーをリセット
        Debug.Log("Combo : " + currentComboCount);
    }

    public void ResetCombo()
    {
        currentComboCount = 0;
        Debug.Log("Combo Reset");
    }

    public int GetCurrentComboCount()
    {
        return currentComboCount;
    }

    // Gizmosを使って攻撃範囲を可視化===============================================================================
    private void OnDrawGizmosSelected()
    {
        // 半透明の塗りとワイヤーで表示

        //赤 = 攻撃範囲
        var center = transform.forward * (AttackAreaSize.z * 0.5f + 0.1f) + transform.up * (AttackAreaSize.y * 0.5f + 0.2f) + transform.position;
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawCube(center, AttackAreaSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, AttackAreaSize);

        //黄色 = 飛びつき範囲
        var JumpInCenter = transform.forward * (JumpInAreaSize.z * 0.5f + 0.1f) + transform.up * (JumpInAreaSize.y * 0.5f + 0.2f) + transform.position;
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(JumpInCenter, JumpInAreaSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(JumpInCenter, JumpInAreaSize);

        //緑 = ターゲット時の飛びつき範囲
        var TargetingJumpInCenter = transform.forward * (TargetingJumpInAreaSize.z * 0.5f + 0.1f) + transform.up * (TargetingJumpInAreaSize.y * 0.5f + 0.2f) + transform.position;
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(TargetingJumpInCenter, TargetingJumpInAreaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(TargetingJumpInCenter, TargetingJumpInAreaSize);
    }
}
