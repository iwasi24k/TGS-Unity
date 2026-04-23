using UnityEngine;
using UnityEngine.UI;

public class SC_EnemyStatusManager : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("HPSlider")]
    [SerializeField]  private Slider hpSlider;

    [Header("Enemy Status")]
    [SerializeField] private int HP = 100;

    [Header("State")]
    [Tooltip("Stateのリスト"),SerializeField] private SC_EnemyBaceState[] stateList;
    [Tooltip("初期状態のStateの配列番号"),SerializeField] private int initialStateNum;
    [Tooltip("吹っ飛びのState"),SerializeField] private SC_EnemyBaceState blowAwayState;

    [Header("衝突判定円")]
    [Tooltip("敵同士の衝突判定円中心"), SerializeField] private Vector3 collisionCenter = Vector3.zero;
    [Tooltip("敵同士の衝突判定円半径"),SerializeField] private float collisionRadius = 0.5f;
    [Tooltip("敵同士の衝突時の吹っ飛びの威力"), SerializeField] private float blowAwayPowerOnCollision = 50f;
    [Tooltip("サーチの角度"), SerializeField] private float searchAngleThreshold = 30f;

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
            Debug.Log("StateListの" + i + "番目のStateをインスタンス化" + "StateName : " + stateList[i].name);
            SC_EnemyBaceState newState = Instantiate(stateList[i]);
            localStateList[i] = newState;
        }

        //初期状態の設定、CurrentIndexを初期状態に合わせて変更
        currentState = localStateList[initialStateNum];
        currentState.Enter(this.gameObject,this);
    }

    void Update()
    {
        currentState.UpdateState(this.gameObject, this);
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

    public void TakeDamage(int damage, Vector3 AttackerPosition , bool isBlowAway = false)
    {
        HP -= damage;
        hpSlider.value = HP;

        if (HP < 0)
        {
            HP = 0;
            TransitionToBlownAway(blowAwayPowerOnCollision, AttackerPosition);
        }
        else if (isBlowAway)
        {
            TransitionToBlownAway(blowAwayPowerOnCollision, AttackerPosition);
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

    private void TransitionToBlownAway(float power,Vector3 attackerPosition)
    {
        SC_EnemyBlownAway blownAway = blowAwayState as SC_EnemyBlownAway;
        if (blownAway != null)
        {
            Debug.Log("吹っ飛び状態に移行\n" + "power : " + power);
            {
                currentState.Exit(this.gameObject, this);
            }

            Vector3 initialBlowDirection = (this.transform.position - attackerPosition).normalized;
            initialBlowDirection.y = 0f;
            initialBlowDirection.Normalize();

            Vector3 blowDirection = SearchForEnemyInDirection(initialBlowDirection, searchAngleThreshold);
            blowDirection.y = 0f;
            blowDirection.Normalize();

            blownAway.SetBlownAway(power, blowDirection);

            blownAway.Enter(this.gameObject, this);
            currentState = blownAway;
        }
    }

    public void ReturnFromBlownAway()
    {
        Debug.Log("吹っ飛び状態から復帰");
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
            Debug.Log("サーチで敵を見つけました : " + closestEnemy.name);

            Vector3 blowDirection = (closestEnemy.transform.position - this.transform.position).normalized;
            return blowDirection;
        }
        else
        {
            Debug.Log("サーチで敵が見つかりませんでした。");
            return direction; 
        }
    }



    //敵同士の衝突判定
    public void CheckCollisionWithOtherEnemies()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + collisionCenter, collisionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != this.gameObject && hitCollider.CompareTag("Enemy"))
            {
                Debug.Log("敵同士が衝突");
                TransitionToBlownAway(blowAwayPowerOnCollision, hitCollider.transform.position);

                SC_EnemyStatusManager otherStatusManager = hitCollider.GetComponent<SC_EnemyStatusManager>();
                if (otherStatusManager != null)
                {
                    otherStatusManager.TransitionToBlownAway(blowAwayPowerOnCollision, this.transform.position);
                }
            }
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Playerと衝突");
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
}
