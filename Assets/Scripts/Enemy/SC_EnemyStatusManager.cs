using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//ダメージの種類
public enum EnemyDamageSource
{
    PlayerAttack,
    EnemyCollision
}

public class SC_EnemyStatusManager : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("HPSlider")]
    [SerializeField]  private Slider hpSlider;

    [Header("Enemy Status")]
    [SerializeField] private int HP = 100;
    private int MaxHP;

    [Header("State")]
    [Tooltip("Stateのリスト"),SerializeField] private SC_EnemyBaceState[] stateList;
    [Tooltip("初期状態のStateの配列番号"),SerializeField] private int initialStateNum;
    [Tooltip("吹っ飛びのState"),SerializeField] private SC_EnemyBaceState blowAwayState;
    private SC_EnemyBaceState localBlowAwayState;
    private AttackType stunAttackType;

    [Header("Stun")]
    [Tooltip("攻撃を受けた時に短時間止まるState")]
    [SerializeField] private SC_EnemyBaceState stunState;

    [Tooltip("Player攻撃を受けた時にStunするか")]
    [SerializeField] private bool useHitStun = true;

    private SC_EnemyBaceState localStunState;
    private SC_EnemyBaceState beforeStunState;

    [Header("Fall Death")]
    [Tooltip("一定Y座標より下に落ちたら死亡するか")]
    [SerializeField] private bool useFallDeath = true;

    [Tooltip("このY座標より下に落ちたら死亡")]
    [SerializeField] private float fallDeathY = -10.0f;

    [Header("衝突判定円")]
    [Tooltip("敵同士の衝突判定円中心"), SerializeField] private Vector3 collisionCenter = Vector3.zero;
    [Tooltip("敵同士の衝突判定円半径"),SerializeField] private float collisionRadius = 0.5f;
    [Tooltip("敵同士の衝突時のダメージ基数"), SerializeField] private int DamageOnCollision = 10;
    [Tooltip("連鎖数1つごとに加算されるダメージ"), SerializeField] private int chainDamageBonus = 5;
    [Tooltip("敵同士の衝突時の吹っ飛びの威力"), SerializeField] private float blowAwayPowerOnCollision = 0.5f;
    [Tooltip("サーチの角度"), SerializeField] private float searchAngleThreshold = 30f;
    [Tooltip("敵同士の衝突最低速度"), SerializeField] private float minCollisionSpeed = 1.0f;
    [Tooltip("敵同士の衝突クールタイム"), SerializeField] private float enemyCollisionCooldown = 0.5f;

    [Header("Fire Points")]
    [Tooltip("複数弾用の発射位置。FirePoint1～FirePoint12を登録する")]
    [SerializeField] private Transform[] firePointList;

    [Header("Move Look Target")]
    [Tooltip("Move中に進行方向へ向けたい子オブジェクトの制御スクリプト")]
    [SerializeField] private SC_MoveLookTarget moveLookTarget;

    [Header("BlowAway Follow Parts")]
    [SerializeField] private SC_PartFollowTarget[] blowAwayFollowParts;

    //----------------------------------------------------------
    [Header("Boss / Special Setting")]
    [Tooltip("この敵が吹っ飛び状態になるかどうか"),SerializeField] private bool canBlownAway = true;

    [Header("Boss Down")]
    [Tooltip("ボスがDownするStateのStateList番号"), SerializeField] private int bossDownStateIndex = 6;
    [Tooltip("この敵がボスDownを使うか"), SerializeField] private bool useBossDown = false;
    [Tooltip("Down中だけPlayer攻撃のダメージを受けるか"), SerializeField] private bool onlyTakePlayerDamageWhileDown = false;
    [Tooltip("1回のDown中に削れるHP量を制限するか"), SerializeField] private bool useBossDownDamageLimit = true;
    [Tooltip("ボスHPを何分割するか。4なら1回のDownで最大HPの1/4まで削れる"), SerializeField] private int bossHpPartCount = 4;
    
    [Header("Boss Shield")]
    [SerializeField] private bool isBoss = false;
    [Tooltip("ボスシールドを使うか"), SerializeField] private bool useBossShield = false;
    [Tooltip("ボスシールドの表示オブジェクト"), SerializeField] private GameObject bossShieldObject;
    [Tooltip("ボスシールドの最大値"), SerializeField] private int maxBossShield = 3;
    [Tooltip("現在のボスシールド値"), SerializeField] private int currentBossShield = 3;
    [Tooltip("シールドが0になった時にDownするか"), SerializeField] private bool downWhenShieldBreak = true;
    [Tooltip("シールドの稲妻演出"), SerializeField]
    private SC_ShieldLightningEffect shieldLightningEffect;
    private bool bossDefeatNotified = false;

    // 攻撃リスト
    private int[] currentBossAttackList;
    private int currentBossAttackListIndex;

    // BossDown中のダメージ制限用
    private int bossDownStartHP;
    private int bossDownHpLimit;
    private bool requestEndBossDown;

    [Header("Start Lock")]
    [SerializeField] private bool useStartLock = false;

    private bool stateStarted;

    //----------------------------------------------------------

    // 相手ごとの再ヒット可能時間
    private Dictionary<GameObject, float> enemyCollisionTimers = new Dictionary<GameObject, float>();

    private SC_EnemyBaceState currentState;
    private SC_EnemyBaceState[] localStateList;
    private int currentStateIndex = 0;

    //ボス戦カメラ用
    public int GetCurrentStateIndex() => currentStateIndex;

    void Start()

    {
        localStateList = new SC_EnemyBaceState[stateList.Length];

        if (useBossShield == false)
        {
            if (hpSlider == null)
            {
                Debug.LogError("HPスライダーがアタッチされていません。");
            }
            else
            {
                hpSlider.maxValue = hpSlider.value = HP;
            }
        }

        //全ステートのインスタンス化し、アセットを直接いじらない形に変更
        for (int i = 0; i < stateList.Length; i++)
        {
            //Debug.Log("StateListの" + i + "番目のStateをインスタンス化" + "StateName : " + stateList[i].name);
            SC_EnemyBaceState newState = Instantiate(stateList[i]);
            localStateList[i] = newState;
        }

        if (blowAwayState != null)
        {
            localBlowAwayState = Instantiate(blowAwayState);
        }

        if (stunState != null)
        {
            localStunState = Instantiate(stunState);
        }

        //初期状態の設定、CurrentIndexを初期状態に合わせて変更
        currentState = localStateList[initialStateNum];

        //HPの初期値をMaxHPに設定
        MaxHP = HP;

        // Boss Shieldの初期値を設定
        if (useBossShield)
        {
            currentBossShield = maxBossShield;
            SetBossShieldVisible(true);

            if (shieldLightningEffect == null && bossShieldObject != null)
            {
                shieldLightningEffect =
                    bossShieldObject.GetComponentInChildren<SC_ShieldLightningEffect>();
            }
        }

        stateStarted = false;

        if (!useStartLock || SC_EnemyStartGate.IsOpened)
        {
            StartCurrentState();
        }

        SC_EnemyManager enemyManager = FindFirstObjectByType<SC_EnemyManager>();

        if (enemyManager != null)
        {
            enemyManager.AddEnemy(gameObject);
        }
    }

    void Update()
    {
        if (IsStartLocked())
        {
            return;
        }

        if (!stateStarted)
        {
            StartCurrentState();
        }

        CheckFallDeath();

        UpdateEnemyCollisionTimers();

        currentState.UpdateState(this.gameObject, this);
    }

    private void FixedUpdate()
    {
        if (IsStartLocked())
        {
            return;
        }

        if (!stateStarted)
        {
            return;
        }

        if (currentState != null)
        {    
            currentState.FixedUpdateState(this.gameObject, this);
        }
    }

    void OnDestroy()
    {
        currentState = null;

        if (localStateList != null)
        {
            for (int i = 0; i < localStateList.Length; i++)
            {
                if (localStateList[i] != null)
                {
                    Destroy(localStateList[i]);
                }
            }
        }

        if (localBlowAwayState != null)
        {
            Destroy(localBlowAwayState);
        }

        if (localStunState != null)
        {
            Destroy(localStunState);
        }
    }

    /* : 以下、各ステータスの管理用関数。　外部から呼び出して仕様。 : */
    public int GetHP()
    {
        return HP;
    }

    public int GetMaxHP()
    {
        return MaxHP;
    }

    public void TakeDamage(int damage, Vector3 AttackerPosition, bool isBlowAway = false, AttackType attackType = 0, EnemyDamageSource damageSource = EnemyDamageSource.PlayerAttack)
    {
        // StartLock中は通常ダメージを受けない
        if (IsStartLocked())
        {
            Debug.Log("StartLock中のためダメージ無効 : " + gameObject.name);
            return;
        }

        // Boss用：Player攻撃の場合
        if (damageSource == EnemyDamageSource.PlayerAttack)
        {
            // Down中以外はHPダメージ無効
            if (onlyTakePlayerDamageWhileDown && !IsBossDown())
            {
                Debug.Log("BossはDown中ではないため、Player攻撃ダメージを無効化");
                return;
            }

        }

        CollisionDamage(damage);

        CheckBossDownDamageLimit();

        if (damageSource == EnemyDamageSource.PlayerAttack && HP > 0 && !isBlowAway)
        {
            ChangeToStun(attackType);
        }

        if (HP <= 0)
        {
            HP = 0;

            if (isBoss && !bossDefeatNotified)
            {
                bossDefeatNotified = true;

                if (SC_Field.Instance != null)
                {
                    SC_Field.Instance.NotifyBossDefeated();
                }
            }

            if (canBlownAway)
            {
                TransitionToBlownAway(damage, AttackerPosition, attackType, true);
            }
            else
            {
                Destroy(this.gameObject);
            }

            return;
        }

        if (isBlowAway && canBlownAway) 
        {
            TransitionToBlownAway(damage, AttackerPosition, attackType, true);
        }
    }

    public void TransitionToNext()
    {
        if (currentState != null)
        {
            currentState.Exit(this.gameObject, this);
        }
        currentStateIndex = (currentStateIndex + 1) % localStateList.Length; //次のステートに移行、ループする形
        currentState = localStateList[currentStateIndex];
        currentState.Enter(this.gameObject, this);
    }

    private void TransitionToBlownAway(
     float power,
     Vector3 attackerPosition,
     AttackType attackType,
     bool exitCurrentState)
    {
        if (!canBlownAway) return;

        SC_EnemyBlownAway blownAway = localBlowAwayState as SC_EnemyBlownAway;

        if (blownAway == null)
        {
            Debug.LogError("localBlowAwayState が SC_EnemyBlownAway ではありません : " + gameObject.name);
            return;
        }

        if (IsBlownAway()) return;

        // ここが重要：
        // StartLock中でEnterしていないStateならExitしない
        if (exitCurrentState && currentState != null)
        {
            currentState.Exit(this.gameObject, this);
        }

        Vector3 initialBlowDirection = transform.position - attackerPosition;
        initialBlowDirection.y = 0.0f;

        if (initialBlowDirection.sqrMagnitude <= 0.0001f)
        {
            initialBlowDirection = transform.forward;
        }

        initialBlowDirection.Normalize();

        Vector3 blowDirection = SearchForEnemyInDirection(
            initialBlowDirection,
            searchAngleThreshold
        );

        blowDirection.y = 0.0f;

        if (blowDirection.sqrMagnitude <= 0.0001f)
        {
            blowDirection = initialBlowDirection;
        }

        blowDirection.Normalize();

        stunAttackType = attackType;

        blownAway.SetBlownAway(power, blowDirection);

        currentState = blownAway;
        stateStarted = true;

        blownAway.Enter(this.gameObject, this);
    }

    public void ReturnFromBlownAway()
    {
        //もしHPが0以下なら、消滅する
        if(HP <= 0)
        {
            Debug.Log("HPが0以下のため、敵を消滅させます。");
            SC_EffectManager.Instance.PlayEffect("Explosion", this.transform.position);
            Destroy(this.gameObject);
            return;
        }

        if (currentState != null)
        {
            currentState.Exit(this.gameObject, this);
        }
        currentState = localStateList[currentStateIndex];
        currentState.Enter(this.gameObject, this);
    }

    //サーチ(座標方向から30度以内にいる敵を探す)
    public Vector3 SearchForEnemyInDirection(Vector3 direction, float angleThreshold)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject closestNormalEnemy = null;
        float closestNormalDistance = Mathf.Infinity;

        GameObject closestBossShieldEnemy = null;
        float closestBossShieldDistance = Mathf.Infinity;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return transform.forward;
        }

        direction.Normalize();

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy == this.gameObject) continue;

            Vector3 toEnemy = enemy.transform.position - transform.position;
            toEnemy.y = 0f;

            float distance = toEnemy.magnitude;

            if (distance <= 0.0001f) continue;

            Vector3 toEnemyDir = toEnemy.normalized;

            float angle = Vector3.Angle(direction, toEnemyDir);

            if (angle > angleThreshold)
            {
                continue;
            }

            SC_EnemyStatusManager enemyStatus = enemy.GetComponent<SC_EnemyStatusManager>();

            if (enemyStatus == null)
            {
                enemyStatus = enemy.GetComponentInParent<SC_EnemyStatusManager>();
            }

            if (enemyStatus != null && enemyStatus.UseBossShield())
            {
                if (distance < closestBossShieldDistance)
                {
                    closestBossShieldDistance = distance;
                    closestBossShieldEnemy = enemy;
                }

                continue;
            }

            if (distance < closestNormalDistance)
            {
                closestNormalDistance = distance;
                closestNormalEnemy = enemy;
            }
        }

        if (closestBossShieldEnemy != null)
        {
            Vector3 blowDirection =
                closestBossShieldEnemy.transform.position - transform.position;

            blowDirection.y = 0f;

            if (blowDirection.sqrMagnitude > 0.0001f)
            {
                return blowDirection.normalized;
            }
        }

        if (closestNormalEnemy != null)
        {
            Vector3 blowDirection =
                closestNormalEnemy.transform.position - transform.position;

            blowDirection.y = 0f;

            if (blowDirection.sqrMagnitude > 0.0001f)
            {
                return blowDirection.normalized;
            }
        }

        return direction;
    }

    public void ChangeToStun(AttackType attackType)
    {
        if (!useHitStun) return;
        if (localStunState == null) return;

        // 吹っ飛び中、BossDown中はStunにしない
        if (IsBlownAway()) return;
        if (IsBossDown()) return;

        // すでにStun中なら入り直さない
        if (currentState == localStunState) return;

        stunAttackType = attackType;

        beforeStunState = currentState;

        if (currentState != null)
        {
            currentState.Exit(this.gameObject, this);
        }

        currentState = localStunState;
        currentState.Enter(this.gameObject, this);
    }

    public void ReturnFromStun()
    {
        if (currentState != null)
        {
            currentState.Exit(this.gameObject, this);
        }

        if (beforeStunState != null)
        {
            currentState = beforeStunState;
        }
        else
        {
            currentState = localStateList[currentStateIndex];
        }

        beforeStunState = null;

        if (currentState != null)
        {
            currentState.Enter(this.gameObject, this);
        }
    }

    //敵同士の衝突判定
    public void CheckCollisionWithOtherEnemies()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + collisionCenter, collisionRadius);
        Rigidbody myRb = GetComponent<Rigidbody>();
        float mySpeed = (myRb != null) ? myRb.linearVelocity.magnitude : 0f;

        if (mySpeed < minCollisionSpeed)
        {
            return;
        }

        foreach (var hitCollider in hitColliders)
        {
            GameObject otherEnemy = hitCollider.gameObject;

            if (otherEnemy == this.gameObject) continue;
            if (!otherEnemy.CompareTag("Enemy")) continue;

            // 同じ敵に連続ヒットしないようにする
            if (!CanHitEnemyCollision(otherEnemy)) continue;

            SC_EnemyStatusManager otherStatusManager = otherEnemy.GetComponent<SC_EnemyStatusManager>();

            if (otherStatusManager == null) continue;

            RegisterEnemyCollision(otherEnemy);
            otherStatusManager.RegisterEnemyCollision(this.gameObject);
         
            int myPower = (int)(mySpeed * blowAwayPowerOnCollision) + ComboManager.Instance.GetComboCount();
            
            int chainCount = ComboManager.Instance.GetComboCount();
            int chainBonusCount = Mathf.Max(0, chainCount - 1);

            int damage = DamageOnCollision + chainBonusCount * chainDamageBonus;

            // 相手がシールド持ちボス
            if (otherStatusManager.UseBossShield())
            {
                // シールドが残っているならシールドダメージ
                if (otherStatusManager.HasBossShield())
                {
                    otherStatusManager.TakeBossShieldDamage(damage);
                }

                // シールドが無く、Down中ならHPダメージ
                else if (otherStatusManager.IsBossDown())
                {
                    otherStatusManager.TakeDamage(
                        damage,
                        this.transform.position,
                        false,
                        0,
                        EnemyDamageSource.EnemyCollision
                    );
                }

                // ボスは吹っ飛ばさない
                // 飛ばされた自分だけ衝突後の処理
                TransitionToBlownAway(
                    myPower,
                    otherEnemy.transform.position,
                    0, 
                    true
                );

                CollisionDamage(damage);

                continue;
            }


            // ここから普通の敵同士の衝突処理
            TransitionToBlownAway(myPower, otherEnemy.transform.position, 0, true);
            CollisionDamage(damage);

            otherStatusManager.TransitionToBlownAway(
                myPower,
                this.transform.position,
                0, 
                true
            );

            otherStatusManager.CollisionDamage(damage);

        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (currentState != null)
        {
            currentState.OnCollisionEnterState(this.gameObject, this, collision);
        }
    }

    // Scene上でこのオブジェクトが選択されているときに攻撃範囲を可視化
    private void OnDrawGizmosSelected()
    {
        // 敵同士の衝突判定円を描画
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + collisionCenter, collisionRadius);

        // サーチの角度を描画
        Gizmos.color = Color.blue;
        Vector3 forward = transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, searchAngleThreshold, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -searchAngleThreshold, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * 2f);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * 2f);

        if (Application.isPlaying)
        {
            if (currentState != null)
            {
                currentState.OnDrawGizmosSelectedState(this.gameObject, this);
            }
        }
        else
        {
            if (stateList != null)
            {
                for (int i = 0; i < stateList.Length; i++)
                {
                    if (stateList[i] != null)
                    {
                        stateList[i].OnDrawGizmosSelectedState(this.gameObject, this);
                    }
                }
            }
        }
    }

    //衝突ダメージを与える関数
    private void CollisionDamage(int damage)
    {
        HP -= damage;

        if (hpSlider != null)
        {
            hpSlider.value = HP;
        }
    }

    //もし敵がBlownAway状態の時に、tureを返す関数
    public bool IsBlownAway()
    {
        return currentState is SC_EnemyBlownAway;
    }

    //タイマー更新
    private void UpdateEnemyCollisionTimers()
    {
        if (enemyCollisionTimers.Count == 0) return;

        List<GameObject> keys = new List<GameObject>(enemyCollisionTimers.Keys);
        List<GameObject> removeList = new List<GameObject>();

        foreach (GameObject enemy in keys)
        {
            if (enemy == null)
            {
                removeList.Add(enemy);
                continue;
            }

            float time = enemyCollisionTimers[enemy] - Time.deltaTime;

            if (time <= 0.0f)
            {
                removeList.Add(enemy);
            }
            else
            {
                enemyCollisionTimers[enemy] = time;
            }
        }

        foreach (GameObject enemy in removeList)
        {
            enemyCollisionTimers.Remove(enemy);
        }
    }

    private bool CanHitEnemyCollision(GameObject otherEnemy)
    {
        if (otherEnemy == null) return false;

        return !enemyCollisionTimers.ContainsKey(otherEnemy);
    }

    private void RegisterEnemyCollision(GameObject otherEnemy)
    {
        if (otherEnemy == null) return;

        enemyCollisionTimers[otherEnemy] = enemyCollisionCooldown;
    }


    public void ResetEnemyStatus()
    {
        HP = MaxHP;
    }
    
    public void SetHP(int hp)
    {
        HP = hp;
        MaxHP = hp;


        if (hpSlider != null)
        {
            hpSlider.maxValue = MaxHP;
            hpSlider.value = HP;
        }
    }


    //Stateを変更する関数、StateListの配列番号で指定
    public void ChangeState(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= localStateList.Length)
        {
            Debug.LogError("存在しないState番号です : " + stateIndex);
            return;
        }

        if (currentState != null)
        {
            currentState.Exit(this.gameObject, this);
        }

        currentStateIndex = stateIndex;
        currentState = localStateList[currentStateIndex];
        currentState.Enter(this.gameObject, this);
    }

    // BossDown状態に移行する関数
    public void TriggerBossDown()
    {
        if (!useBossDown) return;

        ChangeState(bossDownStateIndex);
    }

    // BossDown状態かどうかを返す関数
    public bool IsBossDown()
    {
        return currentState is SC_BossDownState;
    }

    // Bossの攻撃リストを開始する関数
    public void StartBossAttackList(int[] attackList)
    {
        if (attackList == null || attackList.Length == 0)
        {
            ChangeState(0);
            return;
        }

        currentBossAttackList = attackList;
        currentBossAttackListIndex = 0;

        ChangeState(currentBossAttackList[currentBossAttackListIndex]);
    }

    // Bossの攻撃リストの次の攻撃に移行する関数
    public void ChangeNextBossAttackInList()
    {
        if (currentBossAttackList == null || currentBossAttackList.Length == 0)
        {
            ChangeState(0);
            return;
        }

        currentBossAttackListIndex++;

        if (currentBossAttackListIndex >= currentBossAttackList.Length)
        {
            currentBossAttackList = null;
            currentBossAttackListIndex = 0;

            ChangeState(0);
            return;
        }

        ChangeState(currentBossAttackList[currentBossAttackListIndex]);
    }

    // Bossの攻撃リストをクリアする関数
    public void ClearBossAttackList()
    {
        currentBossAttackList = null;
        currentBossAttackListIndex = 0;
    }

    // Bossシールドにダメージを与える関数
    public void TakeBossShieldDamage(int damage)
    {
        if (!useBossShield) return;
        if (damage <= 0) return;

        // すでにDown中ならシールドは減らさない
        if (IsBossDown()) return;

        currentBossShield -= damage;

        if (currentBossShield < 0)
        {
            currentBossShield = 0;
        }

        if (shieldLightningEffect != null)
        {
            shieldLightningEffect.PlayHitEffect();
        }

        Debug.Log("Boss Shield : " + currentBossShield + " / " + maxBossShield);

        if (currentBossShield <= 0)
        {
            SetBossShieldVisible(false);
            OnBossShieldBreak();
        }
    }

    // Bossシールドが0になったときの処理
    private void OnBossShieldBreak()
    {
        if (!downWhenShieldBreak) return;

        TriggerBossDown();
    }

    // Bossシールドをリセットする関数
    public void ResetBossShield()
    {
        if (!useBossShield) return;

        currentBossShield = maxBossShield;

        SetBossShieldVisible(true);

        Debug.Log("Boss Shield Reset : " + currentBossShield);
    }

    // Bossシールドを使うかどうかを返す関数
    public bool UseBossShield()
    {
        return useBossShield;
    }

    // Bossシールドが現在有効かどうかを返す関数
    public bool HasBossShield()
    {
        return useBossShield && currentBossShield > 0;
    }

    // Bossシールドの表示オブジェクトの表示・非表示を切り替える関数
    public void SetBossShieldVisible(bool visible)
    {
        if (bossShieldObject == null) return;

        bossShieldObject.SetActive(visible);
    }

    // BossDown中のダメージ上限を開始する関数
    public void BeginBossDownDamageLimit()
    {
        if (!useBossDownDamageLimit) return;

        bossHpPartCount = Mathf.Max(1, bossHpPartCount);

        bossDownStartHP = HP;

        int damageLimit = Mathf.CeilToInt((float)MaxHP / bossHpPartCount);

        bossDownHpLimit = bossDownStartHP - damageLimit;

        if (bossDownHpLimit < 0)
        {
            bossDownHpLimit = 0;
        }

        requestEndBossDown = false;

        Debug.Log("Down中ダメージ上限 HP : " + bossDownStartHP + " -> " + bossDownHpLimit);
    }

    // BossDown中のダメージ上限をチェックする関数。上限を超えていたら、trueを返す
    public bool IsRequestEndBossDown()
    {
        return requestEndBossDown;
    }

    // BossDown中のダメージ上限をチェックして、必要ならフラグを立てる関数
    public void ClearRequestEndBossDown()
    {
        requestEndBossDown = false;
    }

    // BossDown中のダメージ上限をチェックして、HPを制限する関数
    private void CheckBossDownDamageLimit()
    {
        if (!useBossDownDamageLimit) return;
        if (!IsBossDown()) return;

        if (HP <= bossDownHpLimit)
        {
            HP = bossDownHpLimit;

            if (hpSlider != null)
            {
                hpSlider.value = HP;
            }

            requestEndBossDown = true;

            Debug.Log("Down中の1ゲージ分ダメージ到達。Downを終了します");
        }
    }

    private void CheckFallDeath()
    {
        if (!useFallDeath) return;

        if (transform.position.y <= fallDeathY)
        {
            FallDeath();
        }
    }

    private void FallDeath()
    {
        Debug.Log("敵が落下死しました : " + gameObject.name);

        if (SC_EffectManager.Instance != null)
        {
            SC_EffectManager.Instance.PlayEffect("Explosion", transform.position);
        }

        Destroy(gameObject);
    }

    public int GetCurrentBossShield()
    {
        return currentBossShield;
    }

    public int GetMaxBossShield()
    {
        return maxBossShield;
    }

    private void StartCurrentState()
    {
        if (stateStarted) return;
        if (currentState == null) return;

        stateStarted = true;
        currentState.Enter(this.gameObject, this);
    }

    public void ForceBlowAway(
        float power,
        Vector3 attackerPosition,
        AttackType attackType = 0,
        bool unlockStartLock = true)
    {
        Debug.Log("ForceBlowAway called : " + gameObject.name);

        // 変更前の状態を保存
        bool wasStateStarted = stateStarted;

        if (unlockStartLock)
        {
            useStartLock = false;
        }

        if (!canBlownAway)
        {
            Debug.Log("canBlownAway が false です : " + gameObject.name);
            return;
        }

        if (localBlowAwayState == null)
        {
            Debug.LogError("BlowAwayState が設定されていません : " + gameObject.name);
            return;
        }

        if (currentState == null)
        {
            currentStateIndex = initialStateNum;
            currentState = localStateList[currentStateIndex];
        }

        // StartLock中でEnterされていなかったEnemyなら、
        // currentState.Exit() は呼ばない
        TransitionToBlownAway(
            power,
            attackerPosition,
            attackType,
            wasStateStarted
        );
    }

    public void UnlockStartLock()
    {
        useStartLock = false;
        stateStarted = true;
    }

    public bool IsStartLocked()
    {
        return useStartLock && !SC_EnemyStartGate.IsOpened;
    }

    public AttackType GetStunAttackType()
    {
        return stunAttackType;
    }

    public Transform[] GetFirePointList()
    {
        return firePointList;
    }

    public Transform GetFirePoint(int index)
    {
        if (firePointList == null) return null;
        if (index < 0 || index >= firePointList.Length) return null;

        return firePointList[index];
    }

    public int GetFirePointCount()
    {
        if (firePointList == null) return 0;

        return firePointList.Length;
    }

    public SC_MoveLookTarget GetMoveLookTarget()
    {
        return moveLookTarget;
    }

    public void SetBlowAwayFollowPartsEnabled(bool enabled)
    {
        if (blowAwayFollowParts == null) return;

        for (int i = 0; i < blowAwayFollowParts.Length; i++)
        {
            if (blowAwayFollowParts[i] == null) continue;

            blowAwayFollowParts[i].SetFollowEnabled(enabled);
        }
    }
}
