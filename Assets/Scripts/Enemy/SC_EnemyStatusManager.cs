using Unity.VisualScripting;
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
            TransitionToBlownAway(damage , AttackerPosition);
        }
        else if (isBlowAway)
        {
            TransitionToBlownAway(damage , AttackerPosition);
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

            blownAway.Enter(this.gameObject, this);

            Vector3 blowDirection = (this.transform.position - attackerPosition).normalized;
            blowDirection.y = 0f; // 水平方向のみにする
            blownAway.SetBlownAway(power, blowDirection);
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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Playerと衝突");
        }
    }
}
