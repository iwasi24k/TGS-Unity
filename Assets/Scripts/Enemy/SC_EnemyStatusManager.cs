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
    [Tooltip("初期状態のState"),SerializeField] private SC_EnemyBaceState initialState;
    [Tooltip("Stateのリスト"),SerializeField] private SC_EnemyBaceState[] stateList;
    [Tooltip("吹っ飛びのState"),SerializeField] private SC_EnemyBaceState blowAwayState;

    private SC_EnemyBaceState currentState;
    private SC_EnemyBaceState[] localStateList;
    private int currentStateIndex = 0;

    void Start()
    {
        if(hpSlider == null)
        {
            Debug.LogError("HPスライダーがアタッチされていません。");
        }
        else
        {
            hpSlider.maxValue = hpSlider.value = HP;
        }

        //全ステートのインスタンス化し、アセットを直接いじらない形に変更
        for(int i = 0; i < stateList.Length; i++)
        {
            localStateList[i] = Instantiate(stateList[i]);
        }

        //初期状態の設定、CurrentIndexを初期状態に合わせて変更
        for(int i = 0; i < localStateList.Length; i++)
        {
            if(localStateList[i].name == initialState.name)
            {
                currentState = localStateList[i];
                currentStateIndex = i;
                break;
            }
        }

    }

    void Update()
    {
        currentState.UpdateState(this);
    }

    /* : 以下、各ステータスの管理用関数。　外部から呼び出して仕様。 : */
    public int GetHP()
    {
        return HP;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        hpSlider.value = HP;

        if (HP < 0)
        {
            HP = 0;
        }
    }

    public void TransitionTo(SC_EnemyBaceState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(this);
        }
        currentState = localStateList[currentStateIndex];
        currentStateIndex = (currentStateIndex + 1) % localStateList.Length; //次のステートに移行、ループする形
        currentState.Enter(this);
    }
}
