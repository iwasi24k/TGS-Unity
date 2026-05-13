using UnityEngine;
using UnityEngine.InputSystem;

public enum AttackType
{
    Weak1,
    Weak2,
    Strong,
    Uppercut
}

public class SC_PlayerAttack : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("攻撃用インプットアクション(弱)")]
    [SerializeField] private InputActionReference iaWeakAttack;
    [Tooltip("攻撃用インプットアクション(強)")]
    [SerializeField] private InputActionReference iaStrongAttack;
    [Tooltip("ターゲット情報"), SerializeField] SC_PlayerTarget scTarget;
    [Tooltip("キャラクターコントローラー"),SerializeField] CharacterController ccPlayer;
    [Tooltip("ポーズ用オブジェクト"), SerializeField] SC_Setting scSetting;

    [Header("Settings")]
    [Tooltip("攻撃のクールダウン時間")]
    [SerializeField] private float attackCooldown = 0.5f;
    [Tooltip("攻撃のダメージ量(弱)")]
    [SerializeField] private int weakAttackDamage = 10;
    [Tooltip("攻撃のダメージ量(強)")]
    [SerializeField] private int strongAttackDamage = 20;
    [Tooltip("攻撃範囲"), SerializeField] private Vector3 AttackAreaSize = new Vector3(2f,2f,3f);
    [Tooltip("飛びつきの範囲")]
    [SerializeField] private Vector3 JumpInAreaSize = new Vector3(3f, 2f, 5f);
    [Tooltip("ターゲット時の飛びつきの範囲")]
    [SerializeField] private Vector3 TargetingJumpInAreaSize = new Vector3(3f, 2f, 10f);

    private float currentAttackCooldown = 0f;
    private readonly Collider[] overlapCollision = new Collider[32];

    //アクションコンボ
    private enum AttackInputType
    {
        Weak,
        Strong
    }

    [System.Serializable]
    private class AttackData
    {
        public AttackType attackType;
        public int damage = 10;
        public Vector3 areaSize = new Vector3(2f, 2f, 3f);
        public bool blowAway = false;
    }

    [Header("Combo Settings")]
    [Tooltip("コンボがリセットされるまでの時間")]
    [SerializeField] private float comboResetTime = 2.0f;

    [Tooltip("弱1段目")]
    [SerializeField]
    private AttackData weak1Attack = new AttackData
    {
        attackType = AttackType.Weak1,
        damage = 10,
        areaSize = new Vector3(2f, 2f, 3f),
        blowAway = false
    };

    [Tooltip("弱2段目")]
    [SerializeField]
    private AttackData weak2Attack = new AttackData
    {
        attackType = AttackType.Weak2,
        damage = 10,
        areaSize = new Vector3(2f, 2f, 3f),
        blowAway = false
    };

    [Tooltip("Weak → Weak → Weak の攻撃")]
    [SerializeField]
    private AttackData strongComboAttack = new AttackData
    {
        attackType = AttackType.Strong,
        damage = 20,
        areaSize = new Vector3(2f, 2f, 3f),
        blowAway = true
    };

    [Tooltip("Weak → Weak → Strong の攻撃")]
    [SerializeField]
    private AttackData uppercutComboAttack = new AttackData
    {
        attackType = AttackType.Uppercut,
        damage = 35,
        areaSize = new Vector3(2f, 2f, 3f),
        blowAway = true
    };

    [Tooltip("通常の強攻撃")]
    [SerializeField]
    private AttackData normalStrongAttack = new AttackData
    {
        attackType = AttackType.Strong,
        damage = 20,
        areaSize = new Vector3(2f, 2f, 3f),
        blowAway = true
    };

    private int weakComboCount = 0;
    private float comboTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ccPlayer == null) ccPlayer = GetComponent<CharacterController>();

        if (scTarget == null) scTarget = this.GetComponent<SC_PlayerTarget>();

        if(scSetting == null) scSetting = GameObject.FindGameObjectWithTag("Setting").GetComponent<SC_Setting>();

        if (iaWeakAttack == null)
        {
            Debug.LogError("弱攻撃のInputActionReferenceがアタッチされていません。");
        }
        if (iaStrongAttack == null) 
        {
            Debug.LogError("強攻撃のInputActionReferenceがアタッチされていません。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(scSetting != null && scSetting.IsPaused())
        {
            return; // ポーズ中は攻撃できないようにする
        }

        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");

        var weakAttackInput = iaWeakAttack.action.WasPressedThisFrame();
        var strongAttackInput = iaStrongAttack.action.WasPressedThisFrame();

        UpdateAttackCooldown();
        UpdateComboTimer();

        if (weakAttackInput)
        {
            TryAttackInput(AttackInputType.Weak);
        }

        if (strongAttackInput)
        {
            TryAttackInput(AttackInputType.Strong);
        }
    }

    // SearchAreaSize: 飛びつきの範囲, AttackAreaSize: 攻撃の範囲　強弱で差をつけたい場合を考慮して両方渡すようにしています。
    private void JumpInEnemy(Vector3 SearchAreaSize, Vector3 AttackAreaSize)
    {
        {
            // 攻撃範囲内に敵がいるかチェック
            var attackCenter = transform.forward * (AttackAreaSize.z * 0.5f) + transform.up * (AttackAreaSize.y * 0.5f) + transform.position;
            int attackHitCount = Physics.OverlapBoxNonAlloc(
                attackCenter,
                AttackAreaSize * 0.5f,
                overlapCollision,
                transform.rotation
            );

            // 攻撃範囲に敵が1体でもいれば飛びつかない
            for (int a = 0; a < attackHitCount; a++)
            {
                var col = overlapCollision[a];
                if (col != null && col.CompareTag("Enemy"))
                {
                    return;
                }
            }
        }

        {
            //敵の位置にジャンプする処理
            var center = transform.forward * (SearchAreaSize.z * 0.5f) + transform.up * (SearchAreaSize.y * 0.5f) + transform.position;
            int hitCount = Physics.OverlapBoxNonAlloc(
                 center,
                 SearchAreaSize * 0.5f,
                 overlapCollision,
                 transform.rotation
            );

            Collider closest = null;
            float minSqrDist = float.MaxValue;
            Vector3 selfPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = overlapCollision[i];
                if (hit == null) continue;
                if (!hit.CompareTag("Enemy")) continue;

                // BlownAway状態の敵には飛びつかない
                SC_EnemyStatusManager enemy = hit.GetComponent<SC_EnemyStatusManager>();
                if (enemy != null && enemy.IsBlownAway())
                {
                    continue;
                }

                Vector3 toEnemy = hit.transform.position - selfPos;
                toEnemy.y = 0f; // 水平距離で比較
                float sqr = toEnemy.sqrMagnitude;
                if (sqr < minSqrDist)
                {
                    minSqrDist = sqr;
                    closest = hit;
                }
            }

            if (closest == null)
            {
                // 対象なし
                return;
            }

            // 最も近い敵に飛びつく
            var directionToEnemy = (closest.transform.position - transform.position);
            directionToEnemy.y = 0f;
            if (directionToEnemy.sqrMagnitude <= 0.0001f) return;
            directionToEnemy.Normalize();

            // プレイヤーが敵に到達しすぎないよう攻撃範囲の中心に配置
            var jumpPosition = closest.transform.position - directionToEnemy * (AttackAreaSize.z * 0.5f);

            // 移動（CharacterController があれば 移動）
            if (ccPlayer != null)
            {
                var moveVector = jumpPosition - transform.position;
                ccPlayer.Move(moveVector);
                // ジャンプ後にプレイヤーが敵を向くように回転
                if (moveVector.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(directionToEnemy);
                    transform.rotation = targetRot;
                }
            }

        }
    }

    private void AttackExe(int AttackDamage, Vector3 AreaSize, bool BlowAway, AttackType attackType)
    {

        var center = transform.forward * (AreaSize.z * 0.5f) + transform.up * (AreaSize.y * 0.5f) + transform.position;
        int HitCount = Physics.OverlapBoxNonAlloc(
             center,
             AreaSize * 0.5f,
             overlapCollision,
             transform.rotation
        );

        bool hasHitEnemy = false;

        for (int i = 0; i < HitCount; i++)
        {
            var hit = overlapCollision[i];
            if (hit.CompareTag("Enemy"))
            {
                // BlownAway状態の敵にはダメージを与えない
                SC_EnemyStatusManager enemy = hit.GetComponent<SC_EnemyStatusManager>();
                if (enemy.IsBlownAway())
                {
                    continue;
                }

                enemy.TakeDamage(AttackDamage, transform.position, BlowAway, attackType);
                hasHitEnemy = true;
            }
        }

        if (hasHitEnemy && currentAttackCooldown <= 0f)
        {
            currentAttackCooldown = attackCooldown;
        }
        else
        {
            currentAttackCooldown = attackCooldown * 0.5f; // 敵に当たらなかった場合はクールダウンを短くするなどの調整も可能
        }
    }

    // Scene上でこのオブジェクトが選択されているときに攻撃範囲を可視化
    private void OnDrawGizmosSelected()
    {
        // 半透明の塗りとワイヤーで表示

        //赤 = 攻撃範囲
        var center = transform.forward * (AttackAreaSize.z * 0.5f) + transform.up * (AttackAreaSize.y * 0.5f) + transform.position;
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawCube(center, AttackAreaSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, AttackAreaSize);

        //黄色 = 飛びつき範囲
        var JumpInCenter = transform.forward * (JumpInAreaSize.z * 0.5f) + transform.up * (JumpInAreaSize.y * 0.5f) + transform.position;
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(JumpInCenter, JumpInAreaSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(JumpInCenter, JumpInAreaSize);

        //緑 = ターゲット時の飛びつき範囲
        var TargetingJumpInCenter = transform.forward * (TargetingJumpInAreaSize.z * 0.5f) + transform.up * (TargetingJumpInAreaSize.y * 0.5f) + transform.position;
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(TargetingJumpInCenter, TargetingJumpInAreaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(TargetingJumpInCenter, TargetingJumpInAreaSize);
    }

    //クールダウン
    private void UpdateAttackCooldown()
    {
        if (currentAttackCooldown > 0f)
        {
            currentAttackCooldown -= Time.deltaTime;
        }
    }

    //アクションコンボ

    //コンボタイマー
    private void UpdateComboTimer()
    {
        if (comboTimer <= 0f)
        {
            return;
        }

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
        {
            ResetCombo();
        }
    }

    //アクション入力
    private void TryAttackInput(AttackInputType inputType)
    {
        if (currentAttackCooldown > 0f)
        {
            return;
        }

        AttackData attackData = GetAttackDataByInput(inputType);

        if (attackData == null)
        {
            return;
        }

        ExecuteAttackData(attackData);

        UpdateComboState(inputType, attackData);
    }

    //入力変換
    private AttackData GetAttackDataByInput(AttackInputType inputType)
    {
        switch (inputType)
        {
            case AttackInputType.Weak:
                return GetAttackDataByWeakInput();

            case AttackInputType.Strong:
                return GetAttackDataByStrongInput();
        }

        return null;
    }

    //Weak入力時の分岐
    private AttackData GetAttackDataByWeakInput()
    {
        // Weak → Weak → Weak
        // 3回目のWeak入力はStrong攻撃になる
        if (weakComboCount >= 2)
        {
            Debug.Log("PlayerAttack:3回目のWeak→Strong");
            return strongComboAttack;
        }

        // 1回目のWeak
        if (weakComboCount == 0)
        {
            Debug.Log("PlayerAttack:1回目のWeak");
            return weak1Attack;
        }

        // 2回目のWeak
        if (weakComboCount == 1)
        {
            Debug.Log("PlayerAttack:2回目のWeak");
            return weak2Attack;
        }

        return weak1Attack;
    }

    //Strong入力時の分岐
    private AttackData GetAttackDataByStrongInput()
    {
        if (weakComboCount >= 2)
        {
            Debug.Log("PlayerAttack:Uppercut");
            return uppercutComboAttack;
        }

        Debug.Log("PlayerAttack:1回目のStrong");
        return normalStrongAttack;
    }

    //アクション実行
    private void ExecuteAttackData(AttackData attackData)
    {
        Vector3 jumpInSize = scTarget.GetCurrentTarget() != null
            ? TargetingJumpInAreaSize
            : JumpInAreaSize;

        JumpInEnemy(jumpInSize, attackData.areaSize);
        AttackExe(attackData.damage, attackData.areaSize, attackData.blowAway, attackData.attackType);
    }

    //コンボ状態更新
    private void UpdateComboState(AttackInputType inputType, AttackData attackData)
    {
        // Weak入力で、まだコンボ途中ならカウントを進める
        if (inputType == AttackInputType.Weak &&
            (attackData.attackType == AttackType.Weak1 || attackData.attackType == AttackType.Weak2))
        {
            weakComboCount++;
            comboTimer = comboResetTime;
            return;
        }

        // Strong攻撃、VeryStrong攻撃、通常Strong攻撃を出したらコンボ終了
        ResetCombo();
    }

    //コンボリセット
    private void ResetCombo()
    {
        Debug.Log("PlayerAttack:コンボリセット");
        weakComboCount = 0;
        comboTimer = 0f;
    }
}
