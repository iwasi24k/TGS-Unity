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
    private int MaxHP = 100; //最大HPを定数で定義

    [Header("State")]
    [Tooltip("Stateのリスト"),SerializeField] private SC_EnemyBaceState[] stateList;
    [Tooltip("初期状態のStateの配列番号"),SerializeField] private int initialStateNum;
    [Tooltip("吹っ飛びのState"),SerializeField] private SC_EnemyBaceState blowAwayState;

    [Header("衝突判定円")]
    [Tooltip("敵同士の衝突判定円中心"), SerializeField] private Vector3 collisionCenter = Vector3.zero;
    [Tooltip("敵同士の衝突判定円半径"),SerializeField] private float collisionRadius = 0.5f;
    [Tooltip("敵同士の衝突時の吹っ飛びの威力"), SerializeField] private float blowAwayPowerOnCollision = 0.5f;
    [Tooltip("サーチの角度"), SerializeField] private float searchAngleThreshold = 30f;
    [Tooltip("敵同士の衝突最低速度"), SerializeField] private float minCollisionSpeed = 1.0f;
    [Tooltip("敵同士の衝突クールタイム"), SerializeField] private float enemyCollisionCooldown = 0.5f;

    //----------------------------------------------------------
    [Header("Boss / Special Setting")]
    [Tooltip("この敵が吹っ飛び状態になるかどうか"),SerializeField] private bool canBlownAway = true;

    [Header("Boss Down")]
    [Tooltip("ボスがDownするStateのStateList番号"), SerializeField] private int bossDownStateIndex = 6;
    [Tooltip("この敵がボスDownを使うか"), SerializeField] private bool useBossDown = false;
    [Tooltip("Down中だけPlayer攻撃のダメージを受けるか"), SerializeField] private bool onlyTakePlayerDamageWhileDown = false;
    //----------------------------------------------------------

    // 相手ごとの再ヒット可能時間
    private Dictionary<GameObject, float> enemyCollisionTimers = new Dictionary<GameObject, float>();

    private SC_EnemyBaceState currentState;
    private SC_EnemyBaceState[] localStateList;
    private int currentStateIndex = 0;
    
    void Start()
    {
        localStateList = new SC_EnemyBaceState[stateList.Length];

        if (hpSlider == null)
        {
            Debug.LogError("HPスライダーがアタッチされていません。");
        }
        else
        {
            hpSlider.maxValue = hpSlider.value = HP;
        }

        //全ステートのインスタンス化し、アセットを直接いじらない形に変更
        for (int i = 0; i < stateList.Length; i++)
        {
            //Debug.Log("StateListの" + i + "番目のStateをインスタンス化" + "StateName : " + stateList[i].name);
            SC_EnemyBaceState newState = Instantiate(stateList[i]);
            localStateList[i] = newState;
        }

        //初期状態の設定、CurrentIndexを初期状態に合わせて変更
        currentState = localStateList[initialStateNum];
        currentState.Enter(this.gameObject,this);
    }

    void Update()
    {
        UpdateEnemyCollisionTimers();

        currentState.UpdateState(this.gameObject, this);
    }

    private void FixedUpdate()
    {
        if (currentState != null)
        {    
            currentState.FixedUpdateState(this.gameObject, this);
        }
    }

    void OnDestroy()
    {
        if (currentState != null)
        {
            currentState.Exit(this.gameObject, this);
        }

        for(int i = 0; i < localStateList.Length; i++)
        {
            if (localStateList[i] != null)
            {
                Destroy(localStateList[i]);
            }
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
        // Boss用：Down中以外はPlayer攻撃のダメージを無効化
        if (onlyTakePlayerDamageWhileDown &&
            damageSource == EnemyDamageSource.PlayerAttack &&
            !IsBossDown())
        {
            Debug.Log("BossはDown中ではないため、Player攻撃ダメージを無効化");
            return;
        }

        CollisionDamage(damage);

        if (HP < 0)
        {
            HP = 0;
            if (canBlownAway)
            {
                TransitionToBlownAway(damage, AttackerPosition, attackType);
            }
            else
            {
                Destroy(this.gameObject);
            }

            return;
        }
        else if (isBlowAway && canBlownAway) 
        {
            TransitionToBlownAway(damage, AttackerPosition, attackType);
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

    private void TransitionToBlownAway(float power,Vector3 attackerPosition, AttackType attackType)
    {
        if (!canBlownAway) return;

        SC_EnemyBlownAway blownAway = blowAwayState as SC_EnemyBlownAway;

        if (!IsBlownAway())
        {
            currentState.Exit(this.gameObject, this);

            Vector3 initialBlowDirection = (this.transform.position - attackerPosition).normalized;
            initialBlowDirection.y = 0.0f;
            initialBlowDirection.Normalize();

            Vector3 blowDirection = SearchForEnemyInDirection(initialBlowDirection, searchAngleThreshold);
            blowDirection.y = 0.0f;
            blowDirection.Normalize();

            blownAway.SetBlownAway(power, blowDirection, attackType);

            blownAway.Enter(this.gameObject, this);
            currentState = blownAway;
        }
    }

    public void ReturnFromBlownAway()
    {
        //もしHPが0以下なら、消滅する
        if(HP <= 0)
        {
            Debug.Log("HPが0以下のため、敵を消滅させます。");
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
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != this.gameObject)
            {
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                toEnemy.y = 0f;

                float angle = Vector3.Angle(direction, toEnemy);

                if (angle <= angleThreshold)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
        }
        
        if(closestEnemy != null)
        {
            //Debug.Log("サーチで敵を見つけました : " + closestEnemy.name);

            Vector3 blowDirection = (closestEnemy.transform.position - this.transform.position).normalized;
            return blowDirection;
        }
        else
        {
            //Debug.Log("サーチで敵が見つかりませんでした。");
            return direction; 
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

            RegisterEnemyCollision(otherEnemy);

            int myPower = (int)(mySpeed * blowAwayPowerOnCollision) + ComboManager.Instance.GetComboCount();

            TransitionToBlownAway(myPower, otherEnemy.transform.position, 0);
            CollisionDamage(myPower);

            SC_EnemyStatusManager otherStatusManager = otherEnemy.GetComponent<SC_EnemyStatusManager>();
            if (otherStatusManager != null)
            {
                // 相手側にも、自分との衝突を登録しておく
                otherStatusManager.RegisterEnemyCollision(this.gameObject);

                otherStatusManager.TransitionToBlownAway(myPower, this.transform.position, 0);
                otherStatusManager.CollisionDamage(myPower);
            }
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
        return currentState == blowAwayState;
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
}
